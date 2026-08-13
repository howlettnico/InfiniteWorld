using System;
using App;
using Player;
using UnityEngine;

namespace UI
{
    public class UIManager : AppModule
    {
        private PlayerManager _playerManager;
        [SerializeField] private ItemUICell[] playerMainInventory;
        [SerializeField] private GameObject selectedOutline;

        
        private void Start()
        {
            _playerManager = App.App.Get<PlayerManager>();
        }

        private void Update()
        {
            Player.Player p = _playerManager.player;
            Vector3 oldP = selectedOutline.transform.position;
            selectedOutline.transform.position = new Vector3(4 * 95 + 95 * p.selectedSlot + 105, oldP.y, oldP.z);
            for (int i = 0; i < playerMainInventory.Length && i < p.inventory.NumSlots; i++)
            {
                playerMainInventory[i].slot = p.inventory.slots[i];
            }
        }
    }
}
