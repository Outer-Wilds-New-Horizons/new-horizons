using NewHorizons.External;
using NewHorizons.External.Configs;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Atmosphere
{
    static class VolumesBuilder
    {
        public static void Make(GameObject body, float innerRadius, float outerRadius, IPlanetConfig config)
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

            rulesetGO.AddComponent<OWTriggerVolume>();

            PlanetoidRuleset PR = rulesetGO.AddComponent<PlanetoidRuleset>();
            PR._altitudeFloor = innerRadius;
            PR._altitudeCeiling = outerRadius;
            PR._useMinimap = !config.Base.IsSatellite;
            PR._useAltimeter = !config.Base.IsSatellite;

            EffectRuleset ER = rulesetGO.AddComponent<EffectRuleset>();
            ER._type = EffectRuleset.BubbleType.Underwater;
            ER._material = GameObject.Find("RulesetVolumes_GD").GetComponent<RulesetVolume>().GetValue<Material>("_material");
            ER._cloudMaterial = GameObject.Find("RulesetVolumes_GD").GetComponent<RulesetVolume>().GetValue<Material>("_cloudMaterial");

            volumesGO.transform.localPosition = Vector3.zero;
            rulesetGO.SetActive(true);
            volumesGO.SetActive(true);
        }
    }
}
