using System;
using Features.Items;

namespace Features.Inventory
{
    [Serializable]
    public class Slot
    {
        public bool empty = true;
        public Item item;
        public int count;

        public Slot()
        {
            SetEmpty();
        }

        public void SetEmpty()
        {
            ItemManager m = App.App.Get<ItemManager>();
            empty = true;
            item = new Item(m.GetItemType(ItemType.ItemTypeID.Null));
            count = 0;
        }

        public bool TryAddMatching(Item i)
        {
            if (empty) return false;
            if (!item.Equals(i)) return false;
            if (count >= item.type.stackSize) return false;
            count++;
            return true;
        }
        
        public bool TryAdd(Item i)
        {
            if (empty)
            {
                item = i;
                count = 1;
                empty = false;
                return true;
            }
            if (!item.Equals(i)) return false;
            if (count >= item.type.stackSize) return false;
            count++;
            return true;
        }

        public bool TryRemove(Item i)
        {
            if (empty) return false;
            if (!item.Equals(i)) return false;
            count--;
            if (count == 0) SetEmpty();
            return true;
        }
    }
}