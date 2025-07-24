using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NewHorizons.Utility
{
    /// <summary>
    /// Original SearchUtilities implementation for performance comparison
    /// This mirrors the main branch code before optimizations
    /// </summary>
    public static class SearchUtilitiesOriginal
    {
        private static readonly Dictionary<string, GameObject> DontDestroyOnLoadCachedGameObjects = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, GameObject> CachedGameObjects = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, GameObject> CachedRootGameObjects = new Dictionary<string, GameObject>();

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
            NHLogger.LogVerbose("Clearing search cache (original)");
            CachedGameObjects.Clear();
            CachedRootGameObjects.Clear();
        }

        /// <summary>
        /// ORIGINAL finds active or inactive object by path,
        /// or recursively finds an active or inactive object by name
        /// This is the main branch implementation WITHOUT optimizations
        /// </summary>
        public static GameObject Find(string path, bool warn = true)
        {
            if (DontDestroyOnLoadCachedGameObjects.TryGetValue(path, out var gameObject)) return gameObject;

            if (CachedGameObjects.TryGetValue(path, out var go)) return go;

            // 1: normal find
            Profiler.BeginSample("1-Original");
            go = GameObject.Find(path);
            if (go)
            {
                CachedGameObjects.Add(path, go);
                Profiler.EndSample();
                return go;
            }
            Profiler.EndSample();

            Profiler.BeginSample("2-Original");
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
                Profiler.EndSample();
                return go;
            }
            Profiler.EndSample();

            Profiler.BeginSample("3-Original");
            var name = names.Last();
            if (warn) NHLogger.LogWarning($"Couldn't find object in path {path}, will look for potential matches for name {name}");
            // 3: find resource to include inactive objects (but skip prefabs) - EXPENSIVE FALLBACK
            go = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(x => x.name == name && x.scene.name != null);
            if (go)
            {
                CachedGameObjects.Add(path, go);
                Profiler.EndSample();
                return go;
            }

            if (warn) NHLogger.LogWarning($"Couldn't find object with name {name}");
            Profiler.EndSample();
            return null;
        }

        public static GameObject FindChild(this GameObject g, string childPath) =>
            g.transform.Find(childPath)?.gameObject;
    }
}