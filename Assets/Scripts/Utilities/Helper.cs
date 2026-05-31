using UnityEditor;
using UnityEngine;
using Utilities;

namespace System.Runtime.CompilerServices
{
    // This dummy class allows the Unity compiler to support C# 9 records
    internal static partial class IsExternalInit {}
    
}

namespace Utilities
{
    public static class Helper
    {
        public static int Mod(int dividend, int divisor)
        {
            return (dividend % divisor + divisor) % divisor;
        }
        
        //Written By Claude
        public static void SaveAsset(Object asset, string path)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path) ?? string.Empty);

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Texture2DArray saved to {path}");
        }
    }
}