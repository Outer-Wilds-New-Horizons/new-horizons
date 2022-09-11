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

        internal static void InitPrefabs()
        {
            if (_sandSphere == null) _sandSphere = SearchUtilities.Find("TowerTwin_Body/SandSphere_Draining/SandSphere").InstantiateInactive().Rename("Prefab_TT_SandSphere").DontDestroyOnLoad();
            if (_sandCollider == null) _sandCollider = SearchUtilities.Find("TowerTwin_Body/SandSphere_Draining/Collider").InstantiateInactive().Rename("Prefab_TT_SandCollider").DontDestroyOnLoad();
            if (_sandOcclusion == null) _sandOcclusion = SearchUtilities.Find("TowerTwin_Body/SandSphere_Draining/OcclusionSphere").InstantiateInactive().Rename("Prefab_TT_SandOcclusion").DontDestroyOnLoad();
            if (_sandProxyShadowCaster == null) _sandProxyShadowCaster = SearchUtilities.Find("TowerTwin_Body/SandSphere_Draining/ProxyShadowCaster").InstantiateInactive().Rename("Prefab_TT_SandProxyShadowCaster").DontDestroyOnLoad();
        }

        public static void Make(GameObject planetGO, Sector sector, OWRigidbody rb, SandModule module)
        {
            InitPrefabs();

            var sandGO = new GameObject("Sand");
            sandGO.SetActive(false);

            var sandSphere = GameObject.Instantiate(_sandSphere, sandGO.transform);
            sandSphere.name = "Sphere";
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
                GameObject.Destroy(oldMR);
                sandMR.sharedMaterials[0].color = module.tint.ToColor();
                sandMR.sharedMaterials[1].color = module.tint.ToColor();
            }

            var collider = GameObject.Instantiate(_sandCollider, sandGO.transform);
            collider.name = "Collider";
            collider.SetActive(true);

            var occlusionSphere = GameObject.Instantiate(_sandOcclusion, sandGO.transform);
            occlusionSphere.name = "Occlusion";

            var proxyShadowCasterGO = GameObject.Instantiate(_sandProxyShadowCaster, sandGO.transform);
            proxyShadowCasterGO.name = "ProxyShadowCaster";
            var proxyShadowCaster = proxyShadowCasterGO.GetComponent<ProxyShadowCaster>();
            proxyShadowCaster.SetSuperGroup(sandGO.GetComponent<ProxyShadowCasterSuperGroup>());

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
