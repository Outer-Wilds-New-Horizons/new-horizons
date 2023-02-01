using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NewHorizons.Utility
{
    public class Cache : Dictionary<string, object>
    {
        [NonSerialized] string filepath;

        public Cache(string cacheFilePath) 
        {
            filepath = cacheFilePath;
            var existingEntries = NewHorizons.Main.Instance.ModHelper.Storage.Load<Dictionary<string, ISerializable>>(filepath);

            if (existingEntries == null) return;

            foreach(var entry in existingEntries)
            {
                this[entry.Key] = entry.Value;
            }
        }

        public void WriteToFile() 
        { 
            NewHorizons.Main.Instance.ModHelper.Storage.Save<Dictionary<string, object>>(this, filepath);  
        }
    }
}
