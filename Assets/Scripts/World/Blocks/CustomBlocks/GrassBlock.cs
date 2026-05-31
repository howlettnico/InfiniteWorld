using System;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace World.Blocks.CustomBlocks
{
    public class GrassBlock : CustomBlock
    {
        private GrassBlockData gd;
        
        public GrassBlock(Block b) : base(b) { }
        
        public override void Update()
        {
            if (block.inGround && _blockManager.GetBlock(block.pos, false).type.solid)
            {
                _blockManager.SetBlock(block.pos, _blockManager.NewBlock(BlockType.BlockTypeID.Dirt, block.pos, true, false, true), true);
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