using NewHorizons.External.Configs;
using UnityEngine;

namespace NewHorizons.Builder.Atmosphere
{
    public static class VolumesBuilder
    {
        private static readonly int FogColor = Shader.PropertyToID("_FogColor");

        public static void Make(GameObject planetGO, PlanetConfig config, float sphereOfInfluence)
        {
            var innerRadius = config.Base.surfaceSize;

            var volumesGO = new GameObject("Volumes");
            volumesGO.SetActive(false);
            volumesGO.transform.parent = planetGO.transform;

            var rulesetGO = new GameObject("Ruleset");
            rulesetGO.SetActive(false);
            rulesetGO.transform.parent = volumesGO.transform;

            var SS = rulesetGO.AddComponent<SphereShape>();
            SS.SetCollisionMode(Shape.CollisionMode.Volume);
            SS.SetLayer(Shape.Layer.Sector);
            SS.layerMask = -1;
            SS.pointChecksOnly = true;
            SS.radius = sphereOfInfluence;

            rulesetGO.AddComponent<OWTriggerVolume>();

            var PR = rulesetGO.AddComponent<PlanetoidRuleset>();
            PR._altitudeFloor = innerRadius;
            PR._altitudeCeiling = sphereOfInfluence;
            PR._useMinimap = config.Base.showMinimap;
            PR._useAltimeter = config.Base.showMinimap;

            rulesetGO.AddComponent<AntiTravelMusicRuleset>();

            var ER = rulesetGO.AddComponent<EffectRuleset>();
            ER._type = EffectRuleset.BubbleType.Underwater;
            var gdRuleset = GameObject.Find("GiantsDeep_Body/Sector_GD/Volumes_GD/RulesetVolumes_GD")
                .GetComponent<EffectRuleset>();

            ER._material = gdRuleset._material;

            var cloudMaterial = new Material(gdRuleset._cloudMaterial);
            if (config.Atmosphere?.clouds?.tint != null)
                cloudMaterial.SetColor(FogColor, config.Atmosphere.clouds.tint);
            ER._cloudMaterial = cloudMaterial;

            volumesGO.transform.position = planetGO.transform.position;
            rulesetGO.SetActive(true);
            volumesGO.SetActive(true);
        }
    }
}