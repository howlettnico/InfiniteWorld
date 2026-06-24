using System;
using Features.Items;
using UnityEngine;
using Utilities;
using World.Blocks.CustomBlocks;

namespace World.Blocks
{
        [CreateAssetMenu(fileName = "New Block Type", menuName = "Block Type")]
        public class BlockType : ScriptableObject
        {
                // To create a new type:
                //      - Upload new Texture
                //      - Create new scriptable object and add to _AllBlocks
                //      - For custom heights upload new height texture
                //      - Generate new texture on _AllBlocks
                //      - Add BlockTypeID in BlockType script and select on scriptable object
                //      - if custom script
                //              - write script that extends CustomBlock
                //              - add CustomBlock Script to BlockType script
                //              - select custom script in scriptable object
                //              - add script to SetScript function in Block class
                public enum BlockTypeID
                {
                        Null = 0,
                        Grass = 10,
                        Dirt = 20,
                        Air = 30,
                        LargeGrass = 40,
                        CedarLog = 50,
                        CedarLogSide = 51,
                        Sand = 60,
                        Water = 70,
                        Chest = 80,
                }

                public enum BlockScript
                {
                        None = 0,
                        Grass = 10,
                        Dirt = 20,
                        Chest = 30,
                }
                
                public enum BlockHeight
                {
                        None = 0,
                        Px1 = 1,
                        Px2 = 2,
                        Px3 = 3,
                        Px4 = 4,
                        Px5 = 5,
                        Px6 = 6,
                        Px7 = 7,
                        Px8 = 8,
                        Custom = 100
                }

                public BlockTypeID ID = BlockTypeID.Null;
                public string blockName;
                public Texture2D texture;
                public Texture2D customHeightTexture;
                public BlockState[] states = new BlockState[1];
                public bool rotatable;
                public bool solid;
                public bool replaceable;
                public BlockScript blockScript = BlockScript.None;
                public bool customScript => blockScript != BlockScript.None;
                public bool customData;
                public Drop[] drops;

                
                [Serializable]
                public class BlockState
                {
                        public int baseTextureIndex;
                        [HideInInspector] public int trueTextureIndex;
                        public bool animated;
                        public int animationFrameCount = 1;
                        public float animationFPS = 1;
                        public BlockHeight height;
                        public int baseHeightTextureIndex;
                        [HideInInspector] public int trueHeightTextureIndex;
                        public bool heightAnimated;
                }
        }
}
