using NewHorizons.External;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Atmosphere
{
    static class VolumesBuilder
    {
        public static void Make(GameObject body, float innerRadius, float outerRadius)
        {
            GameObject volumesGO = new GameObject("Volumes");
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
            SS.radius = outerRadius;

            /*OWTriggerVolume trigvol = */
            rulesetGO.AddComponent<OWTriggerVolume>();

            PlanetoidRuleset PR = rulesetGO.AddComponent<PlanetoidRuleset>();
            PR.SetValue("_altitudeFloor", innerRadius);
            PR.SetValue("_altitudeCeiling", outerRadius);

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
