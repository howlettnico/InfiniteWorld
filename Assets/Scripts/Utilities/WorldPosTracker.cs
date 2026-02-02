using System;
using UnityEngine;

namespace Utilities
{
    [System.Serializable]
    public class WorldPosTracker : MonoBehaviour
    {
        [SerializeField] private GameObject obj;
        // [SerializeField] public Position pos;

        private void Update()
        {
            // obj.transform.position = pos.ToVector2();
        }
    }
}
