#region

using System.Collections.Generic;

#endregion

namespace NewHorizons.Utility
{
    public static class CollectionUtilities
    {
        public static T KeyByValue<T, W>(Dictionary<T, W> dict, W val)
        {
            T key = default;
            foreach (var pair in dict)
                if (EqualityComparer<W>.Default.Equals(pair.Value, val))
                {
                    key = pair.Key;
                    break;
                }

            return key;
        }
    }
}