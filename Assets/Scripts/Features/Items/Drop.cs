using System;
using UnityEngine;

namespace Features.Items
{
    [Serializable]
    public class Drop
    {
        public ItemType.ItemTypeID ID;
        [Range(0, 1)] public float dropChance;
        public int minDropAmount;
        public int maxDropAmount;

    }
}