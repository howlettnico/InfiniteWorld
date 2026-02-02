using UnityEngine;
using Utilities;

namespace System.Runtime.CompilerServices
{
    // This dummy class allows the Unity compiler to support C# 9 records
    internal static partial class IsExternalInit {}
    
}

namespace Utilities
{
    public static class Helper
    {
        public static float Dist(Coord p1, Coord p2)
        {
            float dx = p1.x - p2.x;
            float dy = p1.y - p2.y;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public static int Mod(int dividend, int divisor)
        {
            return (dividend % divisor + divisor) % divisor;
        }
        
    }
}