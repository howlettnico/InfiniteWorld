using System;
using System.Collections.Generic;
using App;
using UnityEngine;
using Utilities;
using World.Blocks;
using World.Chunk;
using World.Loading;

namespace World.Save
{
    public class SaveManager : AppModule
    {
        public Color savedChunksDebugColor;
        
        [SerializeField] private int savedChunks = 0;
        public Dictionary<long, ChunkRecord> chunks = new Dictionary<long, ChunkRecord>();
        private ChunkLoadManager _chunkLoadManager;

        private void Start()
        {
            _chunkLoadManager = App.App.Get<ChunkLoadManager>();
        }

        public ChunkRecord GetChunkRecord(Coord chunkPos)
        {
            long key = chunkPos.PackIntoLong();
            if (chunks.TryGetValue(key, out ChunkRecord r)) return r;

            return null;
        }

        public void RecordChunk(Chunk.Chunk c)
        {
            ChunkRecord r = c.GetRecord();
            if (r.editedGroundBlocks.Length == 0 && r.editedAboveGroundBlocks.Length == 0) return; //dont save unedited chunks

            long key = c.ChunkPos.PackIntoLong();
            
            if (chunks.ContainsKey(key)) chunks[key] = r;
            else
            {
                if (!chunks.TryAdd(key, r))
                    throw new Exception($"Unable to add chunk record {c.ChunkPos} to storage");
            }

            savedChunks = chunks.Count;
        }

        [ContextMenu("Save Loaded Chunks")]
        public void SaveLoadedChunks()
        {
            foreach (Chunk.Chunk c in _chunkLoadManager._loadedChunks.Values)
            {
                RecordChunk(c);
            }
        }
    }
}