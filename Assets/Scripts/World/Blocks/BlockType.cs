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
                //      - Create new scriptable object
                //      - Add BlockTypeID in BlockType script
                //      - Add scriptable object to the BlockManager in scene
                //      - Upload new Texture and update BlockPlane Material (/Assets/Rendering/Custom Material/BlockPlane)
                //      - if custom script
                //              - write script that extends Block
                //              - add BlockScript to BlockType script
                //              - go to BlockManager and add return type to NewBlock() and a bunch of other guys
                public enum BlockTypeID
                {
                        Grass = 0,
                        Dirt = 1,
                        Air = 2,
                        LargeGrass = 3,
                        CedarLog = 4,
                        Sand = 5,
                        Water = 6,
                        CedarLogSide = 7
                }

                public enum BlockScript
                {
                        None = 0,
                        Grass = 1,
                        Dirt = 2
                }
                
                public BlockTypeID ID;
                public string blockName;
                [HideInInspector] public int textureIndex;
                public bool animated;
                public int animationFrameCount = 1;
                public float animationFPS = 1;
                public bool rotatable;
                public Drop[] drops;
                public bool solid;
                public bool replaceable;
                public BlockScript blockScript;
        }
}
