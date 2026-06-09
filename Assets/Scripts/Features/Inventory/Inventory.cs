using System;
using System.Collections.Generic;
using Features.Items;
using UnityEngine;

namespace Features.Inventory
{
    [Serializable]
    public struct Inventory
    {
        public int NumSlots { get; private set; }
        public Slot[] slots;
        public bool expandable;

        public Inventory(int numSlots, bool expandable = false)
        {
            NumSlots = numSlots;
            slots = new Slot[numSlots];
            for (int i = 0; i < numSlots; i++)
            {
                slots[i] = new Slot();
            }
            this.expandable = expandable;
        }

        public bool TryAdd(Item item)
        {
            //finding matching slot
            foreach (Slot s in slots)
            {
                if (s.TryAddMatching(item)) return true;
            }
            
            //finding empty slots
            foreach (Slot s in slots)
            {
                if (s.TryAdd(item)) return true;
            }

            if (expandable)
            {
                List<Slot> newSlots = new List<Slot>(slots);
                Slot newSlot = new Slot();
                newSlot.TryAdd(item);
                newSlots.Add(newSlot);
                slots = newSlots.ToArray();
                NumSlots = slots.Length;
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Adds the items it can to the inventoru
        /// </summary>
        /// <param name="item"></param>
        /// <param name="count"></param>
        /// <returns>the number of items that it was unable to add</returns>
        public int TryAdd(Item item, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (TryAdd(item)) continue;
                return count - i;
            }

            return 0;
        }

        /// <summary>
        /// Adds all items in an inventory to this inventory
        /// </summary>
        /// <param name="inventory"></param>
        /// <returns>An inventory containing all items that it was unable to add</returns>
        public Inventory TryAdd(Inventory inventory)
        {
            Inventory remainingInventory = new Inventory(0, true);
            foreach (Slot s in inventory.slots)
            {
                int rem = TryAdd(s.item, s.count);
                if (rem > 0) remainingInventory.TryAdd(s.item, rem);
            }

            return remainingInventory;
        }

        public bool Empty()
        {
            foreach (Slot s in slots)
            {
                if (!s.empty) return false;
            }

            return true;
        }
    }
}