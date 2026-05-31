using Utilities;

namespace World.Blocks.CustomBlocks
{
    public class ChestBlock : CustomBlock
    {
        public enum ChestBlockState
        {
            Closed = 0,
            Open = 1
        }

        public ChestBlock(Block b) : base(b)
        { }
    }
}