using System;
using System.Collections.Generic;
using Features.Inventory;
using UnityEngine;
using Utilities;
using World.Blocks;

namespace World.Chunk
{
    public class Chunk
    {
        private BlockManager _blockManager;
        
        public static readonly int ChunkSize = 16;
        private static readonly int blockCount = ChunkSize * ChunkSize;
        private Block[] _ground;
        private Block[] _aboveGround;
        public Coord ChunkPos;
        public bool save;

        public Chunk(Coord pos)
        {
            _blockManager = App.App.Get<BlockManager>();
            ChunkPos = pos;
            
            _ground = new Block[blockCount];
            _aboveGround = new Block[blockCount];
        }

        public void UpdateAllBlocks()
        {
            foreach (Block b in _ground)
            {
                if (!b.custom) continue;
                b.blockScript.Update();
            }
            
            foreach (Block b in _aboveGround)
            {
                if (!b.custom) continue;
                b.blockScript.Update();
            }
        }

        // ***** Getting and Setting *****
        public void SetBlock(Coord world, Block block, bool ground)
        {
            Coord chunk = WorldToChunkInternal(world);
            (ground ? _ground : _aboveGround)[Inx(chunk.x, chunk.y)] = block;

            save = save || block.save;
        }
        
        public bool TrySetBlock(Coord world, Block block, bool ground)
        {
            Coord chunk = WorldToChunkInternal(world);
            if (!(ground ? _ground : _aboveGround)[Inx(chunk.x, chunk.y)].type.replaceable) return false;
            
            (ground ? _ground : _aboveGround)[Inx(chunk.x, chunk.y)] = block;
            
            save = save || block.save;

            return true;
        }
        
        public Block GetBlock(Coord world, bool ground)
        {
            if (!ChunkPos.Equals(WorldToChunk(world))) throw new Exception(
                $"{world.ToString()} does is not in chunk {ChunkPos}");
            
            Coord chunk = WorldToChunkInternal(world);
            
            return (ground ? _ground : _aboveGround)[Inx(chunk.x, chunk.y)];
        }

        public Inventory BreakBlock(Coord world, bool ground)
        {
            Coord chunk = WorldToChunkInternal(world);
            Inventory i = (ground ? _ground : _aboveGround)[Inx(chunk.x, chunk.y)].Break();
            
            SetBlock(world, _blockManager.NewBlock(BlockType.BlockTypeID.Air, world, false, true, true), ground);
            
            return i;
        }
        
        // ***** Records *****
        public void LoadRecord(ChunkRecord r)
        {
            if (!r.pos.Equals(ChunkPos))
                throw new Exception( $"Attempting to load chunk record {r.pos.ToString()} into chunk {ChunkPos.ToString()}");

            foreach (BlockRecord br in r.editedGroundBlocks)
            {
                SetBlock(br.pos, _blockManager.LoadBlockRecord(br, true), true);
            }
            
            foreach (BlockRecord br in r.editedAboveGroundBlocks)
            {
                SetBlock(br.pos, _blockManager.LoadBlockRecord(br, false), false);
            }
        }
        public ChunkRecord GetRecord()
        {
            List<BlockRecord> ground = new List<BlockRecord>();
            List<BlockRecord> aboveGround = new List<BlockRecord>();
            for (int x = 0; x < ChunkSize; x++)
            {
                for (int y = 0; y < ChunkSize; y++)
                {
                    Block b = _ground[Inx(x, y)];
                    if (b.save) ground.Add(b.GetRecord());
                    
                    Block b2 = _aboveGround[Inx(x, y)];
                    if (b2.save) aboveGround.Add(b2.GetRecord());
                }
            }

            return new ChunkRecord(ChunkPos, ground.ToArray(), aboveGround.ToArray());
        }
        
        // ***** Accessing Information *****

        //TODO make this a bitmap using the bits in a list of ints
        public bool[][] GetAboveGroundSolids()
        {
            bool[][] aboveGroundSolids = Get2DArray<bool>();

            for (int x = 0; x < ChunkSize; x++)
            {
                for (int y = 0; y < ChunkSize; y++)
                {
                    Block b = _aboveGround[Inx(x, y)];
                    aboveGroundSolids[x][y] = b.type.solid;
                }
            }

            return aboveGroundSolids;
        }
        
        // ***** Converting Coordinates
        public static Coord WorldToChunkInternal(Coord world)
        {
            return new Coord(Helper.Mod(world.x, ChunkSize), Helper.Mod(world.y, ChunkSize));
        }
        public static Coord WorldToChunk(Coord world)
        {
            return new Coord(Mathf.FloorToInt((float) world.x / ChunkSize), Mathf.FloorToInt((float) world.y / ChunkSize));
        }
        public static Coord ChunkToWorld(Coord chunk)
        {
            return new Coord(chunk.x * ChunkSize, chunk.y * ChunkSize);
        }

        private static int Inx(int x, int y)
        {
            return y * ChunkSize + x;
        }

        private static T[][] Get2DArray<T>()
        {
            T[][] array = new T[ChunkSize][];

            for (int i = 0; i < ChunkSize; i++)
            {
                array[i] = new T[ChunkSize];
            }

            return array;
        }
    }
}
