using NewHorizons.External.Modules;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class EffectsBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule.AirInfo info, float surfaceSize)
        {
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

            if (info.isRaining)
            {
                var rainGO = GameObject.Instantiate(SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/Effects_GD_Rain"), effectsGO.transform);
                rainGO.transform.position = planetGO.transform.position;

                var pvc = rainGO.GetComponent<PlanetaryVectionController>();
                pvc._densityByHeight = new AnimationCurve(new Keyframe[]
                {
                    new Keyframe(surfaceSize - 0.5f, 0),
                    new Keyframe(surfaceSize, 10f),
                    new Keyframe(info.scale, 0f)
                });

                rainGO.GetComponent<PlanetaryVectionController>()._activeInSector = sector;
                rainGO.GetComponent<PlanetaryVectionController>()._exclusionSectors = new Sector[] { };
                rainGO.SetActive(true);
            }

            if (info.isSnowing)
            {
                var snowGO = new GameObject("SnowEffects");
                snowGO.transform.parent = effectsGO.transform;
                snowGO.transform.position = planetGO.transform.position;
                for (int i = 0; i < 5; i++)
                {
                    var snowEmitter = GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Effects_BH/Effects_BH_Snowflakes"), snowGO.transform);
                    snowEmitter.name = "SnowEmitter";
                    snowEmitter.transform.position = planetGO.transform.position;

                    var pvc = snowEmitter.GetComponent<PlanetaryVectionController>();
                    pvc._densityByHeight = new AnimationCurve(new Keyframe[]
                    {
                        new Keyframe(surfaceSize - 0.5f, 0),
                        new Keyframe(surfaceSize, 10f),
                        new Keyframe(info.scale, 0f)
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
