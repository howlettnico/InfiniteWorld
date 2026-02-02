using System;
using UnityEngine;

namespace Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private GameObject followObject;
        [Range(0f, 10f)] [SerializeField] private float objectWeight;
        [SerializeField] private GameObject cameraObject;
        private Vector2 _cameraPos;
        private float z;

        private void Awake()
        {
            Vector3 position = cameraObject.transform.position;
            
            _cameraPos = position;
            z = position.z;
        }

        private void Update()
        {
            Vector2 followPos = followObject.transform.position;
            
            _cameraPos += (followPos - _cameraPos) * (objectWeight * Time.deltaTime);
            
            cameraObject.transform.position = new Vector3(_cameraPos.x, _cameraPos.y, z);

        }
    }
}
