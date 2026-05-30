using System;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace World.Blocks.CustomBlocks
{
    public class GrassBlock : Block
    {
        private GrassBlockData gd;

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
        
        private void LoadData(BlockData d)
        {
            gd = (GrassBlockData) d;
        }
        
        private BlockData GetBlockData()
        {
            return gd;
        }
        
        public record GrassBlockData() : BlockData;
    }
}