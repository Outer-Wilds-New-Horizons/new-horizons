using NewHorizons.Utility;
using UnityEngine;
using NewHorizons.External.Modules.VariableSize;

namespace NewHorizons.Builder.Body
{
    public static class SandBuilder
    {
        private static GameObject _sandSphere;
        private static GameObject _sandCollider;
        private static GameObject _sandOcclusion;
        private static GameObject _sandProxyShadowCaster;
        private static Material _hgtSandEffectMaterial;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_sandSphere == null) _sandSphere = SearchUtilities.Find("TowerTwin_Body/SandSphere_Draining/SandSphere").InstantiateInactive().Rename("Prefab_TT_SandSphere").DontDestroyOnLoad();
            if (_sandCollider == null) _sandCollider = SearchUtilities.Find("TowerTwin_Body/SandSphere_Draining/Collider").InstantiateInactive().Rename("Prefab_TT_SandCollider").DontDestroyOnLoad();
            if (_sandOcclusion == null) _sandOcclusion = SearchUtilities.Find("TowerTwin_Body/SandSphere_Draining/OcclusionSphere").InstantiateInactive().Rename("Prefab_TT_SandOcclusion").DontDestroyOnLoad();
            if (_sandProxyShadowCaster == null) _sandProxyShadowCaster = SearchUtilities.Find("TowerTwin_Body/SandSphere_Draining/ProxyShadowCaster").InstantiateInactive().Rename("Prefab_TT_SandProxyShadowCaster").DontDestroyOnLoad();
            if (_hgtSandEffectMaterial == null) _hgtSandEffectMaterial = new Material(SearchUtilities.Find("FocalBody/Sector_HGT").GetComponent<EffectRuleset>()._sandMaterial).DontDestroyOnLoad();
        }

        public static void Make(GameObject planetGO, Sector sector, OWRigidbody rb, SandModule module)
        {
            InitPrefabs();

            var sandGO = new GameObject("Sand");
            sandGO.SetActive(false);

            var sandSphere = Object.Instantiate(_sandSphere, sandGO.transform);
            sandSphere.name = "Sphere";
            sandSphere.SetActive(true);

            sandSphere.AddComponent<SphereShape>().radius = 1;
            sandSphere.AddComponent<OWTriggerVolume>();
            var sandMaterial = new Material(_hgtSandEffectMaterial);
            var sER = sandSphere.AddComponent<EffectRuleset>();
            sER._type = EffectRuleset.BubbleType.None;
            sER._sandMaterial = sandMaterial;

            if (module.tint != null)
            {
                var oldMR = sandSphere.GetComponent<TessellatedSphereRenderer>();
                var sandMaterials = oldMR.sharedMaterials;
                var sandMR = sandSphere.AddComponent<TessellatedSphereRenderer>();
                sandMR.CopyPropertiesFrom(oldMR);
                sandMR.sharedMaterials = new Material[]
                {
                    new Material(sandMaterials[0]),
                    new Material(sandMaterials[1])
                };
                Object.Destroy(oldMR);
                sandMR.sharedMaterials[0].color = module.tint.ToColor();
                sandMR.sharedMaterials[1].color = module.tint.ToColor();

                var baseColor = module.tint.ToColor();
                var effectColor = new Color(baseColor.r * 0.184f, baseColor.g * 0.184f, baseColor.b * 0.184f, baseColor.a); // base game does .184 darker
                sandMaterial.color = effectColor;
            }

            var collider = Object.Instantiate(_sandCollider, sandGO.transform);
            collider.name = "Collider";
            collider.SetActive(true);

            var occlusionSphere = Object.Instantiate(_sandOcclusion, sandGO.transform);
            occlusionSphere.name = "Occlusion";
            occlusionSphere.SetActive(true);

            var proxyShadowCasterGO = Object.Instantiate(_sandProxyShadowCaster, sandGO.transform);
            proxyShadowCasterGO.name = "ProxyShadowCaster";
            var proxyShadowCaster = proxyShadowCasterGO.GetComponent<ProxyShadowCaster>();
            proxyShadowCaster.SetSuperGroup(sandGO.GetComponent<ProxyShadowCasterSuperGroup>());
            proxyShadowCasterGO.SetActive(true);

            sandSphere.AddComponent<ChildColliderSettings>();

            if (module.curve != null)
            {
                var levelController = sandGO.AddComponent<SandLevelController>();
                var curve = new AnimationCurve();
                foreach (var pair in module.curve)
                {
                    curve.AddKey(new Keyframe(pair.time, 2f * module.size * pair.value));
                }
                levelController._scaleCurve = curve;
            }

            sandGO.transform.parent = sector?.transform ?? planetGO.transform;
            sandGO.transform.position = planetGO.transform.position;
            sandGO.transform.localScale = Vector3.one * module.size * 2f;

            sandGO.SetActive(true);
        }
    }
}
