using System;
using System.Collections.Generic;
namespace NewHorizons.Utility
{
    public static class CollectionUtilities
    {
        public static T KeyByValue<T, W>(Dictionary<T, W> dict, W val, T defaultValue = default)
        {
            T key = defaultValue;
            foreach (KeyValuePair<T, W> pair in dict)
            {
                if (EqualityComparer<W>.Default.Equals(pair.Value, val))
                {
                    key = pair.Key;
                    break;
                }
            }
            return key;
        }
    }
}
