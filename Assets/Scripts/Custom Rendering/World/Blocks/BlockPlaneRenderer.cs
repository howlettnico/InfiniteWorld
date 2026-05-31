using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Player;
using UnityEngine;
using Utilities;
using World.Blocks;

namespace Custom_Rendering.World.Blocks
{
    public class BlockPlaneRenderer : MonoBehaviour
    {
        private struct BlockStateRenderData
        {
            public int texIndex;
            public int animated; // 0 false, 1 is true
            public int numFrames;
            public float fps;
        }

        private struct BlockRenderData
        {
            public int stateI;
            public int rotated;
        }
        
        private static readonly int WidthNameID = Shader.PropertyToID("_Width");
        private static readonly int HeightNameID = Shader.PropertyToID("_Height");
        private static readonly int BlockDataBufferNameID = Shader.PropertyToID("_BlockDataBuffer");
        private static readonly int BlockStateDataBufferNameID = Shader.PropertyToID("_BlockStateDataBuffer");
        
        private Renderer _rend;
        private MaterialPropertyBlock _propBlock;
        [SerializeField] private int width = 1, height = 1;
        private GraphicsBuffer _blockDataBuffer;
        private GraphicsBuffer _blockStateDataBuffer;
        private BlockManager _blockManager;
        [SerializeField] private bool ground;
        private BlockStateRenderData[] _states;
        private BlockRenderData[] _blockData = Array.Empty<BlockRenderData>();

        private Dictionary<long, int> typeIDAndStateToStateIndex = new Dictionary<long, int>();


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _blockManager = App.App.Get<BlockManager>();
            
            _rend = GetComponent<Renderer>();
            _propBlock = new MaterialPropertyBlock();

            //writing to types buffer (+ 1 to account for null)
            _states = new BlockStateRenderData[_blockManager.numStates + 1];
            _states[0] = new BlockStateRenderData(); //null type (everything getting set to 0 works)
            
            int i = 1;
            foreach (BlockType t in _blockManager.types)
            {
                int sI = 0;
                foreach (BlockType.BlockState s in t.states)
                {
                    _states[i] = new BlockStateRenderData()
                    {
                        texIndex = s.trueTextureIndex,
                        animated = s.animated ? 1 : 0,
                        numFrames = s.animationFrameCount,
                        fps = s.animationFPS
                    };

                    typeIDAndStateToStateIndex.Add(GetKey(t.ID, sI), i);
                    typeIDAndStateToStateIndex.TryGetValue(GetKey(t.ID, sI), out int index);
                    // Debug.Log(t.blockName + " " + sI + " " + index + ":" + i);

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
            _rend.GetPropertyBlock(_propBlock);
            
            _propBlock.SetBuffer(BlockStateDataBufferNameID, _blockStateDataBuffer);
        
            _rend.SetPropertyBlock(_propBlock);
        }
        
        private void OnDisable()
        {
            if (_blockDataBuffer != null)
            {
                _blockDataBuffer.Release();
                _blockDataBuffer = null;
            }
            
            if (_blockStateDataBuffer != null)
            {
                _blockStateDataBuffer.Release();
                _blockStateDataBuffer = null;
            }
        }

        // Update is called once per frame
        void Update()
        {
            //making so that it doesnt break!
            if (width < 1) width = 1;
            if (height < 1) height = 1;
            
            Transform trans = transform;
            
            //Following Player
            Player.Player p = App.App.Get<PlayerManager>().player;
            Vector3 pPos = p.transform.position;
            Coord pCoord = new Coord(pPos);
            trans.position = new Vector3(pCoord.x + (width % 2 == 1 ? .5f : 0), pCoord.y + (height % 2 == 1 ? .5f : 0), 0);
            
            //Adjusting Size
            trans.localScale = new Vector3(width, height);
            
            //creating buffer and array
            int count = width * height;
            
            if (_blockDataBuffer == null || _blockDataBuffer.count != count)
            {
                _blockData = new BlockRenderData[count];
                _blockDataBuffer?.Release(); 
        
                int stride = Marshal.SizeOf(typeof(BlockRenderData));
                _blockDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, stride);
            }
            
            //Getting Block Information
            Coord topLeft = new Coord(pCoord.x - width / 2, pCoord.y - height / 2);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int i = y * width + x;
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
            _blockDataBuffer.SetData(_blockData);

            //Passing data to shader
            _rend.GetPropertyBlock(_propBlock);
            
            _propBlock.SetFloat(WidthNameID, width);
            _propBlock.SetFloat(HeightNameID, height);
            _propBlock.SetBuffer(BlockDataBufferNameID, _blockDataBuffer);
        
            _rend.SetPropertyBlock(_propBlock);
            
        }

        private int GetStateIndex(BlockType.BlockTypeID id, int state)
        {
            if (!typeIDAndStateToStateIndex.TryGetValue(GetKey(id, state), out int index)) Debug.LogError("ID: " + id + " with State" + state + " does not exist");

            return index;
        }

        private long GetKey(BlockType.BlockTypeID id, int state)
        {
            Coord c = new Coord((int)id, state);
            
            return c.PackIntoLong();
        }
    }
}
