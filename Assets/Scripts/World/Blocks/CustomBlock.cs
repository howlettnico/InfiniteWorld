namespace World.Blocks.CustomBlocks
{
    public abstract class CustomBlock
    {
        // Override this if there is custom block data
        public abstract record BlockData;
        
        // Constant Variables
        public BlockManager _blockManager;
        public Block block;
        
        //Updatable Variables
        public virtual bool needsUpdate => false; //override this if the block needs to be updated

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
            return null;
        }
        
        // Override this if there is custom block data
        public virtual void LoadData(BlockData d)
        {}
    }
}