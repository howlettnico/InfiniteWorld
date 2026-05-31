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
        public int state;
        public CustomBlock blockScript;

        public Block(BlockType type, Coord pos, bool inGround, bool rotated, int state, bool save, CustomBlock.BlockData d = null)
        {
            _itemManager = App.App.Get<ItemManager>();
            _blockManager = App.App.Get<BlockManager>();
            
            this.type = type;
            this.pos = pos;
            this.inGround = inGround;
            this.save = save;
            this.rotated = rotated;
            if (this.state >= type.states.Length) Debug.LogError("State " + state + " does not exist for type " + type.name);
            this.state = state;

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
            if (state >= type.states.Length) Debug.LogError("State " + state + " does not exist for type " + type.name);
            state = r.state;

            SetScript();
        }

        private void SetScript()
        {
            if (!type.customScript) return;

            blockScript = type.blockScript switch
            {
                BlockType.BlockScript.Grass  => new GrassBlock(this),
                BlockType.BlockScript.Dirt  => new DirtBlock(this),
                BlockType.BlockScript.Chest  => new ChestBlock(this),
                _ => throw new Exception($"Custom Script {type.blockScript} is not assigned")
            };
        }

        public void Break()
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

            if (!i.Empty()) _itemManager.CreateInventoryEntity(i, new Vector2(0.5f, 0.5f) + (Vector2) pos, true);
        }

        public BlockRecord GetRecord()
        {
            CustomBlock.BlockData data = type.customScript ? blockScript.GetBlockData() : null;
            return new BlockRecord(type.ID, pos, rotated, (int) state, data);
        }
    }
}
