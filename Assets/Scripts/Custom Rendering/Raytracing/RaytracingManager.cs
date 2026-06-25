using System;
using System.Linq;
using System.Runtime.InteropServices;
using Player;
using UnityEngine;
using Utilities;
using World.Blocks;

public class RaytracingManager : MonoBehaviour
{
    private struct BlockStateRenderData
    {
        public int texIndex;
        public int animated; // 0 false, 1 is true
        public int numFrames;
        public float fps;
        public int solid; // 0 false, 1 is true
        public int height;
        public int heightTexIndex;
        public int heightAnimated;
        public int heightExtends;
    }

    private struct BlockRenderData
    {
        public int stateI;
        public int rotated;
    }
    
    [SerializeField] private MeshRenderer quad;

    [SerializeField] private ComputeShader raytracingShader;
    [SerializeField] private RenderTexture texture;
    [SerializeField] private int textureWidth = 16, textureHeight = 16;
    [SerializeField] private int worldWidth = 10, worldHeight = 10;
    [SerializeField] private bool perPixel;
    [SerializeField] private Vector3 sunPos;
    
    private GraphicsBuffer _blockDataBuffer;
    private GraphicsBuffer _blockStateDataBuffer;
    private BlockManager _blockManager;
    private BlockStateRenderData[] _states;
    private BlockRenderData[] _blockData = Array.Empty<BlockRenderData>();
    
    private int[][] _stateIndexLookup; // [typeID][stateIndex] → stateI

    private int kernel;

    private void Start()
    {
        _blockManager = App.App.Get<BlockManager>();
        kernel = raytracingShader.FindKernel("CSMain");
        
        //Texture Creation
        Material material = new Material(quad.material);
        quad.material = material;
        
        CreateTexture();
        
        //writing to types buffer (+ 1 to account for null)
        _states = new BlockStateRenderData[_blockManager.numStates + 1];
        _states[0] = new BlockStateRenderData(); //null type (everything getting set to 0 works)


        _stateIndexLookup = new int[Enum.GetValues(typeof(BlockType.BlockTypeID)).Cast<int>().Max() + 1][];
            
        int i = 1;
        foreach (BlockType t in _blockManager.types)
        {
            _stateIndexLookup[(int)t.ID] = new int[t.states.Length];
            int sI = 0;
            foreach (BlockType.BlockState s in t.states)
            {
                _states[i] = new BlockStateRenderData()
                {
                    texIndex = s.trueTextureIndex,
                    animated = s.animated ? 1 : 0,
                    numFrames = s.animationFrameCount,
                    fps = s.animationFPS,
                    solid = t.solid ? 1 : 0,
                    height = (int) s.height,
                    heightTexIndex = s.trueHeightTextureIndex,
                    heightAnimated = s.heightAnimated ? 1 : 0,
                    heightExtends = s.heightExtends
                };

                // Debug.Log(t.blockName + " " + sI + " " + index + ":" + i);
                _stateIndexLookup[(int)t.ID][sI] = i;

                i++;
                sI++;
            }
        }
            
        //creating buffer
        int stride = Marshal.SizeOf(typeof(BlockStateRenderData));
        _blockStateDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _states.Length, stride);
            
        //Setting data
        _blockStateDataBuffer.SetData(_states);
        
        //Sending to GPU
        SendOneTimeVars();
    }
    
    private void OnDestroy()
    {
        _blockDataBuffer?.Release();
        _blockStateDataBuffer?.Release();
        if (texture != null) texture.Release();
    }

    private void OnValidate()
    {
        SendOneTimeVars();
    }

    private void SendOneTimeVars()
    {
        if (_blockManager == null) return;
        kernel = raytracingShader.FindKernel("CSMain");
        if (_blockStateDataBuffer != null) raytracingShader.SetBuffer(kernel, "_BlockStateDataBuffer", _blockStateDataBuffer);
        if (_blockManager.typeCollection.blockTextures != null) raytracingShader.SetTexture(kernel, "_BlockTexs", _blockManager.typeCollection.blockTextures);
        if (_blockManager.typeCollection.blockHeightTextures != null) raytracingShader.SetTexture(kernel, "_BlockHeightTexs", _blockManager.typeCollection.blockHeightTextures);
    }

    private void CreateTexture()
    {
        texture = new RenderTexture(textureWidth, textureHeight, 0);
        texture.enableRandomWrite = true;
        texture.filterMode = FilterMode.Point;
        texture.Create();
        quad.material.SetTexture("_BaseMap", texture);
    }

    private void GetBlockInformation(Coord pCoord, bool ground)
    {
        Coord topLeft = new Coord(pCoord.x - worldWidth / 2, pCoord.y - worldHeight / 2);

        for (int y = 0; y < worldHeight; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                int i = GetIndex(x, y, ground);
                Coord world = topLeft + new Coord(x, y);

                Block b = _blockManager.GetBlock(world, ground);

                _blockData[i] = b == null ? 
                    new BlockRenderData
                        {stateI = 0, rotated = 0}: 
                    new BlockRenderData
                    {
                        stateI = GetStateIndex(b.type.ID, b.state),
                        rotated = b.rotated ? 1 : 0
                    };
                
                // Debug.Log(b.type.blockName + " " + _blockData[i].typeI);
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        //making so that it doesnt break!
        if (worldWidth < 1) worldWidth = 1;
        if (worldHeight < 1) worldHeight = 1;
        
        //Setting pizel size
        if (perPixel)
        {
            textureWidth = worldWidth * 8;
            textureHeight = worldHeight * 8;
        }
        
        //ensuring texture exists and is updated
        if (texture == null || texture.width != textureWidth || texture.height != textureHeight)
        {
            CreateTexture();
        }
        
        Transform trans = transform;
            
        //Following Player
        Player.Player p = App.App.Get<PlayerManager>().player;
        Vector3 pPos = p.transform.position;
        Coord pCoord = new Coord(pPos);
        trans.position = new Vector3(pCoord.x + (worldWidth % 2 == 1 ? .5f : 0), pCoord.y + (worldHeight % 2 == 1 ? .5f : 0), 0);
        
        //Adjusting Size
        trans.localScale = new Vector3(worldWidth, worldHeight);
        
        //creating buffer and array
        int count = worldWidth * worldHeight * 2;
        
        if (_blockDataBuffer == null || _blockDataBuffer.count != count)
        {
            _blockData = new BlockRenderData[count];
            _blockDataBuffer?.Release(); 
    
            int stride = Marshal.SizeOf(typeof(BlockRenderData));
            _blockDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, stride);
        }
        
        //Getting Block Information
        Coord topLeft = new Coord(pCoord.x - worldWidth / 2, pCoord.y - worldHeight / 2);
        GetBlockInformation(pCoord, true);
        GetBlockInformation(pCoord, false);
        _blockDataBuffer.SetData(_blockData);

        //Passing data to shader
        raytracingShader.SetTexture(kernel, "Result", texture);
        raytracingShader.SetInt("textureWidth", textureWidth);
        raytracingShader.SetInt("textureHeight", textureHeight);
        raytracingShader.SetInt("worldWidth", worldWidth);
        raytracingShader.SetInt("worldHeight", worldHeight);
        raytracingShader.SetFloat("Time", Time.time);
        // raytracingShader.SetVector("Sun", new Vector4(pPos.x - topLeft.x + sunPos.x, pPos.y - topLeft.y + sunPos.y, sunPos.z, 0));
        raytracingShader.SetVector("Sun", sunPos);
        // raytracingShader.SetVector("Sun", new Vector4(Mathf.Sin(Time.time / 10) * 10, 5, Mathf.Sin(Time.time / 10) * 20));
        raytracingShader.SetBuffer(kernel, "_BlockDataBuffer", _blockDataBuffer);
        raytracingShader.Dispatch(kernel, texture.width / 8, texture.height / 8, 1);
    }

    private int GetIndex(int x, int y, bool ground)
    {
        return y * worldWidth + x + (ground ? 0 : worldWidth * worldHeight);
    }
    
    private int GetStateIndex(BlockType.BlockTypeID id, int state)
    {
        return _stateIndexLookup[(int)id][state];;
    }
}
