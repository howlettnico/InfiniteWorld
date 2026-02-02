
using UnityEngine;

namespace Utilities
{
    [System.Serializable]
    public struct Coord
    {
        public int x, y;

        public Coord(int X, int Y)
        {
            x = X;
            y = Y;
        }

        public Coord(Vector2 p)
        {
            x = Int(p.x);
            y = Int(p.y);
        }

        public Coord(long packedLong)
        {
            // 1. Shift the long 32 bits to the right to move the X bits back 
            //    to the "bottom", then cast to int.
            x = (int)(packedLong >> 32);

            // 2. Cast the whole long to an int. This automatically chops off 
            //    the top 32 bits (the X part) and leaves you with just the Y part.
            y = (int)packedLong;
        }

        public long PackIntoLong()
        {
            return (long)(((ulong)x << 32) | ((uint)y & 0xFFFFFFFF));
        }

        private static int Int(float v)
        {
            return Mathf.FloorToInt(v);
        }
        

        public override string ToString()
        {
            return $"({x}, {y})";
        }

        public static implicit operator Coord(Vector2 c)
        {
            return new Coord { x = Int(c.x), y = Int(c.y)};
        }
        
        public static implicit operator Coord(Vector3 c)
        {
            return new Coord { x = Int(c.x), y = Int(c.y)};
        }
        
        public static implicit operator Vector2(Coord c)
        {
            return new Vector2 { x = Int(c.x), y = Int(c.y)};
        }
        
        public static implicit operator Vector3(Coord c)
        {
            return new Vector3 { x = Int(c.x), y = Int(c.y)};
        }
        
        public static Coord operator +(Coord a, Coord b)
        {
            return new Coord(a.x + b.x, a.y + b.y);
        }
        
        public static Coord operator +(Coord a, Vector2 b)
        {
            return new Coord(a.x + Int(b.x), a.y + Int(b.y));
        }
        
        public static Coord operator +(Vector2 a, Coord b)
        {
            return new Coord(Int(a.x) + b.x, Int(a.y) + b.y);
        }
        
        // public static Coord operator +(Coord a, Position b)
        // {
        //     return new Coord(a.x + Int(b.x), a.y + Int(b.y));
        // }
    }
}