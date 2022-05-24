using NewHorizons.Utility;
using UnityEngine;
using NewHorizons.External.Modules.VariableSize;

namespace NewHorizons.Builder.Body
{
    public static class SandBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, OWRigidbody rb, SandModule module)
        {
            var sandGO = new GameObject("Sand");
            sandGO.SetActive(false);

            var sandSphere = GameObject.Instantiate(GameObject.Find("TowerTwin_Body/SandSphere_Draining/SandSphere"), sandGO.transform);
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

            var collider = GameObject.Instantiate(GameObject.Find("TowerTwin_Body/SandSphere_Draining/Collider"), sandGO.transform);
            var sphereCollider = collider.GetComponent<SphereCollider>();
            collider.SetActive(true);

            var occlusionSphere = GameObject.Instantiate(GameObject.Find("TowerTwin_Body/SandSphere_Draining/OcclusionSphere"), sandGO.transform);

            var proxyShadowCasterGO = GameObject.Instantiate(GameObject.Find("TowerTwin_Body/SandSphere_Draining/ProxyShadowCaster"), sandGO.transform);
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
