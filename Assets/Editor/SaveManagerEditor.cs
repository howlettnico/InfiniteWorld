using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utilities;
using World.Chunk;
using World.Loading;
using World.Save;

namespace Editor
{
    [CustomEditor(typeof(SaveManager))]
    public class SaveManagerEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            serializedObject.Update();

            SaveManager m = (SaveManager) target;
            Transform transform = m.transform;
            Vector3 position = transform.position;
            float size = Chunk.ChunkSize;
            
            Handles.color = m.savedChunksDebugColor;

            //Drawing loaded rectanlges
            foreach (long l in m.chunks.Keys)
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
