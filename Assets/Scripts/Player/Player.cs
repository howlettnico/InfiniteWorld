using System;
using Features.Inventory;
using Features.Items;
using UnityEngine;
using Utilities;
using World.Blocks;

namespace Player
{
    public class Player : MonoBehaviour
    {       
        //Managers
        private BlockManager _blockManager;
        private ItemManager _itemManager;

        //Serialized Fields
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private int inventorySize = 9;

        public Inventory inventory;
        
        public Vector2 facing;
        public bool moving;
        public int selectedSlot = 1;

        public Vector2 pos {
            get {return transform.position;} 
            set {transform.position = value;}
        }
        
        //***** Unity *****
        private void Start()
        {
            _blockManager = App.App.Get<BlockManager>();
            _itemManager = App.App.Get<ItemManager>();
            inventory = new Inventory(inventorySize);
        }

        private void Update()
        {
            if (moving) rb.linearVelocity = facing.normalized * moveSpeed;
            else rb.linearVelocity = Vector2.zero;
            // if (moving) pos += facing.normalized * (moveSpeed * Time.deltaTime);
        }
        
        //***** Using *****

        public void Use(bool shift)
        {
            Slot s = GetSelectedSlot();
            Item i = s.item;
            ItemType t = i.type;
            if (s.empty) Break();
            else
            {
                if (t.placeable && TryPlace(t.placedBlockID, _blockManager.GetBlockType(t.placedBlockID).rotatable && FocusOffset().y != 0)) s.TryRemove(s.item);
            }
        }

        //***** Placing and Breaking *****
        
        public void PlaceGround(BlockType.BlockTypeID id)
        {
            _blockManager.SetBlock(pos, _blockManager.NewBlock(id, pos, true, false, true), true);
        }
        
        public bool TryPlace(BlockType.BlockTypeID id, bool rotated = false)
        {
            Coord p = FocusedCoord();
            return _blockManager.TrySetBlock(p, _blockManager.NewBlock(id, p, false, rotated, true), false);
        }
        
        public void BreakGround()
        {
            Coord p = FocusedCoord();
            Inventory i = _blockManager.BreakBlock(p, true);
            Inventory rem = inventory.TryAdd(i);
            //TODO drop rem items on ground
        }

        public void Break()
        {
            Coord p = FocusedCoord();
            Inventory i = _blockManager.BreakBlock(p, false);
            Inventory rem = inventory.TryAdd(i);
            //TODO drop rem items on ground
        }

        //***** Helpers *****
        
        private Coord FocusedCoord()
        {
            return pos + FocusOffset();
        }

        private Coord FocusOffset()
        {
            Vector2 offset = new Vector2(0, 0);
            if (Mathf.Abs(facing.x) > 0)
            {
                offset.x += Math.Sign(facing.x);
            }else if (Mathf.Abs(facing.y) > 0)
            {
                offset.y += Math.Sign(facing.y);
            }

            return offset;
        }

        private Slot GetSlot(int i)
        {
            return inventory.slots[i];
        }

        private Slot GetSelectedSlot()
        {
            return inventory.slots[selectedSlot - 1];
        }
    }
}
