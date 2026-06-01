using System;
using System.Collections.Generic;
using Features.Inventory;
using Features.Items;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entities.Items
{
    public class InventoryEntity : Entity
    {
        [SerializeField] private GameObject spriteHolder;
        private List<GameObject> slotHolders = new List<GameObject>();
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float randVelMult;
        [SerializeField] private float randAngularVelMult;
        [SerializeField] private float slotOffset;
        [SerializeField] private float itemOffset;
        [SerializeField] private int maxStackSize = 6;
        
        public Inventory inventory;

        public void SetInventory(Inventory i)
        {
            inventory = i;
            i.expandable = true;
            UpdateSprites();
        }

        public void RandomizeVelocity()
        {
            Vector2 v = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Random.Range(0.5f, 1f) * randVelMult;
            float a = Random.Range(0.5f, 1f) * (Random.Range(0f, 1f) < 0.5f ? -1 : 1) * randAngularVelMult;
            // Debug.Log(v + " " + a);
            SetVelocity(v);
            SetAngularVelocity(a);
        }

        public void SetVelocity(Vector2 v)
        {
            rb.linearVelocity = v;
        }

        public void SetAngularVelocity(float v)
        {
            rb.angularVelocity = v;
        }

        private void UpdateSprites()
        {
            // Destroy(spriteHolder);

            // spriteHolder = new GameObject("Sprite Holder");
            // spriteHolder.transform.SetParent(transform);
            // spriteHolder.transform.position = transform.position;
            
            for (int sI = 0; sI < inventory.NumSlots; sI++)
            {
                Slot s = inventory.slots[sI];
                
                Vector3 offset = sI == 0 ? 
                    new Vector3() :
                    new Vector3(Random.Range(-slotOffset, slotOffset), Random.Range(-slotOffset, slotOffset));

                GameObject so;

                if (sI >= slotHolders.Count)
                {
                    so = new GameObject("Slot Object");
                    so.transform.SetParent(spriteHolder.transform);
                    so.transform.position = spriteHolder.transform.position + offset;
                    slotHolders.Add(so);
                }
                else
                {
                    so = slotHolders[sI];
                }

                for (int c = 0; c < so.transform.childCount; c++)
                {
                    Destroy(so.transform.GetChild(c).gameObject);
                }


                int count = Mathf.Min(s.count, maxStackSize);
                for (int i = 0; i < count; i++)
                {
                    Vector3 offset2 = s.count == 1 ? 
                        new Vector3() : 
                        new Vector3(
                            Mathf.Lerp(-itemOffset, itemOffset, (float)i / (count - 1)), 
                            Mathf.Lerp(itemOffset, -itemOffset, (float)i / (count - 1)));
                    GameObject sp = new GameObject("Sprite");
                    sp.transform.SetParent(so.transform);
                    sp.transform.position = so.transform.position + offset2;
                    sp.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                    SpriteRenderer r = sp.AddComponent<SpriteRenderer>();
                    r.sprite = s.item.type.texture;
                    r.sortingOrder = i;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent<Player.Player>(out Player.Player p))
            {
                inventory = p.TryAddToInventory(inventory, true);

                if (inventory.Empty()) _entityManager.RemoveEntity(this);
            }else if (other.gameObject.TryGetComponent<InventoryEntity>(out InventoryEntity e))
            {
                if (e.inventory.Empty()) return;
                Inventory i = e.inventory.TryAdd(inventory);
                if (!i.Empty()) Debug.LogError("Failed to combine inventories");
                
                e.UpdateSprites();

                inventory = i;
                
                _entityManager.RemoveEntity(this);
            }
        }

    }
}