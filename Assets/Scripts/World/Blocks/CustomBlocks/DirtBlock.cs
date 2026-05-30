using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace World.Blocks.CustomBlocks
{
    public class DirtBlock : Block
    {
        public DirtBlock(BlockType type, Coord pos, bool inGround, bool rotated, bool record, BlockData d) : base(type, pos, inGround, rotated, record, d)
        {
        }

        public DirtBlock(BlockType type, Coord pos, bool inGround, bool rotated, bool record) : base(type, pos, inGround, rotated, record)
        {
        }

        public DirtBlock(BlockRecord r, bool inGround) : base(r, inGround)
        {
        }

        public override void Update()
        {
            if (!inGround) return;
            
            Block up = _blockManager.GetBlock(pos + new Coord(0, 1), true);
            Block down = _blockManager.GetBlock(pos + new Coord(0, -1), true);
            Block left = _blockManager.GetBlock(pos + new Coord(-1, 0), true);
            Block right = _blockManager.GetBlock(pos + new Coord(1, 0), true);

            int neighbors = up?.type.ID == BlockType.BlockTypeID.Grass ? 1 : 0 +
                            down?.type.ID == BlockType.BlockTypeID.Grass ? 1 : 0 +
                            left?.type.ID == BlockType.BlockTypeID.Grass ? 1 : 0 +
                            right?.type.ID == BlockType.BlockTypeID.Grass ? 1 : 0;
            
            if (Random.Range(0, 1000) < neighbors)
            {
                _blockManager.SetBlock(pos, _blockManager.NewBlock(BlockType.BlockTypeID.Grass, pos, true, false, true), true);
            }
        }
    }
}