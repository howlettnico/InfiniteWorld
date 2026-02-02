using App;
using UnityEngine;

namespace Features.Items
{
    public class ItemManager : AppModule
    {
        [SerializeField]
        private ItemType[] types;


        public ItemType GetItemType(ItemType.ItemTypeID id)
        {
            return types[(int) id];
        }
    }
}