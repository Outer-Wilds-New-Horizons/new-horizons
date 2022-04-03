using NewHorizons.Utility;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Atmosphere
{
    static class EffectsBuilder
    {
        public static void Make(GameObject body, Sector sector, float surfaceSize, float atmoSize, bool hasRain, bool hasSnow)
        {
            GameObject effectsGO = new GameObject("Effects");
            effectsGO.SetActive(false);
            effectsGO.transform.parent = body.transform;
            effectsGO.transform.localPosition = Vector3.zero;

            SectorCullGroup SCG = effectsGO.AddComponent<SectorCullGroup>();
            SCG.SetValue("_sector", sector);
            SCG.SetValue("_particleSystemSuspendMode", CullGroup.ParticleSystemSuspendMode.Stop);
            SCG.SetValue("_occlusionCulling", false);
            SCG.SetValue("_dynamicCullingBounds", false);
            SCG.SetValue("_waitForStreaming", false);

            if(hasRain)
            {
                var rainGO = GameObject.Instantiate(SearchUtilities.CachedFind("/GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/Effects_GD_Rain"), effectsGO.transform);
                rainGO.transform.localPosition = Vector3.zero;

                var pvc = rainGO.GetComponent<PlanetaryVectionController>();
                pvc._densityByHeight = new AnimationCurve(new Keyframe[] 
                { 
                    new Keyframe(surfaceSize - 0.5f, 0),
                    new Keyframe(surfaceSize, 10f), 
                    new Keyframe(atmoSize, 0f) 
                });

                rainGO.GetComponent<PlanetaryVectionController>().SetValue("_activeInSector", sector);
                rainGO.GetComponent<PlanetaryVectionController>().SetValue("_exclusionSectors", new Sector[] { });
                rainGO.SetActive(true);
            }
            
            if(hasSnow)
            {
                var snowGO = new GameObject("SnowEffects");
                snowGO.transform.parent = effectsGO.transform;
                snowGO.transform.localPosition = Vector3.zero;
                for(int i = 0; i < 5; i++)
                {
                    var snowEmitter = GameObject.Instantiate(SearchUtilities.CachedFind("/BrittleHollow_Body/Sector_BH/Effects_BH/Effects_BH_Snowflakes"), snowGO.transform);
                    snowEmitter.name = "SnowEmitter";
                    snowEmitter.transform.localPosition = Vector3.zero;

                    var pvc = snowEmitter.GetComponent<PlanetaryVectionController>();
                    pvc._densityByHeight = new AnimationCurve(new Keyframe[]
                    {
                        new Keyframe(surfaceSize - 0.5f, 0),
                        new Keyframe(surfaceSize, 10f),
                        new Keyframe(atmoSize, 0f)
                    });

                    snowEmitter.GetComponent<PlanetaryVectionController>().SetValue("_activeInSector", sector);
                    snowEmitter.GetComponent<PlanetaryVectionController>().SetValue("_exclusionSectors", new Sector[] { });
                    snowEmitter.SetActive(true);
                }
            }

            effectsGO.transform.localPosition = Vector3.zero;
            effectsGO.SetActive(true);
        }
    }
}
