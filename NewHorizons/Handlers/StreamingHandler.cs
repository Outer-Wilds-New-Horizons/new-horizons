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
        
        // Cache for StreamingMaterialTable to avoid repeated expensive lookups
        private static StreamingMaterialTable[] _cachedMaterialTables = null;

        public static void Init()
        {
            _materialCache.Clear();
            _objectCache.Clear();
            _sectorCache.Clear();
            _cachedMaterialTables = null;
        }
        
        /// <summary>
        /// Get cached material tables to avoid expensive Resources.FindObjectsOfTypeAll calls
        /// </summary>
        private static StreamingMaterialTable[] GetMaterialTables()
        {
            if (_cachedMaterialTables == null)
            {
                _cachedMaterialTables = Resources.FindObjectsOfTypeAll<StreamingMaterialTable>();
            }
            return _cachedMaterialTables;
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
            PerformanceProfiler.StartTimer("StreamingHandler.SetUpStreaming");
            
            Profiler.BeginSample("get bundles");
            PerformanceProfiler.StartTimer("StreamingHandler.GetBundles");
            // find the asset bundles to load
            // tries the cache first, then builds
            if (!_objectCache.TryGetValue(obj, out var assetBundles))
            {
                var assetBundlesList = new List<string>();

                // Use cached tables to avoid expensive repeated lookups
                var tables = GetMaterialTables();
                var streamingHandles = obj.GetComponentsInChildren<StreamingMeshHandle>();
                
                // Process all streaming handles
                foreach (var streamingHandle in streamingHandles)
                {
                    var assetBundle = streamingHandle.assetBundle;
                    assetBundlesList.SafeAdd(assetBundle);

                    if (streamingHandle is StreamingRenderMeshHandle or StreamingSkinnedMeshHandle)
                    {
                        var renderer = streamingHandle.GetComponent<Renderer>();
                        if (renderer == null) continue;
                        
                        var materials = renderer.sharedMaterials;
                        if (materials.Length == 0) continue;

                        // Process materials more efficiently
                        ProcessMaterials(materials, tables, assetBundlesList);
                    }
                }

                assetBundles = assetBundlesList.ToArray();
                _objectCache[obj] = assetBundles;
            }
            PerformanceProfiler.StopTimer("StreamingHandler.GetBundles");
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
            
            PerformanceProfiler.StopTimer("StreamingHandler.SetUpStreaming");
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
        
        /// <summary>
        /// Efficiently process materials to find their asset bundles
        /// </summary>
        private static void ProcessMaterials(Material[] materials, StreamingMaterialTable[] tables, List<string> assetBundlesList)
        {
            // Check cache first for all materials
            foreach (var material in materials)
            {
                if (material == null) continue;
                
                if (_materialCache.TryGetValue(material, out var cachedBundle))
                {
                    assetBundlesList.SafeAdd(cachedBundle);
                }
                else
                {
                    // Find in tables and cache the result
                    foreach (var table in tables)
                    {
                        foreach (var lookup in table._materialPropertyLookups)
                        {
                            if (lookup.material == material)
                            {
                                _materialCache.SafeAdd(material, table.assetBundle);
                                assetBundlesList.SafeAdd(table.assetBundle);
                                goto NextMaterial; // Break out of both loops
                            }
                        }
                    }
                    NextMaterial:;
                }
            }
        }

        public static StreamingGroup GetStreamingGroup(AstroObject.Name name)
        {
            if (name is AstroObject.Name.CaveTwin or AstroObject.Name.TowerTwin)
            {
                var streamingGroupGO = SearchUtilities.Find("FocalBody/StreamingGroup_HGT");
                return streamingGroupGO?.GetComponent<StreamingGroup>();
            }
            else
            {
                var astroObject = Locator.GetAstroObject(name);
                return astroObject?.GetComponentInChildren<StreamingGroup>();
            }
        }
    }
}
