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
        private static readonly Dictionary<Material, string> _materialCache = new();
        private static readonly Dictionary<GameObject, List<string>> _objectCache = new();
        private static readonly Dictionary<string, List<Sector>> _sectorCache = new();
        private static StreamingMaterialTable[] _tables;

        public static void Init()
        {
            _materialCache.Clear();
            _objectCache.Clear();
            _sectorCache.Clear();

            _tables = Resources.FindObjectsOfTypeAll<StreamingMaterialTable>();

            // Track base game assets
            foreach (var streamingGroup in GameObject.FindObjectsOfType<StreamingGroup>())
            {
                var sector = streamingGroup.GetAttachedOWRigidbody().GetComponentInChildren<Sector>();
                var assetBundles = streamingGroup._streamingMaterialTables.Select(x => x.assetBundle);

                foreach (var assetBundle in assetBundles)
                {
                    if (!_sectorCache.TryGetValue(assetBundle, out var sectors))
                    {
                        sectors = new List<Sector>();
                        _sectorCache.Add(assetBundle, sectors);
                    }
                    sectors.SafeAdd(sector);
                }
            }
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
                sector.OnOccupantExitSector += _ =>
                {
                    if (!sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                        group.ReleaseGeneralAssets();
                };
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
            // find the asset bundles to load
            // tries the cache first, then builds
            if (!_objectCache.TryGetValue(obj, out var assetBundles))
            {
                assetBundles = new List<string>();

                foreach (var streamingHandle in obj.GetComponentsInChildren<StreamingMeshHandle>())
                {
                    var assetBundle = streamingHandle.assetBundle;
                    assetBundles.SafeAdd(assetBundle);

                    if (streamingHandle is StreamingRenderMeshHandle or StreamingSkinnedMeshHandle)
                    {
                        var materials = streamingHandle.GetComponent<Renderer>().sharedMaterials;

                        if (materials.Length == 0) continue;

                        // Gonna assume that if theres more than one material its probably in the same asset bundle anyway right
                        if (_materialCache.TryGetValue(materials[0], out assetBundle))
                        {
                            assetBundles.SafeAdd(assetBundle);
                        }
                        else
                        {
                            foreach (var table in _tables)
                            {
                                foreach (var lookup in table._materialPropertyLookups)
                                {
                                    if (materials.Contains(lookup.material))
                                    {
                                        _materialCache.SafeAdd(lookup.material, table.assetBundle);
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
                // Track the sector even if its null. null means stay loaded forever
                if (!_sectorCache.TryGetValue(assetBundle, out var sectors))
                {
                    sectors = new List<Sector>();
                    _sectorCache.Add(assetBundle, sectors);
                }
                sectors.SafeAdd(sector);
            }

            if (sector)
            {
                sector.OnOccupantEnterSector += _ =>
                {
                    if (sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                        foreach (var assetBundle in assetBundles)
                            StreamingManager.LoadStreamingAssets(assetBundle);
                };
                sector.OnOccupantExitSector += _ =>
                {
                    if (!sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                        foreach (var assetBundle in assetBundles)
                            if (!IsBundleInUse(assetBundle))
                                StreamingManager.UnloadStreamingAssets(assetBundle);
                };
            }
            else
            {
                foreach (var assetBundle in assetBundles)
                    StreamingManager.LoadStreamingAssets(assetBundle);
            }
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
                return GameObject.Find("FocalBody/StreamingGroup_HGT").GetComponent<StreamingGroup>();
            }
            else
            {
                return Locator.GetAstroObject(name).GetComponentInChildren<StreamingGroup>();
            }
        }
    }
}