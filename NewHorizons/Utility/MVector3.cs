using UnityEngine;

namespace NewHorizons.Utility
{
    public class MVector3
    {
        public MVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public static implicit operator MVector3(Vector3 vec) => new MVector3(vec.x, vec.y, vec.z);

        public static implicit operator Vector3(MVector3 vec) => new Vector3(vec.X, vec.Y, vec.Z);
    }
}