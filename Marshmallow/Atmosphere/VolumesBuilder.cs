using Marshmallow.External;
using OWML.ModHelper.Events;
using UnityEngine;
using Logger = Marshmallow.Utility.Logger;

namespace Marshmallow.Atmosphere
{
    static class VolumesBuilder
    {
        public static void Make(GameObject body, IPlanetConfig config)
        {
            GameObject volumesGO = new GameObject();
            volumesGO.SetActive(false);
            volumesGO.transform.parent = body.transform;

            GameObject rulesetGO = new GameObject();
            rulesetGO.SetActive(false);
            rulesetGO.transform.parent = volumesGO.transform;

            SphereShape SS = rulesetGO.AddComponent<SphereShape>();
            SS.SetCollisionMode(Shape.CollisionMode.Volume);
            SS.SetLayer(Shape.Layer.Sector);
            SS.layerMask = -1;
            SS.pointChecksOnly = true;
            SS.radius = config.TopCloudSize;

            /*OWTriggerVolume trigvol = */rulesetGO.AddComponent<OWTriggerVolume>();

            PlanetoidRuleset PR = rulesetGO.AddComponent<PlanetoidRuleset>();
            PR.SetValue("_altitudeFloor", config.GroundSize);
            PR.SetValue("_altitudeCeiling", config.TopCloudSize);

            EffectRuleset ER = rulesetGO.AddComponent<EffectRuleset>();
            ER.SetValue("_type", EffectRuleset.BubbleType.Underwater);
            ER.SetValue("_material", GameObject.Find("RulesetVolumes_GD").GetComponent<RulesetVolume>().GetValue<Material>("_material"));
            ER.SetValue("_cloudMaterial", GameObject.Find("RulesetVolumes_GD").GetComponent<RulesetVolume>().GetValue<Material>("_cloudMaterial"));

            rulesetGO.SetActive(true);
            volumesGO.SetActive(true);
            Logger.Log("Finished building volumes", Logger.LogType.Log);
        }
    }
}
