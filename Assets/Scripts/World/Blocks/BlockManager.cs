using System;
using System.Collections.Generic;
using App;
using Features.Inventory;
using UnityEngine;
using Utilities;
using World.Blocks.CustomBlocks;
using World.Loading;
using Exception = System.Exception;

namespace World.Blocks
{
    public class BlockManager : AppModule
    {
        private ChunkLoadManager _loadManager;
        
        [SerializeField] public BlockTypeCollection typeCollection;
        [HideInInspector] public BlockType[] types;
        private Dictionary<int, int> typeIDToIndex;
        public int numStates;

        private void Start()
        {
            _loadManager = App.App.Get<ChunkLoadManager>();
            
            LoadTypes();
        }

        [ContextMenu("Reload Types")]
        private void LoadTypes()
        {
            types = typeCollection.types;

            typeIDToIndex = new Dictionary<int, int>();
            for (int i = 0; i < types.Length; i++)
            {
                typeIDToIndex.Add((int) types[i].ID, i);
                // Debug.Log((int) types[i].ID + " -> " + i);
                numStates += types[i].states.Length;
            }
        }

        public BlockType GetBlockType(BlockType.BlockTypeID id)
        {
            return types[GetIndex(id)];
        }

        public Block GetBlock(Coord c, bool ground)
        {
            return _loadManager.GetBlock(c, ground);
        }

        public Block LoadBlockRecord(BlockRecord r, bool inGround)
        {
            return NewBlock(r, inGround);
        }
        
        /// <summary>
        /// Sets the block at a given position to a given block, regardless of what block is currently there
        /// </summary>
        /// <param name="c">World position of block</param>
        /// <param name="b">New Block</param>
        /// <param name="ground">Whether or not in the ground</param>
        public void SetBlock(Coord c, Block b, bool ground)
        {
            _loadManager.SetBlock(c, b, ground);
        }
        
        /// <summary>
        /// Attempts to set the block at a given position to a new block
        /// </summary>
        /// <param name="c">The world position of the block</param>
        /// <param name="b">The new block to replace the current block</param>
        /// <param name="ground">Whether or not the block is in the ground</param>
        /// <returns>Whether or not the block was successfully set</returns>
        public bool TrySetBlock(Coord c, Block b, bool ground)
        {
            return _loadManager.TrySetBlock(c, b, ground);
        }

        /// <summary>
        /// Breaks a block at a given world position
        /// </summary>
        /// <param name="c">World position of block</param>
        /// <param name="ground">Whether or not the block is in the ground layer</param>
        /// <returns>The inventory of the broken block (what the block dropped)</returns>
        public void BreakBlock(Coord c, bool ground)
        {
            _loadManager.BreakBlock(c, ground);
        }

        /// <summary>
        /// The number of different block types that have been assigned
        /// </summary>
        /// <returns>The number of different block types</returns>
        public int NumTypes()
        {
            return types.Length;
        }

        /// <summary>
        /// Creates a new block of given type with saved block data
        /// </summary>
        /// <param name="type">Type of the block</param>
        /// <param name="pos">World position of block</param>
        /// <param name="inGround">Whether or not the block is in the ground layer</param>
        /// <param name="rotated">Whether or not the block is rotated</param>
        /// <param name="state">Block State</param>
        /// <param name="save">Whether or not the block should be recorded</param>
        /// <param name="data">Saved block data</param>
        /// <returns>A new Block of given type</returns>
        /// <exception cref="Exception">When the desired block type has not been assigned</exception>
        public Block NewBlock(BlockType type, Coord pos, bool inGround, bool rotated, int state, bool save, CustomBlock.BlockData data = null)
        {
            return new Block(type, pos, inGround, rotated, state, save, data);
        }

        /// <summary>
        /// Creates a new block of given type (using the type's ID)
        /// </summary>
        /// <param name="typeID">Type of the block</param>
        /// <param name="pos">World position of block</param>
        /// <param name="inGround">Whether or not the block is in the ground layer</param>
        /// <param name="rotated">Whether or not the block is rotated</param>
        /// <param name="state">Block state</param>
        /// <param name="save">Whether or not the block should be recorded</param>
        /// <param name="data">Saved block data</param>
        /// <returns>A new Block of given type</returns>
        /// <exception cref="Exception">When the desired block type has not been assigned</exception>
        public Block NewBlock(BlockType.BlockTypeID typeID, Coord pos, bool inGround, bool rotated, int state, bool save, CustomBlock.BlockData data = null)
        {
            BlockType type = GetBlockType(typeID);
            return NewBlock(type, pos, inGround, rotated, state, save, data);
        }

        /// <summary>
        /// Creates a new block from its record
        /// </summary>
        /// <param name="r">The saved block record</param>
        /// <param name="inGround">Whether or not the block is in the ground</param>
        /// <returns></returns>
        /// <exception cref="Exception">When the desired block type has not been assigned</exception>
        public Block NewBlock(BlockRecord r, bool inGround)
        {
            return new Block(r, inGround);
        }

        public int GetIndex(BlockType.BlockTypeID id)
        {
            if (!typeIDToIndex.TryGetValue((int)id, out int index)) Debug.LogError("BlockType ID " + id + " is not in mapping dictionary");

            return index;
        }
    }
}
