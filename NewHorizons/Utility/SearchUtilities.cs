using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NewHorizons.Utility
{
    public static class SearchUtilities
    {
        private static readonly Dictionary<string, GameObject> CachedGameObjects = new Dictionary<string, GameObject>();

        public static void ClearCache()
        {
            Logger.Log("Clearing search cache");
            CachedGameObjects.Clear();
        }

        public static List<T> FindObjectsOfTypeAndName<T>(string name) where T : Object
        {
            T[] firstList = GameObject.FindObjectsOfType<T>();
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
            T[] firstList = GameObject.FindObjectsOfType<T>();

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

        public static GameObject FindChild(this GameObject g, string path) =>
            g.transform.Find(path)?.gameObject;

        public static GameObject Find(string path, bool warn = true)
        {
            if (CachedGameObjects.TryGetValue(path, out var go)) return go;

            var paths = path.Split(new[] { '/' }, 1);
            var root = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == paths[0]);
            if (root == null)
            {
                if (warn) Logger.LogWarning($"Couldn't find root object in path ({path})");
                return null;
            }

            go = root.FindChild(paths[1]);
            if (go == null)
            {
                if (warn) Logger.LogWarning($"Couldn't find object in path ({path})");
            }

            CachedGameObjects.Add(path, go);
            return go;
        }

        public static List<GameObject> GetAllChildren(this GameObject parent)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                children.Add(child.gameObject);
            }
            return children;
        }
    }
}
