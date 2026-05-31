using System;
using Features.Inventory;
using Features.Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entities.Items
{
    public class InventoryEntity : Entity
    {
        [SerializeField] private SpriteRenderer r;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float randVelMult;
        [SerializeField] private float randAngularVelMult;
        
        public Inventory inventory;

        public void SetInventory(Inventory i)
        {
            inventory = i;
            UpdateSprites();
        }

        public void RandomizeVelocity()
        {
            Vector2 v = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Random.Range(0.5f, 1f) * randVelMult;
            float a = Random.Range(0.5f, 1f) * (Random.Range(0f, 1f) < 0.5f ? -1 : 1) * randAngularVelMult;
            Debug.Log(v + " " + a);
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
            r.sprite = inventory.slots[0].item.type.texture;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.TryGetComponent<Player.Player>(out Player.Player p)) return;
            
            inventory = p.TryAddToInventory(inventory, true);

            if (inventory.Empty()) _entityManager.RemoveEntity(this);
        }

    }
}