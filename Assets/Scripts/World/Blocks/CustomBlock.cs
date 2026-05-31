namespace World.Blocks.CustomBlocks
{
    public abstract class CustomBlock
    {
        public BlockManager _blockManager;
        public Block block;

        public CustomBlock(Block b)
        {
            block = b;
            _blockManager = b._blockManager;
        }
            
        //Override this if there is custom block actions
        public virtual void Update() { }
        

        // Override this if there is custom block data
        public virtual BlockData GetBlockData()
        {
            return new BlockData();
        }
        
        // Override this if there is custom block data
        public virtual void LoadData(BlockData d)
        {}

        // Override this if there is custom block data
        public record BlockData();
    }
}