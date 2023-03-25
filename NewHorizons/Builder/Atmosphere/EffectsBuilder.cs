using NewHorizons.External.Configs;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class EffectsBuilder
    {
        private static GameObject _rainEmitterPrefab;
        private static GameObject _snowEmitterPrefab;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_rainEmitterPrefab == null) _rainEmitterPrefab = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/Effects_GD_Rain").InstantiateInactive().Rename("Prefab_Effects_Rain").DontDestroyOnLoad();
            if (_snowEmitterPrefab == null) _snowEmitterPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Effects_BH/Effects_BH_Snowflakes").InstantiateInactive().Rename("Prefab_Effects_Snowflakes").DontDestroyOnLoad();
        }

        public static void Make(GameObject planetGO, Sector sector, PlanetConfig config, float surfaceSize)
        {
            InitPrefabs();

            GameObject effectsGO = new GameObject("Effects");
            effectsGO.SetActive(false);
            effectsGO.transform.parent = sector?.transform ?? planetGO.transform;
            effectsGO.transform.position = planetGO.transform.position;

            SectorCullGroup SCG = effectsGO.AddComponent<SectorCullGroup>();
            SCG._sector = sector;
            SCG._particleSystemSuspendMode = CullGroup.ParticleSystemSuspendMode.Stop;
            SCG._occlusionCulling = false;
            SCG._dynamicCullingBounds = false;
            SCG._waitForStreaming = false;

            var minHeight = surfaceSize;
            if (config.HeightMap?.minHeight != null)
            {
                if (config.Water?.size >= config.HeightMap.minHeight) minHeight = config.Water.size; // use sea level if its higher
                else minHeight = config.HeightMap.minHeight;
            }
            else if (config.Water?.size != null) minHeight = config.Water.size;
            else if (config.Lava?.size != null) minHeight = config.Lava.size;

            var maxHeight = config.Atmosphere.size;
            if (config.Atmosphere.clouds?.outerCloudRadius != null) maxHeight = config.Atmosphere.clouds.outerCloudRadius;

            if (config.Atmosphere.hasRain)
            {
                var rainGO = GameObject.Instantiate(_rainEmitterPrefab, effectsGO.transform);
                rainGO.name = "RainEmitter";
                rainGO.transform.position = planetGO.transform.position;

                var pvc = rainGO.GetComponent<PlanetaryVectionController>();
                pvc._densityByHeight = new AnimationCurve(new Keyframe[]
                {
                    new Keyframe(minHeight - 0.5f, 0),
                    new Keyframe(minHeight, 10f),
                    new Keyframe(maxHeight, 0f)
                });

                rainGO.GetComponent<PlanetaryVectionController>()._activeInSector = sector;
                rainGO.GetComponent<PlanetaryVectionController>()._exclusionSectors = new Sector[] { };
                rainGO.SetActive(true);
            }

            if (config.Atmosphere.hasSnow)
            {
                var snowGO = new GameObject("SnowEffects");
                snowGO.transform.parent = effectsGO.transform;
                snowGO.transform.position = planetGO.transform.position;
                for (int i = 0; i < 5; i++)
                {
                    var snowEmitter = GameObject.Instantiate(_snowEmitterPrefab, snowGO.transform);
                    snowEmitter.name = "SnowEmitter";
                    snowEmitter.transform.position = planetGO.transform.position;

                    var pvc = snowEmitter.GetComponent<PlanetaryVectionController>();
                    pvc._densityByHeight = new AnimationCurve(new Keyframe[]
                    {
                        new Keyframe(minHeight - 0.5f, 0),
                        new Keyframe(minHeight, 10f),
                        new Keyframe(maxHeight, 0f)
                    });

                    snowEmitter.GetComponent<PlanetaryVectionController>()._activeInSector = sector;
                    snowEmitter.GetComponent<PlanetaryVectionController>()._exclusionSectors = new Sector[] { };
                    snowEmitter.SetActive(true);
                }
            }

            effectsGO.transform.position = planetGO.transform.position;
            effectsGO.SetActive(true);
        }
    }
}
