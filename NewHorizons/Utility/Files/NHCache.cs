using NewHorizons.Utility.OWML;
using Newtonsoft.Json;
using OWML.Common;
using System;
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

        // Annoying to have to null check the cache for NewHorizonsBody's that have no file path
        // Dummy cache will just not have anything in it and never be saved
        private bool _dummy;

        public NHCache(IModBehaviour mod, string cacheFilePath)
        {
            _mod = mod;
            _filepath = cacheFilePath;
            var fullPath = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, cacheFilePath);

            if (File.Exists(fullPath))
            {
                try
                {
                    var json = File.ReadAllText(fullPath);
                    _data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    // The code above does exactly the same thing that the code below does, but the below for some reason always returns null. no clue why
                    // data = mod.ModHelper.Storage.Load<Dictionary<string, string>>(filepath);

                    _dirty = false;

                    return;
                }
                catch { }
            }

            // Either the file didn't exist or we threw an exception loading it
            _dirty = true;
        }

        public NHCache()
        {
            _dummy = true;
        }

        public void WriteToFile()
        {
            if (!_dummy && _data.Count > 0 && _dirty)
            {
                try
                {
                    _mod.ModHelper.Storage.Save(_data, _filepath);
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Failed to save cache {e}");
                }
            }
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
