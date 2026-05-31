using UnityEngine;
using World.Blocks;

namespace Features.Items
{
    [CreateAssetMenu(fileName = "New Item Type Collection", menuName = "Item Type Collection")]
    public class ItemTypeCollection : ScriptableObject
    {
        [SerializeField] public ItemType[] types;
    }
}