using System;
using App;
using Input;
using UnityEngine;
using UnityEngine.Serialization;
using World.Blocks;

namespace Player
{
    public class PlayerInputManager : AppModule
    {
        private Player _player;
        private bool shift = false;


        private void Start()
        {
            _player = App.App.Get<PlayerManager>().player;
        }

        private void OnEnable()
        {
            InputManager manager = App.App.Get<InputManager>();
            manager.OnPlayerMove += Move;
            manager.OnPlayerUse += Use;
            manager.OnPlayerInteract += Interact;
            manager.OnPlayerShift += Shift;
            manager.OnPlayerNumKeys += SelectSlot;
        }
        
        private void OnDisable()
        {
            InputManager manager = App.App.Get<InputManager>();
            manager.OnPlayerMove -= Move;
            manager.OnPlayerUse -= Use;
            manager.OnPlayerInteract -= Interact;
            manager.OnPlayerShift -= Shift;
            manager.OnPlayerNumKeys -= SelectSlot;
        }

        private void Move(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) + Mathf.Abs(direction.y) != 0)
            {
                _player.facing = direction;
                _player.moving = true;
            }
            else _player.moving = false;

        }
    
        private void Use()
        {
            _player.Use(shift);
            // if (shift) _player.Place(BlockType.BlockTypeID.Air);
            // else _player.Place(BlockType.BlockTypeID.CedarLog);
        }

        private void Interact()
        {
            Debug.Log("Interact");
        }
    
        private void Shift(bool shift)
        {
            this.shift = shift;
            Debug.Log("Shift: " + shift);
        }

        private void SelectSlot(int slot)
        {
            _player.selectedSlot = slot;
        }
    }
}
