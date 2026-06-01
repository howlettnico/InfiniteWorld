using System.Collections.Generic;
using App;
using Entities;
using Entities.Items;
using UnityEngine;
using UnityEngine.Serialization;
using World.Loading;

namespace Features.Items
{
    public class ItemManager : AppModule
    {
        private EntityManager _entityManager;
        
        [SerializeField] public ItemTypeCollection typeCollection;
        [HideInInspector] public ItemType[] types;
        private Dictionary<int, int> typeIDToIndex;

        [SerializeField] private GameObject inventoryEntityPrefab;

        private void Start()
        {
            _entityManager = App.App.Get<EntityManager>();
            
            types = typeCollection.types;

            typeIDToIndex = new Dictionary<int, int>();
            for (int i = 0; i < types.Length; i++)
            {
                // Debug.Log(types[i].ID + " "+ (int) types[i].ID + " " + i);
                typeIDToIndex.Add((int) types[i].ID, i);
            }
        }

        public ItemType GetItemType(ItemType.ItemTypeID id)
        {
            return types[GetIndex(id)];
        }
        
        public int GetIndex(ItemType.ItemTypeID id)
        {
            if (!typeIDToIndex.TryGetValue((int)id, out int index)) Debug.LogError("ItemType ID " + id + " is not in mapping dictionary");

            return index;
        }

        public void CreateInventoryEntity(Inventory.Inventory inventory, Vector2 pos, bool randomVel, Vector2 vel = new Vector2())
        {
            InventoryEntity e = (InventoryEntity) _entityManager.AddEntity(inventoryEntityPrefab, pos);

            e.SetInventory(inventory);
            if (randomVel) e.RandomizeVelocity();
            else e.SetVelocity(vel);
        }
    }
}