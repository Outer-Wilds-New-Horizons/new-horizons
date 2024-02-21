#region

using NewHorizons.Utility.DebugTools.Menu;
using NewHorizons.Utility.OWML;
using Newtonsoft.Json;
using System;
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
            if (vec == null)
            {
                NHLogger.LogWarning($"Null MVector2 can't be turned into a non-nullable Vector2, returning Vector2.zero - {Environment.StackTrace}");
                return Vector2.zero;
            }
            return new Vector2(vec.x, vec.y);
        }
    }
}