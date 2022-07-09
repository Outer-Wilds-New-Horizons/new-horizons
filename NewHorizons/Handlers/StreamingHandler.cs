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

        /// <summary>
        /// makes it so that this object's streaming stuff will be connected to the given sector
        /// </summary>
        public static void SetUpStreaming(GameObject obj, Sector sector)
        {
            // find the asset bundles to load
            List<string> assetBundles;
            if (_objectCache.ContainsKey(obj))
            {
                assetBundles = _objectCache[obj];
            }
            else
            {
                assetBundles = new List<string>();

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

            if (sector)
            {
                // load it if ur already in the sector
                if (sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                {
                    foreach (var assetBundle in assetBundles)
                    {
                        StreamingManager.LoadStreamingAssets(assetBundle);
                    }
                }

                sector.OnSectorOccupantsUpdated += () =>
                {
                    if (sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                    {
                        foreach (var assetBundle in assetBundles)
                        {
                            StreamingManager.LoadStreamingAssets(assetBundle);
                        }
                    }
                    else
                    {
                        foreach (var assetBundle in assetBundles)
                        {
                            StreamingManager.UnloadStreamingAssets(assetBundle);
                        }
                    }
                };
            }
            else
            {
                // just load it immediately and hope for the best
                foreach (var assetBundle in assetBundles)
                {
                    StreamingManager.LoadStreamingAssets(assetBundle);
                }
            }
        }
    }
}
