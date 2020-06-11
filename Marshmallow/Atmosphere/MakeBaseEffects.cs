using OWML.ModHelper.Events;
using System.Reflection;
using UnityEngine;

namespace Marshmallow.Atmosphere
{
    static class MakeBaseEffects
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

            GameObject rainGO = new GameObject();
            rainGO.SetActive(false);
            rainGO.transform.parent = effectsGO.transform;

            /*ParticleSystem ps = */GameObject.Instantiate(GameObject.Find("Effects_GD_Rain").GetComponent<ParticleSystem>());

            VectionFieldEmitter VFE = rainGO.AddComponent<VectionFieldEmitter>();
            VFE.fieldRadius = 20f;
            VFE.particleCount = 10;
            VFE.emitOnLeadingEdge = false;
            VFE.emitDirection = VectionFieldEmitter.EmitDirection.Radial;
            VFE.reverseDir = true;
            VFE.SetValue("_affectingForces", new ForceVolume[0]);
            VFE.SetValue("_applyForcePerParticle", false);

            PlanetaryVectionController PVC = rainGO.AddComponent<PlanetaryVectionController>();
            PVC.SetValue("_followTarget", PVC.GetType().GetNestedType("FollowTarget", BindingFlags.NonPublic).GetField("Player").GetValue(PVC));
            PVC.SetValue("_activeInSector", sector);

            rainGO.GetComponent<Renderer>().material = GameObject.Find("Effects_GD_Rain").GetComponent<Renderer>().material;

            effectsGO.SetActive(true);
            rainGO.SetActive(true);
        }
    }
}
