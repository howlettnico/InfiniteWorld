using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace World.Blocks
{
    public static class BlockTextureManager
    {
        public static int textureWidth = 8;

        public static void UpdateCombinedTexture(BlockTypeCollection c)
        {
            int texturePixelCount = textureWidth * textureWidth;
            
            //loading types
            BlockType[] types = c.types;

            //Counting # of textures
            int textures = 1;
            foreach (BlockType type in types)
            {
                textures += type.animated ? type.animationFrameCount : 1;
            }

            //Creating Texture
            Texture2DArray texArray = new Texture2DArray(textureWidth, textureWidth, textures, TextureFormat.RGBA32, false);
            texArray.wrapMode = TextureWrapMode.Clamp;
            texArray.filterMode = FilterMode.Point;

            //Writing null texture
            Color[] nullColors = c.nullType.texture.GetPixels();
            texArray.SetPixels(nullColors, 0);
            c.nullType.textureIndex = 0;
            
            //Writing rest of textures
            int textureI = 1;
            for (int typeI = 0; typeI < types.Length; typeI++)
            {
                BlockType type = types[typeI];
                Color[] colors = type.texture.GetPixels();
                
                //Setting texture index
                type.textureIndex = textureI;
                // Debug.Log(type.name + " " + type.textureIndex);

                for (int frameI = 0; frameI < (type.animated ? type.animationFrameCount : 1); frameI++)
                {
                    int startI = type.animated ? type.animationFrameCount - frameI - 1: frameI;
                    int endI = type.animated ? type.animationFrameCount - frameI : frameI + 1;
                    Color[] colorsSub = colors[(startI * texturePixelCount)..(endI * texturePixelCount)];
                    // Debug.Log(frameI +") " + (startI * texturePixelCount) + " -> " + (endI * texturePixelCount));

                    texArray.SetPixels(colorsSub, textureI);

                    textureI++;
                }
            }
            
            Helper.SaveAsset(texArray, "Assets/Sprites/World/Blocks/_BlockTexs.asset");
            
            c.blockPlaneMaterial.SetTexture("_BlockTexs", texArray);
        }
    }
}