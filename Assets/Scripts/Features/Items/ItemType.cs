using UnityEngine;
using World.Blocks;

namespace Features.Items
{
    [CreateAssetMenu(fileName = "New Item Type", menuName = "Item Type")]
    public class ItemType : ScriptableObject
    {
        public enum ItemTypeID
        {
            Null = 0,
            OakLog = 1
        }

        public ItemTypeID ID;
        public string itemName;
        public int stackSize;
        public bool placeable;
        public BlockType.BlockTypeID placedBlockID;
    }
}