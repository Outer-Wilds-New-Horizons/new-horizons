#region

using Newtonsoft.Json;
using UnityEngine;

#endregion

namespace NewHorizons.External.SerializableData
{
    [JsonObject]
    public class MVector2
    {
        public MVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float x;
        public float y;

        public static implicit operator MVector2(Vector2 vec)
        {
            return new MVector2(vec.x, vec.y);
        }

        public static implicit operator Vector2(MVector2 vec)
        {
            return new Vector2(vec.x, vec.y);
        }
    }
}