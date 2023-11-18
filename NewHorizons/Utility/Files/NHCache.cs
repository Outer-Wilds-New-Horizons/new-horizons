using Newtonsoft.Json;
using OWML.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NewHorizons.Utility.Files
{
    public class NHCache
    {
        private string _filepath;
        private IModBehaviour _mod;
        private Dictionary<string, string> _data = new();
        private HashSet<string> _accessedKeys = new();
        private bool _dirty;

        public NHCache(IModBehaviour mod, string cacheFilePath)
        {
            _mod = mod;
            _filepath = cacheFilePath;
            var fullPath = mod.ModHelper.Manifest.ModFolderPath + cacheFilePath;

            if (!File.Exists(fullPath))
            {
                _data = new Dictionary<string, string>();
                _dirty = true;
            }
            else
            {
                var json = File.ReadAllText(fullPath);
                _data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                // The code above does exactly the same thing that the code below does, but the below for some reason always returns null. no clue why
                // data = mod.ModHelper.Storage.Load<Dictionary<string, string>>(filepath);

                _dirty = false;
            }
        }

        public void WriteToFile()
        {
            if (_data.Count <= 0)
            {
                return; // don't write empty caches
            }
            if (!_dirty)
            {
                return; // don't write unmodified caches
            }
            _mod.ModHelper.Storage.Save(_data, _filepath);
        }

        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        public T Get<T>(string key)
        {
            _accessedKeys.Add(key);
            var json = _data[key];
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void Set<T>(string key, T value)
        {
            _dirty = true;
            _accessedKeys.Add(key);
            _data[key] = JsonConvert.SerializeObject(value);
        }

        public void ClearUnaccessed()
        {
            var keys = _data.Keys.ToList();
            foreach (var key in keys)
            {
                if (_accessedKeys.Contains(key)) continue;
                _data.Remove(key);
            }
        }
    }
}
