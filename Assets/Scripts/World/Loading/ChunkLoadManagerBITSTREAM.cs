using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Player;
using UnityEngine;
using Utilities;
using Utilities.Bit_Streams;
using World.Blocks;

namespace Custom_Rendering.World.Blocks
{
    public class BlockPlaneRendererBITSTREAM : MonoBehaviour
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
        private static readonly int BlockDataSBSBufferNameID = Shader.PropertyToID("_BlockDataSBS");
        private static readonly int BlockDataSBSLengthsBufferNameID = Shader.PropertyToID("_BlockDataSBSLengths");
        private static readonly int BlockDataSBSSegmentLength = Shader.PropertyToID("_BlockDataSBSSegmentLength");
        private static readonly int BlockDataSBSNumLength = Shader.PropertyToID("_BlockDataSBSNumLength");
        private static readonly int BlockStateDataBufferNameID = Shader.PropertyToID("_BlockStateDataBuffer");
        
        private Renderer _rend;
        private MaterialPropertyBlock _propBlock;
        [SerializeField] private int width = 1, height = 1;
        private GraphicsBuffer _blockDataSBSBuffer;
        private GraphicsBuffer _blockDataSBSLengthsBuffer;
        private GraphicsBuffer _blockStateDataBuffer;
        private BlockManager _blockManager;
        [SerializeField] private bool ground;
        private BlockStateRenderData[] _states;
        private SmartBitStream _blockDataSBS;
        private int[] _blockData = new int[0];

        private Dictionary<long, int> typeIDAndStateToStateIndex = new Dictionary<long, int>();


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _blockManager = App.App.Get<BlockManager>();
            
            _rend = GetComponent<Renderer>();
            _propBlock = new MaterialPropertyBlock();
            
            //Initiating _blockDataStream
            int[] lengths = new[] { BitStream.NumBits(_blockManager.numStates + 1), 1};
            // ^BlockRenderData: int stateI, bool rotated
            _blockDataSBS = new SmartBitStream(lengths);

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
                    Debug.Log(t.blockName + " " + sI + " " + index + ":" + i + " TI: " + _states[i].texIndex);

                    i++;
                    sI++;
                }
            }
            
            // Debug.Log(_states.Length + " " + BitStream.NumBits(_blockManager.numStates + 1) + " " + (Mathf.Pow(2, BitStream.NumBits(_blockManager.numStates + 1))));
            
            //creating buffers
            int stride = Marshal.SizeOf(typeof(BlockStateRenderData));
            _blockStateDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _states.Length, stride);
            int stride1 = Marshal.SizeOf(typeof(int));
            _blockDataSBSLengthsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, lengths.Length, stride1);
            
            
            
            //Setting datas
            _blockStateDataBuffer.SetData(_states);
            _blockDataSBSLengthsBuffer.SetData(lengths);
            
            //Sending to GPU
            _rend.GetPropertyBlock(_propBlock);
            
            _propBlock.SetBuffer(BlockStateDataBufferNameID, _blockStateDataBuffer);
            _propBlock.SetBuffer(BlockDataSBSLengthsBufferNameID, _blockDataSBSLengthsBuffer);
            _propBlock.SetInt(BlockDataSBSSegmentLength, _blockDataSBS.GetSegmentLength());
            _propBlock.SetInt(BlockDataSBSNumLength, lengths.Length);

            _rend.SetPropertyBlock(_propBlock);
            
            Update();
        }
        
        private void OnDisable()
        {
            if (_blockDataSBSBuffer != null)
            {
                _blockDataSBSBuffer.Release();
                _blockDataSBSBuffer = null;
            }
            
            if (_blockStateDataBuffer != null)
            {
                _blockStateDataBuffer.Release();
                _blockStateDataBuffer = null;
            }

            if (_blockDataSBSLengthsBuffer != null)
            {
                _blockDataSBSLengthsBuffer?.Release();
                _blockDataSBSLengthsBuffer = null;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Debug.Log("NEW UPDATE ----------------------------");
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

            //Getting Block Information
            _blockDataSBS.Clear();
            
            Coord topLeft = new Coord(pCoord.x - width / 2, pCoord.y - height / 2);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    Coord world = topLeft + new Coord(x, y);

                    Block b = _blockManager.GetBlock(world, ground);

                    // Debug.Log("INDEX: " + i + " (" + x + ", " + y + ")");
                    if (b == null)
                    {
                        _blockDataSBS.AddNext(0);//state index
                        _blockDataSBS.AddNext(0);//rotated bool
                        // Debug.Log("ID: NULL");
                    }
                    else
                    {
                        _blockDataSBS.AddNext(GetStateIndex(b.type.ID, b.state));//state index
                        _blockDataSBS.AddNext(b.rotated ? 1 : 0);//rotated bool
                        // Debug.Log(b.type.blockName + " ID: " + b.type.ID + " State: " + b.state + " Index: " + GetStateIndex(b.type.ID, b.state) + " Rot: " + b.rotated + " " + (b.rotated ? 1 : 0));
                    }
                    
                    // _blockDataSBS.GoToSegment(i);
                    // int state = _blockDataSBS.ReadNext();
                    // int rotated = _blockDataSBS.ReadNext();
                    // Debug.Log("State: " + state + " Rotated: " + rotated);

                }
            }
            
            //creating buffer and array
            int count = _blockDataSBS.GetCount();
            
            if (_blockDataSBSBuffer == null || _blockDataSBSBuffer.count != count)
            {
                _blockData = new int[count];
                _blockDataSBSBuffer?.Release(); 
        
                int stride = Marshal.SizeOf(typeof(int));
                _blockDataSBSBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, stride);
            }
            
            //Exporting stream to buffer and writing data
            _blockDataSBS.PackAndExport(_blockData);
            // Debug.Log((Marshal.SizeOf(typeof(BlockRenderData)) * width * height) + " vs " + (_blockData.Length * Marshal.SizeOf(typeof(int))) + " \n" + "\n\n");
            _blockDataSBSBuffer.SetData(_blockData);

            //Passing data to shader
            _rend.GetPropertyBlock(_propBlock);
            
            _propBlock.SetFloat(WidthNameID, width);
            _propBlock.SetFloat(HeightNameID, height);
            _propBlock.SetBuffer(BlockDataSBSBufferNameID, _blockDataSBSBuffer);
        
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
