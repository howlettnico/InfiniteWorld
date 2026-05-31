using UnityEngine;

namespace Entities
{
    public class Entity : MonoBehaviour
    {
        [HideInInspector] public EntityManager _entityManager;
        
        public Vector2 pos {
            get => transform.position;
            set => transform.position = value;
        }
    }
}