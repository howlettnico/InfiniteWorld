using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utilities;
using World.Chunk;
using World.Loading;

namespace Editor
{
    [CustomEditor(typeof(ChunkLoadManager))]
    public class ChunkLoadManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _loadedChunksSerializedProperty;

        private void OnEnable()
        {
            // Find the property using its exact name (case-sensitive)
            _loadedChunksSerializedProperty = serializedObject.FindProperty("_loadedChunks");
        }
        private void OnSceneGUI()
        {
            serializedObject.Update();

            ChunkLoadManager m = (ChunkLoadManager) target;
            Transform transform = m.transform;
            Vector3 position = transform.position;
            float size = Chunk.ChunkSize;
            
            Handles.color = m.loadedChunksDebugColor;

            //Drawing loaded rectanlges
            foreach (long l in m._loadedChunks.Keys)
            {
                Coord c = new Coord(l);
                Coord world = Chunk.ChunkToWorld(c);
                Vector3[] verts = new Vector3[]
                {
                    position + transform.TransformDirection(new Vector3(world.x, world.y, 0)),
                    position + transform.TransformDirection(new Vector3(world.x + size, world.y, 0)),
                    position + transform.TransformDirection(new Vector3(world.x + size, world.y + size, 0)),
                    position + transform.TransformDirection(new Vector3(world.x, world.y + size, 0))
                };

                Handles.DrawLine(verts[0], verts[1]);
                Handles.DrawLine(verts[1], verts[2]);
                Handles.DrawLine(verts[2], verts[3]);
                Handles.DrawLine(verts[3], verts[0]);
            }
            

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
