using NewHorizons.OrbitalPhysics;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NewHorizons.Utility
{
    public static class NewHorizonsExtensions
    {
        public static MVector3 ToMVector3(this Vector3 vector3)
        {
            return new MVector3(vector3.x, vector3.y, vector3.z);
        }

        public static T GetSetting<T>(this Dictionary<string, object> dict, string settingName)
        {
            return (T)dict[settingName];
        }

        public static OrbitalHelper.FalloffType GetFalloffType(this GravityVolume gv)
        {
            var falloffTypeString = typeof(GravityVolume).GetField("_falloffType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(gv).ToString();
            var falloffType = falloffTypeString.Equals("linear") ? OrbitalHelper.FalloffType.linear : OrbitalHelper.FalloffType.inverseSquared;
            return falloffType;
        }
    }
}
