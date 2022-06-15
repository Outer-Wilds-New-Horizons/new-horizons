using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using NomaiCoordinates = NewHorizons.External.Configs.StarSystemConfig.NomaiCoordinates;
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
                }
                catch (Exception)
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

        public static Quaternion TransformRotation(this Transform transform, Quaternion localRotation)
        {
            return transform.rotation * localRotation;
        }

        public static bool CheckAllCoordinates(this NomaiCoordinateInterface nomaiCoordinateInterface) => Main.SystemDict.Where(system => system.Value.Config.coords != null).Select(system => new KeyValuePair<string, NomaiCoordinates>(system.Key, system.Value.Config.coords)).Any(system => nomaiCoordinateInterface.CheckCoordinates(system.Key, system.Value));

        public static bool CheckAllCoordinates(this NomaiCoordinateInterface nomaiCoordinateInterface, out string selectedSystem)
        {
            foreach (KeyValuePair<string, NomaiCoordinates> cbs in Main.SystemDict.Where(system => system.Value.Config.coords != null).Select(system => new KeyValuePair<string, NomaiCoordinates>(system.Key, system.Value.Config.coords)))
            {
                if (CheckCoordinates(nomaiCoordinateInterface, cbs.Key, cbs.Value))
                {
                    selectedSystem = cbs.Key;
                    return true;
                }
            }
            selectedSystem = null;
            return false;
        }

        public static bool CheckCoordinates(this NomaiCoordinateInterface nomaiCoordinateInterface, string system, NomaiCoordinates coordinates)
        {
            bool xCorrect = nomaiCoordinateInterface._nodeControllers[0].CheckCoordinate(coordinates.x);
            bool yCorrect = nomaiCoordinateInterface._nodeControllers[1].CheckCoordinate(coordinates.y);
            bool zCorrect = nomaiCoordinateInterface._nodeControllers[2].CheckCoordinate(coordinates.z);
            Utility.Logger.Log($"Coordinate Check for {system}: {xCorrect}, {yCorrect}, {zCorrect} [{string.Join("-", coordinates.x)}, {string.Join("-", coordinates.y)}, {string.Join("-", coordinates.z)}]");
            return xCorrect && yCorrect && zCorrect;
        }
    }
}
