using System.Collections;
using System.Collections.Generic;
using App;
using UnityEngine;

namespace Entities
{
    public class EntityManager : AppModule
    {
        public List<Entity> loadedEntities;

        public Entity AddEntity(GameObject entityPrefab, Vector2 pos)
        {
            GameObject o = Instantiate(entityPrefab, pos, Quaternion.identity, transform);
            if(!o.TryGetComponent<Entity>(out Entity entity)) Debug.LogError("Attempted to add entity at (" + pos.x + ", " + pos.y + "_ from prefab without an entity: " + entityPrefab);
            loadedEntities.Add(entity);
            entity._entityManager = this;
            
            return entity;
        }

        public void RemoveEntity(Entity e)
        {
            if (!loadedEntities.Remove(e)) Debug.LogError("Tried to remove entity that was not loaded");
            
            Destroy(e.gameObject);
        }
    }
}