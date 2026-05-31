using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using App;
using Features.Inventory;
using Player;
using UnityEngine;
using Utilities;
using World.Blocks;
using World.Chunk;
using World.Generation;
using World.Save;

namespace World.Loading
{
    public class ChunkLoadManager : AppModule
    {
        public enum ColliderMethod
        {
            Individual = 0,
            Merging = 1,
            Varied = 2
        }

        // ***** Managers ***** 
        private BlockManager _blockManager;
        private SaveManager _saveManager;
        private GenerationManager _generationManager;
        
        public Color loadedChunksDebugColor;
        
        [SerializeField] private GameObject radiusDisplayObject;
        [SerializeField] private GameObject colliderParentObject;
        [SerializeField] private ColliderMethod colliderMethod = ColliderMethod.Varied;
        [field:SerializeField] public int LoadDistance { get; private set; }
        public Dictionary<long, Chunk.Chunk> _loadedChunks = new Dictionary<long, Chunk.Chunk>();
        public Dictionary<long, GameObject> _loadedColliders = new Dictionary<long, GameObject>();
        public int LoadedChunks = 0;
        
        
        // ***** Unity *****
        private void Start()
        {
            _blockManager = App.App.Get<BlockManager>();
            _generationManager = App.App.Get<GenerationManager>();
            _saveManager = App.App.Get<SaveManager>();
        }

        private void Update()
        {
            foreach (KeyValuePair<long, Chunk.Chunk> pair in _loadedChunks)
            {
                Chunk.Chunk c = (Chunk.Chunk)pair.Value;
                c.UpdateAllBlocks();
            }
        }

        // ***** Updating Blocks *****
        public void UpdateLoadedBlocks(Coord centerChunk)
        {
            //Moving render thing
            Coord world = Chunk.Chunk.ChunkToWorld(centerChunk);
            radiusDisplayObject.transform.localScale = new Vector3(LoadDistance * Chunk.Chunk.ChunkSize * 2, LoadDistance * Chunk.Chunk.ChunkSize * 2);
            radiusDisplayObject.transform.position = new Vector3(world.x, world.y);
            
            //Actual loading and unloading
            Load(centerChunk);

            Cleanup(centerChunk);

            LoadedChunks = _loadedChunks.Count;
        }

        // ***** General Unloading and Loading *****

        [ContextMenu("Reload All Chunks")]
        public void ReloadChunks()
        {
            long[] loadedChunks = _loadedChunks.Keys.ToArray();

            foreach (long cKey in loadedChunks)
            {
                _loadedChunks.TryGetValue(cKey, out Chunk.Chunk c);
                UnloadChunk(c);
                Load(new Coord(cKey));
            }
        }

        private void Cleanup(Coord centerChunk)
        {
            long[] keys = new long[_loadedChunks.Keys.Count];
            _loadedChunks.Keys.CopyTo(keys, 0);
            
            foreach (long key in keys)
            {
                Coord cPos = new Coord(key);
                if (Helper.Dist(centerChunk, cPos) <= LoadDistance) continue;
                if (!_loadedChunks.TryGetValue(key, out Chunk.Chunk c)) continue;

                UnloadChunk(c);
            }
        }

        private void Load(Coord centerChunk)
        {
            for (int x = centerChunk.x - LoadDistance; x <= centerChunk.x + LoadDistance; x++)
            {
                for (int y = centerChunk.y - LoadDistance; y <= centerChunk.y + LoadDistance; y++)
                {
                    Coord cPos = new Coord(x, y);
                    if (Helper.Dist(centerChunk, cPos) > LoadDistance) continue;
                    if (_loadedChunks.ContainsKey(cPos.PackIntoLong())) continue;

                    LoadChunk(cPos);
                }
            }
        }

        // ***** Chunk Specific Loading and Unloading *****
        private void LoadChunk(Coord cPos)
        {
            // Debug.Log("Load: " + cPos + "\n" + ToString());
            Chunk.Chunk c = _generationManager.GenerateChunk(cPos);

            ChunkRecord r = _saveManager.GetChunkRecord(cPos);

            if (r != null) c.LoadRecord(r);

            if (!_loadedChunks.TryAdd(cPos.PackIntoLong(), c)) throw new Exception($"Failed to load chunk {cPos.ToString()}");
            
            LoadChunkColliders(c);
        }

        private void UnloadChunk(Chunk.Chunk c)
        {
            // Debug.Log("Unload: " + c.ChunkPos + "\n" + ToString());
            _saveManager.RecordChunk(c);
            
            _loadedChunks.Remove(c.ChunkPos.PackIntoLong());
            
            UnloadChunkColliders(c);
        }
        
        // ***** Colliders *****

        private void LoadChunkColliders(Chunk.Chunk c)
        {
            if (_loadedColliders.ContainsKey(c.ChunkPos.PackIntoLong())) UnloadChunkColliders(c);

            Coord chunkOrigin = Chunk.Chunk.ChunkToWorld(c.ChunkPos);
            GameObject holder = new GameObject(c.ChunkPos.ToString(), typeof(CompositeCollider2D));
            holder.transform.SetParent(colliderParentObject.transform);
            holder.transform.position = chunkOrigin;

            Rigidbody2D rb = holder.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            
            bool[][] aboveGroundSolids = c.GetAboveGroundSolids();

            int width = aboveGroundSolids.Length;
            int height = aboveGroundSolids[0].Length;

            if (colliderMethod == ColliderMethod.Varied)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Coord offset = new Coord(x, y);
                        Coord world = chunkOrigin + offset;

                        bool solid = aboveGroundSolids[x][y];
                        if (!solid) continue;

                        int w = 1, h = 1;

                        int f = 0;
                        bool expanding = true;
                        while (expanding && f < 1000)
                        {
                            f++;
                            
                            //expanding to the right
                            bool rightExpansionWorks = x + w < width;
                            for (int y1 = 0; y1 < h && rightExpansionWorks; y1++)
                            {
                                if (!aboveGroundSolids[x + w][y + y1]) rightExpansionWorks = false;
                            }
                            
                            //expanding up
                            bool upExpansionWorks = y + h < height;
                            for (int x1 = 0; x1 < w && upExpansionWorks; x1++)
                            {
                                if (!aboveGroundSolids[x + x1][y + h]) upExpansionWorks = false;
                            }
                            
                            //deciding on expansion
                            if (rightExpansionWorks && (h > w || !upExpansionWorks)) w++;
                            else if (upExpansionWorks) h++;
                            
                            //updating expanding
                            expanding = rightExpansionWorks || upExpansionWorks;
                        }
                        
                        if (f == 1000) Debug.LogError("Collider Expansion Failed");
                        
                        //updating solid bitmap
                        for (int x1 = 0; x1 < w; x1++)
                        {
                            for (int y1 = 0; y1 < h; y1++)
                            {
                                aboveGroundSolids[x + x1][y + y1] = false;
                            }
                        }
                        
                        //spawning in 
                        GameObject o = new GameObject("Block Collider");
                        BoxCollider2D b = o.AddComponent<BoxCollider2D>();
                        o.transform.SetParent(holder.transform);
                        o.transform.position = new Vector2(world.x + w * 0.5f, world.y + h * 0.5f);
                        b.size = new Vector2(w, h);
                    }
                }
            }
            else
            {
                //simplifying solids
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Coord offset = new Coord(x, y);
                        Coord world = chunkOrigin + offset;

                        bool solid = aboveGroundSolids[x][y];
                        if (!solid) continue;

                        if (colliderMethod == ColliderMethod.Individual)
                        {
                            int neighbors = 0;

                            neighbors += x - 1 < 0 ? 0 : (aboveGroundSolids[x - 1][y] ? 1 : 0);
                            neighbors += x + 1 >= width ? 0 : (aboveGroundSolids[x + 1][y] ? 1 : 0);
                            neighbors += y - 1 < 0 ? 0 : (aboveGroundSolids[x][y - 1] ? 1 : 0);
                            neighbors += y + 1 >= height ? 0 : (aboveGroundSolids[x][y + 1] ? 1 : 0);

                            if (neighbors == 4) continue;

                            GameObject o = new GameObject("Block Collider", typeof(BoxCollider2D));
                            o.transform.SetParent(holder.transform);
                            o.transform.position = new Vector2(world.x + 0.5f, world.y + 0.5f);
                        }
                        else if (colliderMethod == ColliderMethod.Merging)//Merging colliders
                        {
                            BoxCollider2D b = holder.AddComponent<BoxCollider2D>();
                            b.compositeOperation = Collider2D.CompositeOperation.Merge;
                            b.offset = new Vector2(x + 0.5f, y + 0.5f);
                        }
                    }
                }
            }

            //Generating geometry to avoid clipping
            // Physics2D.SyncTransforms();
            // CompositeCollider2D comp = holder.GetComponent<CompositeCollider2D>();
            // comp.GenerateGeometry();
            
            if (!_loadedColliders.TryAdd(c.ChunkPos.PackIntoLong(), holder)) throw new Exception($"Failed to load colliders of chunk {c.ChunkPos.ToString()}");
        }

        private void UnloadChunkColliders(Chunk.Chunk c)
        {
            long key = c.ChunkPos.PackIntoLong();
            if (!_loadedColliders.TryGetValue(key, out GameObject o)) return;

           Destroy(o);
            
            _loadedColliders.Remove(key);
        }

        // ***** Getting and Setting *****
        public Block GetBlock(Coord world, bool ground)
        {
            Coord chunkPos = Chunk.Chunk.WorldToChunk(world);
            if (!_loadedChunks.TryGetValue(chunkPos.PackIntoLong(), out Chunk.Chunk c)) return null;
                // throw new Exception($"Failed to get block {world.ToString()} in chunk {chunkPos.ToString()} from loadedChunks");

            return c.GetBlock(world, ground);
        }

        public void SetBlock(Coord world, Block b, bool ground)
        {
            Coord chunkPos = Chunk.Chunk.WorldToChunk(world);
            if (!_loadedChunks.TryGetValue(chunkPos.PackIntoLong(), out Chunk.Chunk c)) //return null;
                throw new Exception($"Failed to set block {world.ToString()} in chunk {chunkPos.ToString()} from loadedChunks");

            c.SetBlock(world, b, ground);
            
            //TODO make this not have to reload the entire chunk whenever a collider is added
            if (b.type.solid) LoadChunkColliders(c);
        }
        
        public bool TrySetBlock(Coord world, Block b, bool ground)
        {
            Coord chunkPos = Chunk.Chunk.WorldToChunk(world);
            if (!_loadedChunks.TryGetValue(chunkPos.PackIntoLong(), out Chunk.Chunk c)) //return null;
                throw new Exception($"Failed to set block {world.ToString()} in chunk {chunkPos.ToString()} from loadedChunks");

            if (!c.TrySetBlock(world, b, ground)) return false;
            
            //TODO make this not have to reload the entire chunk whenever a collider is added
            LoadChunkColliders(c);
            
            return true;
        }

        // public bool BlockLoaded(Coord world)
        // {
        //     
        // }

        public Inventory BreakBlock(Coord world, bool ground)
        {
            Coord chunkPos = Chunk.Chunk.WorldToChunk(world);
            if (!_loadedChunks.TryGetValue(chunkPos.PackIntoLong(), out Chunk.Chunk c)) //return null;
                throw new Exception($"Failed to set block {world.ToString()} in chunk {chunkPos.ToString()} from loadedChunks");

            Inventory i = c.BreakBlock(world, ground);
            
            //TODO make this not have to reload the entire chunk whenever a collider is added
            LoadChunkColliders(c);

            return i;
        }

        // ***** Helpers *****

        public override string ToString()
        {
            StringBuilder s = new StringBuilder("String Builder:\nLoaded Chunks: ");
            
            foreach (long l in _loadedChunks.Keys)
            {
                Coord c = new Coord(l);
                s.Append(c.ToString()).Append(", ");
            }

            return s.ToString();
        }
    }
}