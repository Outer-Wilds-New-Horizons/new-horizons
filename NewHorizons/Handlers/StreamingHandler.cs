using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace NewHorizons.Handlers
{
    /// <summary>
    /// handles streaming meshes so they stay loaded
    /// </summary>
    public static class StreamingHandler
    {
        private static Dictionary<Material, string> _materialCache;
        private static Dictionary<GameObject, List<string>> _objectCache;

        public static void Init()
        {
            _materialCache = new Dictionary<Material, string>();
            _objectCache = new Dictionary<GameObject, List<string>>();
        }

        public static void OnOccupantEnterSector(GameObject obj, SectorDetector sd, Sector sector)
        {
            HookStreaming(obj);

            // If its too laggy put this back idk
            /*
            if (sector.GetOccupants().Count > 0 || sd._occupantType == DynamicOccupant.Player)
            {
                LoadObject(obj);
            }
            */
        }

        /// <summary>
        /// makes it so that this object's streaming stuff will be connected to the given sector
        /// </summary>
        public static void HookStreaming(GameObject obj, Sector sector = null)
        {
            var assetBundles = new List<string>();

            if (_objectCache.ContainsKey(obj))
            {
                assetBundles = _objectCache[obj];
            }
            else
            {
                var tables = Resources.FindObjectsOfTypeAll<StreamingMaterialTable>();
                foreach (var streamingHandle in obj.GetComponentsInChildren<StreamingMeshHandle>())
                {
                    var assetBundle = streamingHandle.assetBundle;
                    if (!assetBundles.Contains(assetBundle))
                    {
                        assetBundles.Add(assetBundle);
                    }
                    if (streamingHandle is StreamingRenderMeshHandle or StreamingSkinnedMeshHandle)
                    {
                        var materials = streamingHandle.GetComponent<Renderer>().sharedMaterials;

                        if (materials.Length == 0) continue;

                        // Gonna assume that if theres more than one material its probably in the same asset bundle anyway right
                        if (_materialCache.TryGetValue(materials[0], out assetBundle))
                        {
                            assetBundles.Add(assetBundle);
                        }
                        else
                        {
                            foreach (var table in tables)
                            {
                                foreach (var x in table._materialPropertyLookups)
                                {
                                    if (materials.Contains(x.material))
                                    {
                                        _materialCache.SafeAdd(x.material, table.assetBundle);
                                        assetBundles.SafeAdd(table.assetBundle);
                                    }
                                }
                            }
                        }
                    }
                }
                _objectCache[obj] = assetBundles;
            }

            foreach (var assetBundle in assetBundles)
            {
                StreamingManager.LoadStreamingAssets(assetBundle);
            }
        }
    }
}
