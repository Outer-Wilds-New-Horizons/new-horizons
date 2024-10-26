using Newtonsoft.Json;
using UnityEngine;

namespace NewHorizons.External.SerializableData
{
    [JsonObject]
    public class MVector3
    {
        public MVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float x;
        public float y;
        public float z;

        public static implicit operator MVector3(Vector3 vec)
        {
            return new MVector3(vec.x, vec.y, vec.z);
        }

        public static implicit operator Vector3(MVector3 vec)
        {
            return new Vector3(vec.x, vec.y, vec.z);
        }

        public float Length() => Mathf.Sqrt(x * x + y * y + z * z);

        public override string ToString() => $"{x}, {y}, {z}";
    }
}
