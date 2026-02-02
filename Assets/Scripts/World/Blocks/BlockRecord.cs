using Utilities;

namespace World.Blocks
{
    public record BlockRecord(BlockType.BlockTypeID ID, Coord pos, bool rotated, Block.BlockData data);
}