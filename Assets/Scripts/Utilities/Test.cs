using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using World.Chunk;

namespace Utilities
{
    public class Test : MonoBehaviour
    {
        private SmartBitStream b;
        private List<int> vals = new List<int>();
        private List<int> vals1 = new List<int>();

        private void Start()
        {
            // b = new SmartBitStream(new []{1, 8});
            //
            // for (int i = 0; i < 100; i++)
            // {
            //     int v = UnityEngine.Random.Range(0, 2);
            //     b.AddNext(v);
            //     vals.Add(v);
            //     int v1 = UnityEngine.Random.Range(0, 128);
            //     b.AddNext(v1);
            //     vals1.Add(v1);
            // }
            //
            // StringBuilder s = new StringBuilder();
            // for (int i = 0; i < 100; i++)
            // {
            //     int v = b.ReadNext();
            //     s.Append("Val: " + vals[i] + " Saved: " + v + (vals[i] == v ? " " : "WOTNOGNONGONGONG") + "\n");
            //     int v1 = b.ReadNext();
            //     s.Append("Val1: " + vals1[i] + " Saved1: " + v1 + (vals1[i] == v1 ? " " : "WOTNOGNONGONGONG") + "\n\n");
            // }
            //
            // s.Append("NEXT Test: \n");
            // for (int i = 0; i < 100; i++)
            // {
            //     b.GoToSegment(i);
            //     int v = b.ReadNext();
            //     s.Append("Val: " + vals[i] + " Saved: " + v + (vals[i] == v ? " " : "WOTNOGNONGONGONG") + "\n");
            //     int v1 = b.ReadNext();
            //     s.Append("Val1: " + vals1[i] + " Saved1: " + v1 + (vals1[i] == v1 ? " " : "WOTNOGNONGONGONG") + "\n\n");
            // }
            //
            // Debug.Log(s);
        }

        private void Update()
        {
        }
    }
}
