using System;
using Features.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [Serializable]
    public class ItemUICell : MonoBehaviour
    {
        [SerializeField] public Slot slot;
        [SerializeField] private GameObject itemDisplay;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI count;

        private void Start()
        {
            slot = new Slot();
        }

        // Update is called once per frame
        void Update()
        {
            itemDisplay.SetActive(!slot.empty);
            // if (slot.empty) return;

            itemImage.sprite = slot.item.type.texture;
            count.text = "" + slot.count;
        }
    }
}
