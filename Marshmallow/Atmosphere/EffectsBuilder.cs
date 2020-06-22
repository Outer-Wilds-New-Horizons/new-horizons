using OWML.ModHelper.Events;
using UnityEngine;
using Logger = Marshmallow.Utility.Logger;

namespace Marshmallow.Atmosphere
{
    static class EffectsBuilder
    {
        public static void Make(GameObject body, Sector sector)
        {
            GameObject effectsGO = new GameObject();
            effectsGO.SetActive(false);
            effectsGO.transform.parent = body.transform;

            SectorCullGroup SCG = effectsGO.AddComponent<SectorCullGroup>();
            SCG.SetValue("_sector", sector);
            SCG.SetValue("_particleSystemSuspendMode", CullGroup.ParticleSystemSuspendMode.Stop);
            SCG.SetValue("_occlusionCulling", false);
            SCG.SetValue("_dynamicCullingBounds", false);
            SCG.SetValue("_waitForStreaming", false);

            var rainGO = GameObject.Instantiate(GameObject.Find("Effects_GD_Rain"));
            rainGO.transform.parent = effectsGO.transform;
            rainGO.transform.localPosition = Vector3.zero;

            effectsGO.SetActive(true);
            rainGO.SetActive(true);
            Logger.Log("Finished building effects", Logger.LogType.Log);
        }
    }
}
