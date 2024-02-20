using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System;
using UnityEngine;

namespace NewHorizons.Builder.Atmosphere
{
    public static class EffectsBuilder
    {
        private static GameObject _rainEmitterPrefab;
        private static GameObject _snowEmitterPrefab;
        private static GameObject _snowLightEmitterPrefab;
        private static GameObject _emberEmitterPrefab;
        private static GameObject _leafEmitterPrefab;
        private static GameObject _planktonEmitterPrefab;
        private static GameObject _bubbleEmitterPrefab;
        private static GameObject _currentEmitterPrefab;
        private static GameObject _cloudEmitterPrefab;
        private static GameObject _crawliesEmitterPrefab;
        private static GameObject _firefliesEmitterPrefab;
        private static GameObject _pollenEmitterPrefab;
        private static GameObject _iceMoteEmitterPrefab;
        private static GameObject _rockMoteEmitterPrefab;
        private static GameObject _crystalMoteEmitterPrefab;
        private static GameObject _sandMoteEmitterPrefab;
        private static GameObject _fogEmitterPrefab;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_rainEmitterPrefab == null) _rainEmitterPrefab = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/Effects_GD_Rain").InstantiateInactive().Rename("Prefab_Effects_Rain").DontDestroyOnLoad();
            if (_snowEmitterPrefab == null) _snowEmitterPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Effects_BH/Effects_BH_Snowflakes").InstantiateInactive().Rename("Prefab_Effects_Snowflakes").DontDestroyOnLoad();
            if (_snowLightEmitterPrefab == null) _snowLightEmitterPrefab = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Effects_TH/Effects_TH_Snowflakes").InstantiateInactive().Rename("Prefab_Effects_SnowflakesLight").DontDestroyOnLoad();
            if (_emberEmitterPrefab == null) _emberEmitterPrefab = SearchUtilities.Find("VolcanicMoon_Body/Sector_VM/Effects_VM/Effects_VM_Embers").InstantiateInactive().Rename("Prefab_Effects_Embers").DontDestroyOnLoad();
            if (_leafEmitterPrefab == null) _leafEmitterPrefab = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/Effects_GD_Leaves").InstantiateInactive().Rename("Prefab_Effects_Leaves").DontDestroyOnLoad();
            if (_planktonEmitterPrefab == null) _planktonEmitterPrefab = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/Effects_GD_Plankton").InstantiateInactive().Rename("Prefab_Effects_Plankton").DontDestroyOnLoad();
            if (_bubbleEmitterPrefab == null) _bubbleEmitterPrefab = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/Effects_GD_Bubbles").InstantiateInactive().Rename("Prefab_Effects_Bubbles").DontDestroyOnLoad();
            if (_currentEmitterPrefab == null) _currentEmitterPrefab = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/Effects_GD_RadialCurrent").InstantiateInactive().Rename("Prefab_Effects_Current").DontDestroyOnLoad();
            if (_cloudEmitterPrefab == null) _cloudEmitterPrefab = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Effects_GD/Effects_GD_Clouds").InstantiateInactive().Rename("Prefab_Effects_Clouds").DontDestroyOnLoad();
            if (_crawliesEmitterPrefab == null) _crawliesEmitterPrefab = SearchUtilities.Find("DB_EscapePodDimension_Body/Sector_EscapePodDimension/Effects_EscapePodDimension/Effects_DB_Crawlies (1)").InstantiateInactive().Rename("Prefab_Effects_Crawlies").DontDestroyOnLoad();
            if (_firefliesEmitterPrefab == null) _firefliesEmitterPrefab = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Effects_TH/Effects_TH_Fireflies").InstantiateInactive().Rename("Prefab_Effects_Fireflies").DontDestroyOnLoad();
            if (_pollenEmitterPrefab == null) _pollenEmitterPrefab = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Effects_TH/Effects_TH_SurfacePollen").InstantiateInactive().Rename("Prefab_Effects_Pollen").DontDestroyOnLoad();
            if (_iceMoteEmitterPrefab == null) _iceMoteEmitterPrefab = SearchUtilities.Find("DarkBramble_Body/Effects_DB/Effects_DB_IceMotes").InstantiateInactive().Rename("Prefab_Effects_IceMotes").DontDestroyOnLoad();
            if (_rockMoteEmitterPrefab == null) _rockMoteEmitterPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Effects_BH/Effects_BH_RockMotes").InstantiateInactive().Rename("Prefab_Effects_RockMotes").DontDestroyOnLoad();
            if (_crystalMoteEmitterPrefab == null) _crystalMoteEmitterPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Effects_BH/Effects_BH_CrystalMotes").InstantiateInactive().Rename("Prefab_Effects_CrystalMotes").DontDestroyOnLoad();
            if (_sandMoteEmitterPrefab == null) _sandMoteEmitterPrefab = SearchUtilities.Find("CaveTwin_Body/Sector_CaveTwin/Effects_CaveTwin/Effects_HGT_SandMotes").InstantiateInactive().Rename("Prefab_Effects_SandMotes").DontDestroyOnLoad();
            if (_fogEmitterPrefab == null) _fogEmitterPrefab = SearchUtilities.Find("DB_EscapePodDimension_Body/Sector_EscapePodDimension/Effects_EscapePodDimension/Effects_DB_Fog (1)").InstantiateInactive().Rename("Prefab_Effects_Fog").DontDestroyOnLoad();
        }


        #region obsolete
        // Never change method signatures, people directly reference the NH dll and it can break backwards compatibility
        // Dreamstalker needed this one
        [Obsolete]
        public static void Make(GameObject planetGO, Sector sector, PlanetConfig config, float surfaceHeight)
            => InternalMake(planetGO, sector, config, surfaceHeight);
        #endregion

        public static void Make(GameObject planetGO, Sector sector, PlanetConfig config)
            => InternalMake(planetGO, sector, config, null);

        /// <summary>
        /// Nullable surface height for backwards compat
        /// </summary>
        /// <param name="planetGO"></param>
        /// <param name="sector"></param>
        /// <param name="config"></param>
        /// <param name="surfaceHeight"></param>
        private static void InternalMake(GameObject planetGO, Sector sector, PlanetConfig config, float? surfaceHeight)
        {
            InitPrefabs();

            var effectsGO = new GameObject("Effects");
            effectsGO.SetActive(false);
            effectsGO.transform.parent = sector?.transform ?? planetGO.transform;
            effectsGO.transform.position = planetGO.transform.position;

            var sectorCullGroup = effectsGO.AddComponent<SectorCullGroup>();
            sectorCullGroup._sector = sector;
            sectorCullGroup._particleSystemSuspendMode = CullGroup.ParticleSystemSuspendMode.Stop;
            sectorCullGroup._occlusionCulling = false;
            sectorCullGroup._dynamicCullingBounds = false;
            sectorCullGroup._waitForStreaming = false;

            var (minHeight, maxHeight) = GetDefaultHeightRange(config);
            // min height override for backwards compat
            minHeight = surfaceHeight ?? minHeight;

            if (config.ParticleFields != null)
            {
                foreach (var particleField in config.ParticleFields)
                {
                    var prefab = GetPrefabByType(particleField.type);
                    var emitter = GameObject.Instantiate(prefab, effectsGO.transform);
                    emitter.name = !string.IsNullOrWhiteSpace(particleField.rename) ? particleField.rename : prefab.name.Replace("Prefab_", "");
                    emitter.transform.position = planetGO.transform.position;

                    var vfe = emitter.GetComponent<VectionFieldEmitter>();
                    var pvc = emitter.GetComponent<PlanetaryVectionController>();
                    pvc._vectionFieldEmitter = vfe;
                    pvc._densityByHeight = particleField.densityByHeightCurve != null ? particleField.densityByHeightCurve.ToAnimationCurve() : new AnimationCurve(new Keyframe[]
                    {
                        new Keyframe(minHeight - 0.5f, 0),
                        new Keyframe(minHeight, 10f),
                        new Keyframe(maxHeight, 0f)
                    });
                    pvc._followTarget = particleField.followTarget == ParticleFieldModule.FollowTarget.Probe ? PlanetaryVectionController.FollowTarget.Probe : PlanetaryVectionController.FollowTarget.Player;
                    pvc._activeInSector = sector;
                    pvc._exclusionSectors = new Sector[] { };

                    emitter.SetActive(true);
                }
            }

            effectsGO.transform.position = planetGO.transform.position;
            effectsGO.SetActive(true);
        }

        public static GameObject GetPrefabByType(ParticleFieldModule.ParticleFieldType type)
        {
            return type switch
            {
                ParticleFieldModule.ParticleFieldType.Rain => _rainEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.SnowflakesHeavy => _snowEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.SnowflakesLight => _snowLightEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.Embers => _emberEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.Clouds => _cloudEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.Leaves => _leafEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.Bubbles => _bubbleEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.Fog => _fogEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.CrystalMotes => _crystalMoteEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.RockMotes => _rockMoteEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.IceMotes => _iceMoteEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.SandMotes => _sandMoteEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.Crawlies => _crawliesEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.Fireflies => _firefliesEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.Plankton => _planktonEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.Pollen => _pollenEmitterPrefab,
                ParticleFieldModule.ParticleFieldType.Current => _currentEmitterPrefab,
                _ => null,
            };
        }

        private static (float, float) GetDefaultHeightRange(PlanetConfig config)
        {
            var minHeight = 0f;

            if (config.HeightMap?.minHeight != null)
            {
                if (config.Water?.size >= config.HeightMap.minHeight) minHeight = config.Water.size; // use sea level if its higher
                else minHeight = config.HeightMap.minHeight;
            }
            else if (config.Water?.size != null)
            {
                minHeight = config.Water.size;
            }
            else if (config.Lava?.size != null)
            {
                minHeight = config.Lava.size;
            }
            else if (config.Base != null)
            {
                minHeight = config.Base.surfaceSize;
            }

            var maxHeight = 100f;

            if (config.Atmosphere != null)
            {
                if (config.Atmosphere.clouds?.outerCloudRadius != null)
                {
                    maxHeight = config.Atmosphere.clouds.outerCloudRadius;
                }
                else
                {
                    maxHeight = config.Atmosphere.size;
                }
            }
            else if (minHeight != 0f)
            {
                maxHeight = minHeight * 2f;
            }

            return (minHeight, maxHeight);
        }
    }
}
