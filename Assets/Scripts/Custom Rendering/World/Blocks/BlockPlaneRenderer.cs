using System;
using System.Runtime.InteropServices;
using Player;
using UnityEngine;
using Utilities;
using World.Blocks;

namespace Custom_Rendering.World.Blocks
{
    public class BlockPlaneRenderer : MonoBehaviour
    {
        private struct BlockTypeRenderData
        {
            public int texIndex;
            public int animated; // 0 false, 1 is true
            public int numFrames;
            public float fps;
        }

        private struct BlockRenderData
        {
            public int typeID;
            public int rotated;
        }
        
        private static readonly int WidthNameID = Shader.PropertyToID("_Width");
        private static readonly int HeightNameID = Shader.PropertyToID("_Height");
        private static readonly int BlockDataBufferNameID = Shader.PropertyToID("_BlockDataBuffer");
        private static readonly int BlockTypeDataBufferNameID = Shader.PropertyToID("_BlockTypeDataBuffer");
        
        private Renderer _rend;
        private MaterialPropertyBlock _propBlock;
        [SerializeField] private int width = 1, height = 1;
        private GraphicsBuffer _blockDataBuffer;
        private GraphicsBuffer _blockTypeDataBuffer;
        private BlockManager _blockManager;
        [SerializeField] private bool ground;
        private BlockTypeRenderData[] _types;
        private BlockRenderData[] _blockData = Array.Empty<BlockRenderData>();


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rend = GetComponent<Renderer>();
            _propBlock = new MaterialPropertyBlock();

            _blockManager = App.App.Get<BlockManager>();

            //writing to types buffer
            //getting type data
            _types = new BlockTypeRenderData[_blockManager.NumTypes()];
            int i = 0;
            foreach (BlockType t in _blockManager.types)
            {
                _types[i] = new BlockTypeRenderData()
                {
                    texIndex = t.textureIndex,
                    animated = t.animated ? 1 : 0,
                    numFrames = t.animationFrameCount,
                    fps = t.animationFPS
                };
                
                i++;
            }
            
            //creating buffer
            int stride = Marshal.SizeOf(typeof(BlockTypeRenderData));
            _blockTypeDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _types.Length, stride);
            
            //Setting data
            _blockTypeDataBuffer.SetData(_types);
            
            //Sending to GPU
            _rend.GetPropertyBlock(_propBlock);
            
            _propBlock.SetBuffer(BlockTypeDataBufferNameID, _blockTypeDataBuffer);
        
            _rend.SetPropertyBlock(_propBlock);
        }
        
        private void OnDisable()
        {
            if (_blockDataBuffer != null)
            {
                _blockDataBuffer.Release();
                _blockDataBuffer = null;
            }
            
            if (_blockTypeDataBuffer != null)
            {
                _blockTypeDataBuffer.Release();
                _blockTypeDataBuffer = null;
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
                            {typeID = 0, rotated = 0}: 
                        new BlockRenderData
                            {
                                typeID = (int)b.type.ID,
                                rotated = b.rotated ? 1 : 0
                            };
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
    }
}
