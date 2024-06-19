using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NewHorizons.Utility
{
    public static class SearchUtilities
    {
        private static readonly Dictionary<string, GameObject> DontDestroyOnLoadCachedGameObjects = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, GameObject> CachedGameObjects = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, GameObject> CachedRootGameObjects = new Dictionary<string, GameObject>();

        public static void AddToDontDestroyOnLoadCache(string path, GameObject go)
        {
            DontDestroyOnLoadCachedGameObjects[path] = go.InstantiateInactive().DontDestroyOnLoad();
        }

        public static void ClearCache()
        {
            NHLogger.LogVerbose("Clearing search cache");
            CachedGameObjects.Clear();
            CachedRootGameObjects.Clear();
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
            if (DontDestroyOnLoadCachedGameObjects.TryGetValue(path, out var gameObject)) return gameObject;

            if (CachedGameObjects.TryGetValue(path, out var go)) return go;

            // 1: normal find
            go = GameObject.Find(path);
            if (go)
            {
                CachedGameObjects.Add(path, go);
                return go;
            }

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
                return go;
            }

            var name = names.Last();
            if (warn) NHLogger.LogWarning($"Couldn't find object in path {path}, will look for potential matches for name {name}");
            // 3: find resource to include inactive objects (but skip prefabs)
            go = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(x => x.name == name && x.scene.name != null);
            if (go)
            {
                CachedGameObjects.Add(path, go);
                return go;
            }

            if (warn) NHLogger.LogWarning($"Couldn't find object with name {name}");
            return null;
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
    }
}
