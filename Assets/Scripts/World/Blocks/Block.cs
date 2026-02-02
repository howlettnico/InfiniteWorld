using System;
using Features.Inventory;
using Features.Items;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace World.Blocks
{
    public class Block
    {
        
        // ***** Managers ******
        public ItemManager _itemManager;
        public BlockManager _blockManager;

        // ****** Actual *****
        public BlockType type;
        public Coord pos;
        public bool inGround;
        public bool record;
        public bool rotated;

        public Block(BlockType type, Coord pos, bool inGround, bool rotated, bool record, BlockData d)
        {
            _itemManager = App.App.Get<ItemManager>();
            _blockManager = App.App.Get<BlockManager>();
            
            this.type = type;
            this.pos = pos;
            this.inGround = inGround;
            this.record = record;
            this.rotated = rotated;
            
            LoadData(d);
        }
        
        public Block(BlockType type, Coord pos, bool inGround, bool rotated, bool record)
        {
            _itemManager = App.App.Get<ItemManager>();
            _blockManager = App.App.Get<BlockManager>();
            
            this.type = type;
            this.pos = pos;
            this.inGround = inGround;
            this.record = record;
            this.rotated = rotated;
        }

        public Block(BlockRecord r, bool inGround)
        {;
            _itemManager = App.App.Get<ItemManager>();
            _blockManager = App.App.Get<BlockManager>();
            
            this.pos = pos;
            this.inGround = inGround;
            record = true;
            rotated = r.rotated;
        }

        public Inventory Break()
        {
            Inventory i = new Inventory(0, true);
            foreach (Drop d in type.drops)
            {
                if (Random.Range(0f, 1f) > d.dropChance) continue;
                int num = d.minDropAmount == d.maxDropAmount
                    ? d.minDropAmount
                    : Random.Range(d.minDropAmount, d.maxDropAmount + 1);

                Item item = new Item(_itemManager.GetItemType(d.ID));
                int rem = i.TryAdd(item, num);

                if (rem > 0)
                    throw new Exception(
                        $"Failed to add all {item.customItemName}s to expandable inventory: THIS SHOULD BE IMPOSSIBLE");
            }

            return i;
        }

        public BlockRecord GetRecord()
        {
            return new BlockRecord(type.ID, pos, rotated, GetBlockData());
        }

        //Override this if there is custom block actions
        public virtual void Update()
        {
            
        }

        // Override this if there is custom block data
        public virtual BlockData GetBlockData()
        {
            // Debug.Log(type);
            return new BlockData();
        }
        
        // Override this if there is custom block data
        public virtual void LoadData(BlockData d)
        {}

        // Override this if there is custom block data
        public record BlockData();
    }
}
