using NewHorizons.External.Configs;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class VolumesBuilder
    {
        public static void Make(GameObject planetGO, PlanetConfig config, float sphereOfInfluence)
        {
            var innerRadius = config.Base.SurfaceSize;
            var useMiniMap = config.Base.IsSatellite;

            GameObject volumesGO = new GameObject("Volumes");
            volumesGO.SetActive(false);
            volumesGO.transform.parent = planetGO.transform;

            GameObject rulesetGO = new GameObject("Ruleset");
            rulesetGO.SetActive(false);
            rulesetGO.transform.parent = volumesGO.transform;

            SphereShape SS = rulesetGO.AddComponent<SphereShape>();
            SS.SetCollisionMode(Shape.CollisionMode.Volume);
            SS.SetLayer(Shape.Layer.Sector);
            SS.layerMask = -1;
            SS.pointChecksOnly = true;
            SS.radius = sphereOfInfluence;

            rulesetGO.AddComponent<OWTriggerVolume>();

            PlanetoidRuleset PR = rulesetGO.AddComponent<PlanetoidRuleset>();
            PR._altitudeFloor = innerRadius;
            PR._altitudeCeiling = sphereOfInfluence;
            PR._useMinimap = useMiniMap;
            PR._useAltimeter = useMiniMap;

            EffectRuleset ER = rulesetGO.AddComponent<EffectRuleset>();
            ER._type = EffectRuleset.BubbleType.Underwater;
            var gdRuleset = GameObject.Find("GiantsDeep_Body/Sector_GD/Volumes_GD/RulesetVolumes_GD").GetComponent<EffectRuleset>();

            ER._material = gdRuleset._material;

            var cloudMaterial = new Material(gdRuleset._cloudMaterial);
            if (config.Atmosphere?.CloudTint != null)
            {
                cloudMaterial.SetColor("_FogColor", config.Atmosphere.CloudTint.ToColor32());
            }
            ER._cloudMaterial = cloudMaterial;

            volumesGO.transform.position = planetGO.transform.position;
            rulesetGO.SetActive(true);
            volumesGO.SetActive(true);
        }
    }
}
