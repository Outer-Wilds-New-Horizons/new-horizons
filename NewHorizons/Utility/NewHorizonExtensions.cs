using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

        public static float GetFalloffExponent(this GravityVolume gv)
        {
            if (gv == null) return 0;
            if (gv._falloffType == GravityVolume.FalloffType.linear) return 1;
            if (gv._falloffType == GravityVolume.FalloffType.inverseSquared) return 2;

            return 0;
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

                try
                {
                    targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
                }  catch(Exception)
                {
                    Logger.LogWarning($"Couldn't copy property {targetProperty.Name} from {source} to {destination}");
                }
            }
        }

        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

        public static GameObject InstantiateInactive(this GameObject original)
        {
            if (!original.activeSelf)
            {
                return UnityEngine.Object.Instantiate(original);
            }

            original.SetActive(false);
            var copy = UnityEngine.Object.Instantiate(original);
            original.SetActive(true);
            return copy;
        }
    }
}
