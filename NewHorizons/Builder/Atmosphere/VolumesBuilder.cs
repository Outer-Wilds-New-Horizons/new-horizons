using NewHorizons.External.Configs;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class VolumesBuilder
    {
        private static readonly int FogColor = Shader.PropertyToID("_FogColor");

        private static Material _gdMaterial, _gdCloudMaterial;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_gdMaterial == null) _gdMaterial = new Material(SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Volumes_GD/RulesetVolumes_GD").GetComponent<EffectRuleset>()._material).DontDestroyOnLoad();
            if (_gdCloudMaterial == null) _gdCloudMaterial = new Material(SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Volumes_GD/RulesetVolumes_GD").GetComponent<EffectRuleset>()._cloudMaterial).DontDestroyOnLoad();
        }

        public static void Make(GameObject planetGO, OWRigidbody owrb, PlanetConfig config, float sphereOfInfluence)
        {
            InitPrefabs();

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
            PR._shuttleLandingRadius = sphereOfInfluence;
            PR._useMinimap = config.Base.showMinimap;
            PR._useAltimeter = config.Base.showMinimap;

            rulesetGO.AddComponent<AntiTravelMusicRuleset>();

            var ER = rulesetGO.AddComponent<EffectRuleset>();
            ER._type = EffectRuleset.BubbleType.Underwater;

            ER._material = _gdMaterial;

            if (config.Atmosphere?.clouds != null)
            {
                var cloudMaterial = new Material(_gdCloudMaterial);

                if (config.Atmosphere?.clouds?.tint != null)
                {
                    cloudMaterial.SetColor(FogColor, config.Atmosphere.clouds.tint.ToColor());
                }
                // For all prefabs but GD we want grey fog between clouds
                // I can't find an EffectsRuleset on the QM so I don't know how it works
                else if (config.Atmosphere.clouds.cloudsPrefab != External.Modules.CloudPrefabType.GiantsDeep)
                {
                    cloudMaterial.SetColor(FogColor, new Color(43f/255f, 51f/255f, 57f/255f));
                }

                ER._cloudMaterial = cloudMaterial;
            }

            volumesGO.transform.position = planetGO.transform.position;
            rulesetGO.SetActive(true);
            volumesGO.SetActive(true);
        }
    }
}
