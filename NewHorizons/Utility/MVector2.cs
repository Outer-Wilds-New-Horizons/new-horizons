using UnityEngine;

namespace NewHorizons.Utility
{
    public class MVector2
    {
        public MVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }

        public static implicit operator MVector2(Vector2 vec) => new MVector2(vec.x, vec.y);

        public static implicit operator Vector2(MVector2 vec) => new Vector2(vec.X, vec.Y);
    }
}