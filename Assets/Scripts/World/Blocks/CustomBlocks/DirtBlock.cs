using NUnit.Framework.Constraints;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace World.Blocks.CustomBlocks
{
    public class DirtBlock : CustomBlock
    {
        public override bool needsUpdate => true;
        
        public DirtBlock(Block b) : base(b) { }
        
        public override void Update()
        {
            if (!block.inGround) return;

            Block above = _blockManager.GetBlock(block.pos, false);

            if (above.type.solid) return;
            
            Block up = _blockManager.GetBlock(block.pos + new Coord(0, 1), true);
            Block down = _blockManager.GetBlock(block.pos + new Coord(0, -1), true);
            Block left = _blockManager.GetBlock(block.pos + new Coord(-1, 0), true);
            Block right = _blockManager.GetBlock(block.pos + new Coord(1, 0), true);

            int neighbors = up?.type.ID == BlockType.BlockTypeID.Grass ? 1 : 0 +
                            down?.type.ID == BlockType.BlockTypeID.Grass ? 1 : 0 +
                            left?.type.ID == BlockType.BlockTypeID.Grass ? 1 : 0 +
                            right?.type.ID == BlockType.BlockTypeID.Grass ? 1 : 0;
            
            if (Random.Range(0, 10000) < neighbors)
            {
                _blockManager.SetBlock(block.pos, _blockManager.NewBlock(BlockType.BlockTypeID.Grass, block.pos, true, false, 0, true), true);
            }
        }
    }
}