using Newtonsoft.Json;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace NewHorizons.Utility
{
    public class Cache
    {
        string filepath;
        IModBehaviour mod;
        Dictionary<string, string> data = new Dictionary<string, string>();

        public Cache(IModBehaviour mod, string cacheFilePath) 
        {
            this.mod = mod;

            filepath = cacheFilePath;
            
			var json = File.ReadAllText(mod.ModHelper.Manifest.ModFolderPath + cacheFilePath);
            data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            // the above is exactly the same thing that the below does, but the below for some reason always returns null. no clue why
            // data = mod.ModHelper.Storage.Load<Dictionary<string, string>>(filepath);

            if (data == null)
            {
                Logger.LogWarning("Failed to load cache! Cache path: " + cacheFilePath);
                data = new Dictionary<string, string>();
            }

            Logger.LogWarning("CACHE DEBUG: Cache path: " + cacheFilePath);
            Logger.LogWarning("CACHE DEBUG: Loaded cache == null? " + (data == null));
            Logger.LogWarning("CACHE DEBUG: Loaded cache keys: " + String.Join(",", data?.Keys ?? new Dictionary<string, string>().Keys));
        }

        public void WriteToFile() 
        { 
            mod.ModHelper.Storage.Save<Dictionary<string, string>>(data, filepath);
        }

        public bool ContainsKey(string key) 
        {
            return data.ContainsKey(key);
        }

        public T Get<T>(string key)
        {
            var json = data[key];
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void Set<T>(string key, T value)
        {
            data[key] = JsonConvert.SerializeObject(value);
        }
    }
}
