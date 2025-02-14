using NewHorizons.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace NewHorizons.Handlers
{
    /// <summary>
    /// handles streaming meshes so they stay loaded
    /// </summary>
    public static class StreamingHandler
    {
        private static readonly Dictionary<Material, string> _materialCache = new();
        private static readonly Dictionary<GameObject, string[]> _objectCache = new();
        private static readonly Dictionary<string, List<Sector>> _sectorCache = new();

        public static void Init()
        {
            _materialCache.Clear();
            _objectCache.Clear();
            _sectorCache.Clear();
        }

        public static void SetUpStreaming(AstroObject.Name name, Sector sector)
        {
            var group = GetStreamingGroup(name);

            if (sector)
            {
                sector.OnOccupantEnterSector += _ =>
                {
                    if (sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                        group.RequestGeneralAssets();
                };
                /*
                sector.OnOccupantExitSector += _ =>
                {
                    if (!sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                        group.ReleaseGeneralAssets();
                };
                */
            }
            else
            {
                group.RequestGeneralAssets();
            }
        }

        /// <summary>
        /// makes it so that this object's streaming stuff will be connected to the given sector
        /// </summary>
        public static void SetUpStreaming(GameObject obj, Sector sector)
        {
            // TODO: used OFTEN by detail builder. 20-40ms adds up to seconds. speed up!

            Profiler.BeginSample("get bundles");
            // find the asset bundles to load
            // tries the cache first, then builds
            if (!_objectCache.TryGetValue(obj, out var assetBundles))
            {
                var assetBundlesList = new List<string>();

                var tables = Resources.FindObjectsOfTypeAll<StreamingMaterialTable>();
                foreach (var streamingHandle in obj.GetComponentsInChildren<StreamingMeshHandle>())
                {
                    var assetBundle = streamingHandle.assetBundle;
                    assetBundlesList.SafeAdd(assetBundle);

                    if (streamingHandle is StreamingRenderMeshHandle or StreamingSkinnedMeshHandle)
                    {
                        var materials = streamingHandle.GetComponent<Renderer>().sharedMaterials;

                        if (materials.Length == 0) continue;

                        // Gonna assume that if theres more than one material its probably in the same asset bundle anyway right
                        if (_materialCache.TryGetValue(materials[0], out assetBundle))
                        {
                            assetBundlesList.SafeAdd(assetBundle);
                        }
                        else
                        {
                            foreach (var table in tables)
                            {
                                foreach (var lookup in table._materialPropertyLookups)
                                {
                                    if (materials.Contains(lookup.material))
                                    {
                                        _materialCache.SafeAdd(lookup.material, table.assetBundle);
                                        assetBundlesList.SafeAdd(table.assetBundle);
                                    }
                                }
                            }
                        }
                    }
                }

                assetBundles = assetBundlesList.ToArray();
                _objectCache[obj] = assetBundles;
            }
            Profiler.EndSample();

            Profiler.BeginSample("get sectors");
            foreach (var assetBundle in assetBundles)
            {
                // Track the sector even if its null. null means stay loaded forever
                if (!_sectorCache.TryGetValue(assetBundle, out var sectors))
                {
                    sectors = new List<Sector>();
                    _sectorCache.Add(assetBundle, sectors);
                }
                sectors.SafeAdd(sector);
            }
            Profiler.EndSample();

            Profiler.BeginSample("load assets");
            if (sector)
            {
                sector.OnOccupantEnterSector += _ =>
                {
                    if (sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                        foreach (var assetBundle in assetBundles)
                            StreamingManager.LoadStreamingAssets(assetBundle);
                };
                /*
                sector.OnOccupantExitSector += _ =>
                {
                    if (!sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                        foreach (var assetBundle in assetBundles)
                            StreamingManager.UnloadStreamingAssets(assetBundle);
                };
                */
            }
            else
            {
                foreach (var assetBundle in assetBundles)
                    StreamingManager.LoadStreamingAssets(assetBundle);
            }
            Profiler.EndSample();
        }

        public static bool IsBundleInUse(string assetBundle)
        {
            if (_sectorCache.TryGetValue(assetBundle, out var sectors))
                foreach (var sector in sectors)
                    // If a sector in the list is null then it is always in use
                    if (sector == null || sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                        return true;
            return false;
        }

        public static StreamingGroup GetStreamingGroup(AstroObject.Name name)
        {
            if (name is AstroObject.Name.CaveTwin or AstroObject.Name.TowerTwin)
            {
                return SearchUtilities.Find("FocalBody/StreamingGroup_HGT").GetComponent<StreamingGroup>();
            }
            else
            {
                return Locator.GetAstroObject(name).GetComponentInChildren<StreamingGroup>();
            }
        }
    }
}
