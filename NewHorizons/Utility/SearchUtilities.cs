using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        public static GameObject CachedFind(string path)
        {
            if (CachedGameObjects.ContainsKey(path))
            {
                return CachedGameObjects[path];
            }
            else
            {
                GameObject foundObject = GameObject.Find(path);
                if (foundObject != null)
                {
                    CachedGameObjects.Add(path, foundObject);
                }
                return foundObject;
            }
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
            List<T> finalList = new List<T>();

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

        public static string GetPath(Transform current)
        {
            if (current.parent == null) return current.name;
            return GetPath(current.parent) + "/" + current.name;
        }

        /*
        public static GameObject Find(string path)
        {
            var go = GameObject.Find(path);
            if (go != null) return go;

            var names = path.Split(new char[] { '\\', '/' });

            foreach (var possibleMatch in FindObjectsOfTypeAndName<GameObject>(names.Last()))
            {
                Logger.LogPath(possibleMatch);
                if (GetPath(possibleMatch.transform) == path) return possibleMatch;
            }

            return null;
        }
        */

        public static GameObject FindChild(GameObject g, string childName)
        {
            foreach(Transform child in g.transform)
            {
                if (child.gameObject.name == childName) return child.gameObject;
            }

            return null;
        }

        public static GameObject Find(string path, bool warn = true)
        {
            if (CachedGameObjects.ContainsKey(path))
            {
                return CachedGameObjects[path];
            }
            try
            {
                var go = GameObject.Find(path);

                var names = path.Split(new char[] { '\\', '/' });
                if (go == null)
                {

                    // Get the root object and hope its the right one
                    var root = GameObject.Find(names[0]);
                    if (root == null) root = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Where(x => x.name.Equals(names[0])).FirstOrDefault();

                    var t = root?.transform;
                    if (t == null)
                    {
                        if (warn) Logger.LogWarning($"Couldn't find root object in path ({names[0]})");
                    }
                    else
                    {
                        for (int i = 1; i < names.Length; i++)
                        {
                            var child = t.transform.Find(names[i]);

                            if (child == null)
                            {
                                foreach (Transform c in t.GetComponentsInChildren<Transform>(true))
                                {
                                    if (c.name.Equals(names[i]))
                                    {
                                        child = c;
                                        break;
                                    }
                                }
                            }

                            if (child == null)
                            {
                                if (warn) Logger.LogWarning($"Couldn't find object in path ({names[i]})");
                                t = null;
                                break;
                            }

                            t = child;
                        }
                    }

                    go = t?.gameObject;
                }

                if (go == null)
                {
                    var name = names.Last();
                    if (warn) Logger.LogWarning($"Couldn't find object {path}, will look for potential matches for name {name}");
                    go = FindObjectOfTypeAndName<GameObject>(name);
                }

                if (go != null)
                {
                    CachedGameObjects.Add(path, go);
                }

                return go;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<GameObject> GetAllChildren(GameObject parent)
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
