using System;
using Features.Inventory;
using Features.Items;
using UnityEngine;
using Utilities;
using World.Blocks.CustomBlocks;
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
        public bool save;
        public bool rotated;
        public bool custom;
        public CustomBlock blockScript;

        public Block(BlockType type, Coord pos, bool inGround, bool rotated, bool save, CustomBlock.BlockData d = null)
        {
            _itemManager = App.App.Get<ItemManager>();
            _blockManager = App.App.Get<BlockManager>();
            
            this.type = type;
            this.pos = pos;
            this.inGround = inGround;
            this.save = save;
            this.rotated = rotated;
            custom = type.blockScript != BlockType.BlockScript.None;

            SetScript();
            
            if (type.customData) blockScript.LoadData(d);
        }

        public Block(BlockRecord r, bool inGround)
        {;
            _itemManager = App.App.Get<ItemManager>();
            _blockManager = App.App.Get<BlockManager>();

            type = _blockManager.GetBlockType(r.ID);
            pos = r.pos;
            this.inGround = inGround;
            save = true;
            rotated = r.rotated;
            custom = type.blockScript != BlockType.BlockScript.None;

            SetScript();
        }

        private void SetScript()
        {
            if (!custom) return;

            blockScript = type.blockScript switch
            {
                BlockType.BlockScript.Grass  => new GrassBlock(this),
                BlockType.BlockScript.Dirt  => new DirtBlock(this),
                _ => throw new Exception($"Custom Script {type.blockScript} is not assigned")
            };
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
            CustomBlock.BlockData data = custom ? blockScript.GetBlockData() : null;
            return new BlockRecord(type.ID, pos, rotated, data);
        }
    }
}
