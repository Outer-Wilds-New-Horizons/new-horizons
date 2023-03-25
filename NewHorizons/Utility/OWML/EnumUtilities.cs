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
            foreach (var @enum in Enums)
            {
                if (@enum == null) continue;
                EnumUtils.Remove(@enum.GetType(), @enum);
            }
            Enums.Clear();
        }

        public static T Create<T>(string name) where T : Enum
        {
            T @enum = EnumUtils.Create<T>(name);
            Enums.SafeAdd(@enum);
            return @enum;
        }

        public static void Create<T>(string name, T value) where T : Enum
        {
            EnumUtils.Create(name, value);
            Enums.SafeAdd(value);
        }

        public static void Remove<T>(string name) where T : Enum
        {
            T @enum = EnumUtils.Parse<T>(name);
            Enums.Remove(@enum);
            EnumUtils.Remove<T>(name);
        }

        public static void Remove<T>(T value) where T : Enum
        {
            Enums.Remove(value);
            EnumUtils.Remove<T>(value);
        }
    }
}
