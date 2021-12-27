using NewHorizons.OrbitalPhysics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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
            if (gv == null) return OrbitalHelper.FalloffType.none;
            var falloffTypeString = typeof(GravityVolume).GetField("_falloffType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(gv).ToString();
            var falloffType = falloffTypeString.Equals("linear") ? OrbitalHelper.FalloffType.linear : OrbitalHelper.FalloffType.inverseSquared;
            return falloffType;
        }

        public static float GetFalloffExponent(this GravityVolume gv)
        {
            if (gv == null) return 0;
            return (float)typeof(GravityVolume).GetField("_falloffExponent", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(gv);
        }

        public static string ToCamelCase(this string str)
        {
            StringBuilder strBuilder = new StringBuilder(str);
            strBuilder[0] = strBuilder[0].ToString().ToLower().ToCharArray()[0];
            return strBuilder.ToString();
        }

        public static string ToTitleCase(this string str)
        {
            StringBuilder strBuilder = new StringBuilder(str);
            strBuilder[0] = strBuilder[0].ToString().ToUpper().ToCharArray()[0];
            return strBuilder.ToString();
        }

        public static void CopyPropertiesFrom(this object destination, object source)
        {
            // If any this null throw an exception
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");
            // Getting the Types of the objects
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();

            // Iterate the Properties of the source instance and  
            // populate them from their desination counterparts  
            PropertyInfo[] srcProps = typeSrc.GetProperties();
            foreach (PropertyInfo srcProp in srcProps)
            {
                if (!srcProp.CanRead)
                {
                    continue;
                }
                PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
                if (targetProperty == null)
                {
                    continue;
                }
                if (!targetProperty.CanWrite)
                {
                    continue;
                }
                if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate)
                {
                    continue;
                }
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    continue;
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                {
                    continue;
                }

                targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
            }
        }
    }
}
