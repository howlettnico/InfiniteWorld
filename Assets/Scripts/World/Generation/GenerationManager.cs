using System;
using App;
using Player;
using UnityEngine;
using World.Blocks;
using Utilities;
using Utilities.NoiseDotNet;
using World.Blocks.CustomBlocks;
using Random = UnityEngine.Random;
using Chuck = World.Chunk.Chunk;

namespace World.Generation
{
    public class GenerationManager : AppModule
    {
        public enum GenOffset
        {
            Altitude = 0
        }

        private BlockManager _blockManager;
        public int seed = 100;
        [SerializeField] private float smallFreq = 0.01f, ampMult = 2, freqMult = 2;
        [SerializeField] private int octaves = 1;

        private void Start()
        {
            _blockManager = App.App.Get<BlockManager>();
        }

        public Chuck GenerateChunk(Coord chunkPos)
        {
            // Debug.Log($"Generating block at ({c.x}, {c.y})");
            BlockManager blockManager = App.App.Get<BlockManager>();

            Coord worldPos = Chuck.ChunkToWorld(chunkPos);

            //Getting perlin noise output
            //TODO make these get stored over frames
            float[] output = Utilities.Random.GetChunk(chunkPos, smallFreq, 1f, octaves, ampMult, freqMult, seed, (int)GenOffset.Altitude);

            //Creating chunk
            Chuck c = new Chuck(chunkPos);
            
            for (int x = 0; x < Chuck.ChunkSize; x++)
            {
                for (int y = 0; y < Chuck.ChunkSize; y++)
                {
                    Coord blockPos = new Coord(x + worldPos.x, y + worldPos.y);

                    float height = output[x * Chuck.ChunkSize + y];
                    
                    //Assigning Ground Block
                    BlockType.BlockTypeID groundID = 0;
                    BlockType.BlockTypeID aboveGroundID = 0;
                    int groundBlockState = 0;
                    int aboveGroundBlockState = 0;

                    // if (Mathf.Abs(height) > 0.5f) Debug.Log(height + " " + blockPos.ToString());

                    if (height < -0.5)
                    {
                        groundID = BlockType.BlockTypeID.Water;
                        aboveGroundID = BlockType.BlockTypeID.Air;
                    }
                    else if (height < -0.45)
                    {
                        groundID = BlockType.BlockTypeID.Sand;
                        aboveGroundID = BlockType.BlockTypeID.Air;
                    }
                    else if (height < -0.2)
                    {
                        groundID = BlockType.BlockTypeID.Dirt;
                        aboveGroundID = BlockType.BlockTypeID.Air;
                    }
                    else if (height < 0.2)
                    {
                        groundID = BlockType.BlockTypeID.Grass;
                        aboveGroundID = BlockType.BlockTypeID.Air;
                    }
                    else if (height < 0.5)
                    {
                        groundID = BlockType.BlockTypeID.Grass;
                        aboveGroundID = BlockType.BlockTypeID.LargeGrass;
                    }
                    else
                    {
                        groundID = BlockType.BlockTypeID.Dirt;
                        aboveGroundID = BlockType.BlockTypeID.CedarLog;
                    }

                    Block ground = _blockManager.NewBlock(groundID, blockPos, true, false, groundBlockState, false);
                    c.SetBlock(blockPos, ground, true);

                    Block aboveGround = _blockManager.NewBlock(aboveGroundID, blockPos, false, false, aboveGroundBlockState, false);
                    c.SetBlock(blockPos, aboveGround, false);
                }
            }

            return c;
        }
        
    }
}
