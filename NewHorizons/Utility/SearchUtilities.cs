using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Newtonsoft.Json;

namespace NewHorizons.Utility
{
    public static class SearchUtilities
    {
        private static readonly Dictionary<string, GameObject> DontDestroyOnLoadCachedGameObjects = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, GameObject> CachedGameObjects = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, GameObject> CachedRootGameObjects = new Dictionary<string, GameObject>();
        
        // Disk-based cache for persistent scene hierarchy paths
        private static readonly Dictionary<string, string> DiskCachedPaths = new Dictionary<string, string>();
        private static readonly string CacheFilePath = Path.Combine(Application.persistentDataPath, "NH_SearchCache.json");
        private static bool _diskCacheLoaded = false;

        public static void AddToDontDestroyOnLoadCache(string path, GameObject go)
        {
            DontDestroyOnLoadCachedGameObjects[path] = go.InstantiateInactive().DontDestroyOnLoad();
        }

        public static void ClearDontDestroyOnLoadCache()
        {
            foreach (var go in DontDestroyOnLoadCachedGameObjects.Values)
            {
                GameObject.Destroy(go);
            }
            DontDestroyOnLoadCachedGameObjects.Clear();
        }

        public static void ClearCache()
        {
            NHLogger.LogVerbose("Clearing search cache");
            CachedGameObjects.Clear();
            CachedRootGameObjects.Clear();
            
            // Save disk cache when clearing memory cache
            SaveDiskCache();
        }
        
        /// <summary>
        /// Load the disk-based cache from persistent storage
        /// </summary>
        private static void LoadDiskCache()
        {
            if (_diskCacheLoaded) return;
            
            try
            {
                if (File.Exists(CacheFilePath))
                {
                    var json = File.ReadAllText(CacheFilePath);
                    var diskCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (diskCache != null)
                    {
                        foreach (var kvp in diskCache)
                        {
                            DiskCachedPaths[kvp.Key] = kvp.Value;
                        }
                        NHLogger.LogVerbose($"Loaded {DiskCachedPaths.Count} cached paths from disk");
                    }
                }
            }
            catch (System.Exception ex)
            {
                NHLogger.LogError($"Failed to load search cache: {ex.Message}");
            }
            finally
            {
                _diskCacheLoaded = true;
            }
        }
        
        /// <summary>
        /// Save the disk-based cache to persistent storage
        /// </summary>
        public static void SaveDiskCache()
        {
            try
            {
                var json = JsonConvert.SerializeObject(DiskCachedPaths, Formatting.Indented);
                File.WriteAllText(CacheFilePath, json);
                NHLogger.LogVerbose($"Saved {DiskCachedPaths.Count} cached paths to disk");
            }
            catch (System.Exception ex)
            {
                NHLogger.LogError($"Failed to save search cache: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Add a path mapping to the disk cache
        /// </summary>
        private static void CachePath(string searchName, string fullPath)
        {
            if (!string.IsNullOrEmpty(searchName) && !string.IsNullOrEmpty(fullPath))
            {
                DiskCachedPaths[searchName] = fullPath;
            }
        }

        public static List<T> FindObjectsOfTypeAndName<T>(string name) where T : Object
        {
            T[] firstList = Object.FindObjectsOfType<T>();
            List<T> finalList = new List<T>();

            for (var i = 0; i < firstList.Length; i++)
            {
                if (firstList[i].name == name)
                {
                    finalList.Add(firstList[i]);
                }
            }

            return finalList;
        }

        public static T FindObjectOfTypeAndName<T>(string name) where T : Object
        {
            T[] firstList = Object.FindObjectsOfType<T>();

            for (var i = 0; i < firstList.Length; i++)
            {
                if (firstList[i].name == name)
                {
                    return firstList[i];
                }
            }

            return null;
        }

        public static List<T> FindResourcesOfTypeAndName<T>(string name) where T : Object
        {
            T[] firstList = Resources.FindObjectsOfTypeAll<T>();
            List<T> finalList = new List<T>();

            for (var i = 0; i < firstList.Length; i++)
            {
                if (firstList[i].name == name)
                {
                    finalList.Add(firstList[i]);
                }
            }

            return finalList;
        }

        public static T FindResourceOfTypeAndName<T>(string name) where T : Object
        {
            T[] firstList = Resources.FindObjectsOfTypeAll<T>();

            for (var i = 0; i < firstList.Length; i++)
            {
                if (firstList[i].name == name)
                {
                    return firstList[i];
                }
            }

            return null;
        }

        public static string GetPath(this Transform current)
        {
            if (current.parent == null) return current.name;
            return current.parent.GetPath() + "/" + current.name;
        }

        public static GameObject FindChild(this GameObject g, string childPath) =>
            g.transform.Find(childPath)?.gameObject;

        /// <summary>
        /// finds active or inactive object by path,
        /// or recursively finds an active or inactive object by name
        /// </summary>
        public static GameObject Find(string path, bool warn = true)
        {
            PerformanceProfiler.StartTimer("SearchUtilities.Find");
            
            try
            {
                if (DontDestroyOnLoadCachedGameObjects.TryGetValue(path, out var gameObject)) 
                {
                    PerformanceProfiler.StopTimer("SearchUtilities.Find");
                    PerformanceProfiler.StartTimer("SearchUtilities.Find.DontDestroyCache");
                    PerformanceProfiler.StopTimer("SearchUtilities.Find.DontDestroyCache");
                    return gameObject;
                }

                if (CachedGameObjects.TryGetValue(path, out var go)) 
                {
                    PerformanceProfiler.StopTimer("SearchUtilities.Find");
                    PerformanceProfiler.StartTimer("SearchUtilities.Find.MemoryCache");
                    PerformanceProfiler.StopTimer("SearchUtilities.Find.MemoryCache");
                    return go;
                }

                // Load disk cache if not already loaded
                LoadDiskCache();

                // 1: normal find
                Profiler.BeginSample("1-Direct");
                PerformanceProfiler.StartTimer("SearchUtilities.Find.DirectFind");
                go = GameObject.Find(path);
                if (go)
                {
                    CachedGameObjects.Add(path, go);
                    CachePath(path.Split('/').Last(), go.transform.GetPath());
                    PerformanceProfiler.StopTimer("SearchUtilities.Find.DirectFind");
                    Profiler.EndSample();
                    PerformanceProfiler.StopTimer("SearchUtilities.Find");
                    return go;
                }
                PerformanceProfiler.StopTimer("SearchUtilities.Find.DirectFind");
                Profiler.EndSample();

                Profiler.BeginSample("2-RootCache");
                PerformanceProfiler.StartTimer("SearchUtilities.Find.RootCache");
                // 2: find inactive using root + transform.find
                var names = path.Split('/');

                // Cache the root objects so we don't loop through all of them each time
                var rootName = names[0];
                if (!CachedRootGameObjects.TryGetValue(rootName, out var root))
                {
                    root = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == rootName);
                    if (root != null)
                    {
                        CachedRootGameObjects.Add(rootName, root);
                    }
                }

                var childPath = string.Join("/", names.Skip(1));
                go = root ? root.FindChild(childPath) : null;
                if (go)
                {
                    CachedGameObjects.Add(path, go);
                    CachePath(names.Last(), go.transform.GetPath());
                    PerformanceProfiler.StopTimer("SearchUtilities.Find.RootCache");
                    Profiler.EndSample();
                    PerformanceProfiler.StopTimer("SearchUtilities.Find");
                    return go;
                }
                PerformanceProfiler.StopTimer("SearchUtilities.Find.RootCache");
                Profiler.EndSample();

                Profiler.BeginSample("3-DiskCache");
                PerformanceProfiler.StartTimer("SearchUtilities.Find.DiskCache");
                // 3: Check disk cache for known path mappings
                var name = names.Last();
                if (DiskCachedPaths.TryGetValue(name, out var cachedPath))
                {
                    go = GameObject.Find(cachedPath);
                    if (go == null)
                    {
                        // Try with root + transform.find for inactive objects
                        var cachedNames = cachedPath.Split('/');
                        if (cachedNames.Length > 0)
                        {
                            var cachedRootName = cachedNames[0];
                            if (!CachedRootGameObjects.TryGetValue(cachedRootName, out var cachedRoot))
                            {
                                cachedRoot = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == cachedRootName);
                                if (cachedRoot != null)
                                {
                                    CachedRootGameObjects.Add(cachedRootName, cachedRoot);
                                }
                            }
                            
                            if (cachedRoot && cachedNames.Length > 1)
                            {
                                var cachedChildPath = string.Join("/", cachedNames.Skip(1));
                                go = cachedRoot.FindChild(cachedChildPath);
                            }
                        }
                    }
                    
                    if (go)
                    {
                        CachedGameObjects.Add(path, go);
                        PerformanceProfiler.StopTimer("SearchUtilities.Find.DiskCache");
                        Profiler.EndSample();
                        PerformanceProfiler.StopTimer("SearchUtilities.Find");
                        return go;
                    }
                    else
                    {
                        // Remove invalid cache entry
                        DiskCachedPaths.Remove(name);
                    }
                }
                PerformanceProfiler.StopTimer("SearchUtilities.Find.DiskCache");
                Profiler.EndSample();

                Profiler.BeginSample("4-ResourceSearch");
                PerformanceProfiler.StartTimer("SearchUtilities.Find.ResourceSearch");
                if (warn) NHLogger.LogWarning($"Couldn't find object in path {path}, will look for potential matches for name {name}");
                // 4: find resource to include inactive objects (but skip prefabs) - EXPENSIVE FALLBACK
                go = Resources.FindObjectsOfTypeAll<GameObject>()
                    .FirstOrDefault(x => x.name == name && x.scene.name != null);
                if (go)
                {
                    CachedGameObjects.Add(path, go);
                    CachePath(name, go.transform.GetPath());
                    PerformanceProfiler.StopTimer("SearchUtilities.Find.ResourceSearch");
                    Profiler.EndSample();
                    PerformanceProfiler.StopTimer("SearchUtilities.Find");
                    return go;
                }

                if (warn) NHLogger.LogWarning($"Couldn't find object with name {name}");
                PerformanceProfiler.StopTimer("SearchUtilities.Find.ResourceSearch");
                Profiler.EndSample();
                PerformanceProfiler.StopTimer("SearchUtilities.Find");
                return null;
            }
            finally
            {
                // Ensure timer is stopped even if an exception occurs
                if (PerformanceProfiler.IsTimerActive("SearchUtilities.Find"))
                {
                    PerformanceProfiler.StopTimer("SearchUtilities.Find");
                }
            }
        }

        public static List<GameObject> GetAllChildren(this GameObject parent)
        {
            var children = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                children.Add(child.gameObject);
            }
            return children;
        }

        /// <summary>
        /// transform.find but works for gameobjects with same name
        /// </summary>
        public static List<Transform> FindAll(this Transform @this, string path)
        {
            var names = path.Split('/');
            var currentTransforms = new List<Transform> { @this };
            foreach (var name in names)
            {
                var newTransforms = new List<Transform>();
                foreach (var currentTransform in currentTransforms)
                {
                    foreach (Transform child in currentTransform)
                    {
                        if (child.name == name)
                        {
                            newTransforms.Add(child);
                        }
                    }
                }
                currentTransforms = newTransforms;
            }

            return currentTransforms;
        }
    }
}
