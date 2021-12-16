using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Atmosphere
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
                var rainGO = GameObject.Instantiate(GameObject.Find("/GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/Effects_GD_Rain"), effectsGO.transform);
                rainGO.transform.localPosition = Vector3.zero;

                var pvc = rainGO.GetComponent<PlanetaryVectionController>();
                pvc.SetValue("_densityByHeight", new AnimationCurve(new Keyframe[] { new Keyframe(surfaceSize, 10f), new Keyframe(atmoSize, 0f) }));

                rainGO.GetComponent<PlanetaryVectionController>().SetValue("_activeInSector", sector);
                rainGO.GetComponent<PlanetaryVectionController>().SetValue("_exclusionSectors", new Sector[] { });
                rainGO.SetActive(true);
            }
            
            if(hasSnow)
            {
                var snowGO = GameObject.Instantiate(GameObject.Find("/BrittleHollow_Body/Sector_BH/Effects_BH/Effects_BH_Snowflakes"), effectsGO.transform);
                snowGO.transform.localPosition = Vector3.zero;

                var pvc = snowGO.GetComponent<PlanetaryVectionController>();
                pvc.SetValue("_densityByHeight", new AnimationCurve(new Keyframe[] { new Keyframe(surfaceSize, 10f), new Keyframe(atmoSize, 0f) }));

                var particleSystem = snowGO.GetComponent<ParticleSystem>();
                var e = particleSystem.emission;
                e.rateOverTime = 50;

                snowGO.GetComponent<PlanetaryVectionController>().SetValue("_activeInSector", sector);
                snowGO.GetComponent<PlanetaryVectionController>().SetValue("_exclusionSectors", new Sector[] { });
                snowGO.SetActive(true);
            }

            effectsGO.SetActive(true);

            Logger.Log("Finished building effects", Logger.LogType.Log);
        }
    }
}
