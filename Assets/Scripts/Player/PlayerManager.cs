using System;
using App;
using UnityEngine;
using Utilities;
using World.Blocks;
using World.Chunk;
using World.Generation;
using World.Loading;

namespace Player
{
    public class PlayerManager : AppModule
    {
        //Managers
        private ChunkLoadManager _chunkLoadManager;
        public Player player;

        private void Start()
        {
            _chunkLoadManager = App.App.Get<ChunkLoadManager>();
        }

        private void Update()
        {
            _chunkLoadManager.UpdateLoadedBlocks(Chunk.WorldToChunk(player.pos));
        }
    }
}