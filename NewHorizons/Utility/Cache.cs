using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NewHorizons.Utility
{
    public class Cache : Dictionary<string, Object>
    {
        [NonSerialized] string filepath;

        public Cache(string cacheFilePath) 
        {
            filepath = cacheFilePath;
            var existingEntries = NewHorizons.Main.Instance.ModHelper.Storage.Load<Dictionary<string, Object>>(filepath);

            Logger.LogWarning("CACHE DEBUG: Cache path: " + cacheFilePath);
            Logger.LogWarning("CACHE DEBUG: Loaded cache == null? " + (existingEntries == null));
            Logger.LogWarning("CACHE DEBUG: Loaded cache keys: " + String.Join(",", existingEntries?.Keys));

            if (existingEntries == null) return;

            foreach(var entry in existingEntries)
            {
                this[entry.Key] = entry.Value;
            }
        }

        public void WriteToFile() 
        { 
            NewHorizons.Main.Instance.ModHelper.Storage.Save<Dictionary<string, Object>>(this, filepath);  
        }
    }
}
