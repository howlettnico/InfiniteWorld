using Utilities;
using World.Blocks;

namespace World.Chunk
{
    public record ChunkRecord(Coord pos, BlockRecord[] editedGroundBlocks, BlockRecord[] editedAboveGroundBlocks);
}