using System;

namespace Features.Items
{
    [Serializable]
    public struct Item
    {
        public ItemType type;
        public string customItemName;

        public Item(ItemType type)
        {
            this.type = type;
            customItemName = type.itemName;
        }

        public Item(ItemType type, string name)
        {
            this.type = type;
            customItemName = name;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Item)) base.Equals(obj);
            return Equals((Item)obj);
        }

        public bool Equals(Item other)
        {
            return Equals(type, other.type) && customItemName == other.customItemName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(type, customItemName);
        }
    }
}