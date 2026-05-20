using UnityEngine;
using Utilities;

namespace World.Blocks.CustomBlocks
{
    public class GrassBlock : Block
    {

        public GrassBlock(BlockType type, Coord pos, bool inGround, bool rotated, bool record, BlockData b) : base(type, pos, inGround, rotated,
            record, b)
        {
        }
        public GrassBlock(BlockType type, Coord pos, bool inGround, bool rotated, bool record) : base(type, pos, inGround, rotated, record) 
        {
        }

        public GrassBlock(BlockRecord r, bool inGround) : base(r, inGround) {}
        
        public override void Update()
        {
            if (inGround && _blockManager.GetBlock(pos, false).type.solid)
            {
                _blockManager.SetBlock(pos, _blockManager.NewBlock(BlockType.BlockTypeID.Dirt, pos, true, false, true), true);
            }
        }

        // private void LoadData(BlockData d)
        // {
        //     Debug.Log("Grass!");
        //     GrassBlockData gd = (GrassBlockData)d;
        // }
        //
        // private BlockData GetBlockData()
        // {
        //     return new GrassBlockData();
        // }
        //
        // public record GrassBlockData() : BlockData;
    }
}