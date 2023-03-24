using Newtonsoft.Json;
using OWML.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NewHorizons.Utility
{
    public class Cache
    {
        string filepath;
        IModBehaviour mod;
        Dictionary<string, string> data = new Dictionary<string, string>();
        HashSet<string> accessedKeys = new HashSet<string>();
        bool dirty;

        public Cache(IModBehaviour mod, string cacheFilePath) 
        {
            this.mod = mod;
            this.filepath = cacheFilePath;
            var fullPath = mod.ModHelper.Manifest.ModFolderPath + cacheFilePath;
            
            if (!File.Exists(fullPath))
            {
                data = new Dictionary<string, string>();
                dirty = true;
                return;
            }

			var json = File.ReadAllText(fullPath);
            data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            // the code above does exactly the same thing that the code below does, but the below for some reason always returns null. no clue why
            // data = mod.ModHelper.Storage.Load<Dictionary<string, string>>(filepath);

            dirty = false;
        }

        public void WriteToFile() 
        {
            if (data.Count <= 0) return; // don't write empty caches
            if (!dirty) return; // don't write unmodified caches
            mod.ModHelper.Storage.Save<Dictionary<string, string>>(data, filepath);
        }

        public bool ContainsKey(string key) 
        {
            return data.ContainsKey(key);
        }

        public T Get<T>(string key)
        {
            accessedKeys.Add(key);
            var json = data[key];
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void Set<T>(string key, T value)
        {
            dirty = true;
            accessedKeys.Add(key);
            data[key] = JsonConvert.SerializeObject(value);
        }

        public void ClearUnaccessed() 
        {
            var keys = data.Keys.ToList();
            foreach(var key in keys) 
            {
                if (accessedKeys.Contains(key)) continue;
                data.Remove(key);
            }
        }
    }
}
