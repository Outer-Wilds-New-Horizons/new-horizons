using OWML.Utils;
using System;
using System.Collections.Generic;

namespace NewHorizons.Utility.OWML
{
    public static class EnumUtilities
    {
        private static List<Enum> Enums = new List<Enum>();

        public static void ClearCache()
        {
            NHLogger.LogVerbose($"Clearing enum cache");
            foreach (var @enum in Enums)
            {
                if (@enum == null) continue;
                EnumUtils.Remove(@enum.GetType(), @enum);
            }
            Enums.Clear();
        }

        public static T Create<T>(string name) where T : Enum
        {
            NHLogger.LogVerbose($"Creating enum value [{name}] for type [{typeof(T).FullName}]");
            T @enum = EnumUtils.Create<T>(name);
            Enums.SafeAdd(@enum);
            return @enum;
        }

        public static void Create<T>(string name, T value) where T : Enum
        {
            NHLogger.LogVerbose($"Creating enum value [{name}] for type [{typeof(T).FullName}]");
            EnumUtils.Create(name, value);
            Enums.SafeAdd(value);
        }

        public static void Remove<T>(string name) where T : Enum
        {
            NHLogger.LogVerbose($"Removing enum value [{name}] from type [{typeof(T).FullName}]");
            T @enum = EnumUtils.Parse<T>(name);
            Enums.Remove(@enum);
            EnumUtils.Remove<T>(name);
        }

        public static void Remove<T>(T value) where T : Enum
        {
            NHLogger.LogVerbose($"Removing enum value [{value.GetName()}] from type [{typeof(T).FullName}]");
            Enums.Remove(value);
            EnumUtils.Remove<T>(value);
        }
    }
}
