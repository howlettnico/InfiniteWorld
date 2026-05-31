using UnityEditor;
using UnityEngine;
using World.Blocks;
using World.Loading;

namespace Editor
{
    [CustomEditor(typeof(BlockTypeCollection))]
    public class BlockTypeCollectionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BlockTypeCollection tc = (BlockTypeCollection) target;

            if (GUILayout.Button("Generate Texture"))
            {
                BlockTextureManager.UpdateCombinedTexture(tc);
                
            }
            base.OnInspectorGUI();
        }
    }
}