using Utilities;
using World.Blocks.CustomBlocks;

namespace World.Blocks
{
    public record BlockRecord(BlockType.BlockTypeID ID, Coord pos, bool rotated, int state, CustomBlock.BlockData data);
}