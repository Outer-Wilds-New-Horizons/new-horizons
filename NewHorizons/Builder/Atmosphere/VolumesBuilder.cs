using NewHorizons.External.Configs;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class VolumesBuilder
    {
        private static readonly int FogColor = Shader.PropertyToID("_FogColor");

        public static void Make(GameObject planetGO, OWRigidbody owrb, PlanetConfig config, float sphereOfInfluence)
        {
            var innerRadius = config.Base.surfaceSize;

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
            PR._useMinimap = config.Base.showMinimap;
            PR._useAltimeter = config.Base.showMinimap;

            rulesetGO.AddComponent<AntiTravelMusicRuleset>();

            var gdRuleset = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Volumes_GD/RulesetVolumes_GD")?.GetComponent<EffectRuleset>();
            if (gdRuleset != null)
            {
                EffectRuleset ER = rulesetGO.AddComponent<EffectRuleset>();
                ER._type = EffectRuleset.BubbleType.Underwater;

                ER._material = gdRuleset._material;

                var cloudMaterial = new Material(gdRuleset._cloudMaterial);
                if (config.Atmosphere?.clouds?.tint != null)
                {
                    cloudMaterial.SetColor(FogColor, config.Atmosphere.clouds.tint.ToColor());
                }
                ER._cloudMaterial = cloudMaterial;
            }

            if (config.Base.zeroGravityRadius != 0)
            {
                var zeroGObject = new GameObject("ZeroGVolume");
                zeroGObject.transform.parent = volumesGO.transform;
                zeroGObject.transform.localPosition = Vector3.zero;
                zeroGObject.transform.localScale = Vector3.one * config.Base.zeroGravityRadius;
                zeroGObject.layer = LayerMask.NameToLayer("BasicEffectVolume");

                var sphereCollider = zeroGObject.AddComponent<SphereCollider>();
                sphereCollider.radius = 1;
                sphereCollider.isTrigger = true;

                var owCollider = zeroGObject.AddComponent<OWCollider>();
                owCollider._parentBody = owrb;
                owCollider._collider = sphereCollider;

                var triggerVolume = zeroGObject.AddComponent<OWTriggerVolume>();
                triggerVolume._owCollider = owCollider;

                var zeroGVolume = zeroGObject.AddComponent<ZeroGVolume>();
                zeroGVolume._attachedBody = owrb;
                zeroGVolume._triggerVolume = triggerVolume;
                zeroGVolume._inheritable = true;
                zeroGVolume._priority = 1;
            }

            volumesGO.transform.position = planetGO.transform.position;
            rulesetGO.SetActive(true);
            volumesGO.SetActive(true);
        }
    }
}
