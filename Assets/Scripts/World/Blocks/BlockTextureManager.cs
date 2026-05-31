using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace World.Blocks
{
    public static class BlockTextureManager
    {
        public static int textureSize = 8;

        public static void UpdateCombinedTexture(BlockTypeCollection c)
        {
            int texturePixelCount = textureSize * textureSize;
            
            //loading types
            BlockType[] types = c.types;

            //Counting # of textures
            int totalTextures = 1, currentTexture = 1;
            foreach (BlockType type in types)
            {
                int start = currentTexture;
                foreach (BlockType.BlockState state in type.states)
                {
                    currentTexture = start + state.baseTextureIndex;
                    totalTextures = Math.Max(totalTextures, 
                        currentTexture + (state.animated ? state.animationFrameCount : 1));
                }

                currentTexture = totalTextures;
            }

            //Creating Texture
            Texture2DArray texArray = new Texture2DArray(textureSize, textureSize, totalTextures, TextureFormat.RGBA32, false);
            texArray.wrapMode = TextureWrapMode.Clamp;
            texArray.filterMode = FilterMode.Point;

            //Writing null texture
            Color[] nullColors = c.nullType.texture.GetPixels();
            texArray.SetPixels(nullColors, 0);
            c.nullType.states[0].trueTextureIndex = 0;
            
            //Writing rest of textures
            int textureI = 1, maxTextureI = 1;
            for (int typeI = 0; typeI < types.Length; typeI++)
            {
                BlockType type = types[typeI];
                Color[] colors = type.texture.GetPixels();
                int numTextures = type.texture.height / textureSize;
                
                Debug.Log(type.name);

                int start = maxTextureI;

                for (int stateI = 0; stateI < type.states.Length; stateI++)
                {
                    BlockType.BlockState state = type.states[stateI];

                    textureI = start + state.baseTextureIndex;
                    
                    //Setting texture index
                    state.trueTextureIndex = textureI;
                    
                    // Debug.Log(stateI + ") An: " + state.animated + " FC: " + state.animationFrameCount + " BI:" + state.baseTextureIndex + " TI: " + state.trueTextureIndex);

                    for (int frameI = 0; frameI < (state.animated ? state.animationFrameCount : 1); frameI++)
                    {
                        Debug.Log("Frame: " + frameI + " Tex: " + textureI + "/" + maxTextureI);
                        //Skipping if texture has already been drawn
                        if (textureI < maxTextureI)
                        {
                            Debug.Log("Skipped");
                            textureI++;
                            continue;
                        }

                        int stateTexI = state.baseTextureIndex + frameI;
                        int startI = numTextures - stateTexI - 1;
                        int endI = numTextures - stateTexI;
                        
                        // int startI = state.animated ? state.animationFrameCount - (state.baseTextureIndex + frameI) - 1 : state.baseTextureIndex + frameI;
                        // int endI = state.animated ? state.animationFrameCount - (state.baseTextureIndex + frameI) : state.baseTextureIndex + frameI + 1;
                        Color[] texColors = colors[(startI * texturePixelCount)..(endI * texturePixelCount)];
                        // Debug.Log(frameI +") " + (startI * texturePixelCount) + " -> " + (endI * texturePixelCount));

                        texArray.SetPixels(texColors, textureI);

                        textureI++;
                    }

                    maxTextureI = Math.Max(maxTextureI, textureI);
                }
            }
            
            Helper.SaveAsset(texArray, "Assets/Sprites/World/Blocks/_BlockTexs.asset");
            
            c.blockPlaneMaterial.SetTexture("_BlockTexs", texArray);
        }
    }
}