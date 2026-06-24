using UnityEngine;

namespace World.Blocks
{
    [CreateAssetMenu(fileName = "New Block Type Collection", menuName = "Block Type Collection")]
    public class BlockTypeCollection : ScriptableObject
    {
        [SerializeField] public Material blockPlaneMaterial;
        [SerializeField] public Texture2DArray blockTextures;
        [SerializeField] public Texture2DArray blockHeightTextures;
        [SerializeField] public BlockType nullType;
        [SerializeField] public BlockType[] types;
    }
}