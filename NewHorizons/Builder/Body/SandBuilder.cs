using NewHorizons.External.VariableSize;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    static class SandBuilder
    {
        public static void Make(GameObject go, Sector sector, OWRigidbody rb, SandModule module)
        {
            var sandGO = new GameObject("Sand");
            sandGO.SetActive(false);

            var sandSphere = GameObject.Instantiate(GameObject.Find("TowerTwin_Body/SandSphere_Draining/SandSphere"), sandGO.transform);
            if(module.Tint != null)
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
                sandMR.sharedMaterials[0].color = module.Tint.ToColor();
                sandMR.sharedMaterials[1].color = module.Tint.ToColor();
            }

            var collider = GameObject.Instantiate(GameObject.Find("TowerTwin_Body/SandSphere_Draining/Collider"), sandGO.transform);
            var sphereCollider = collider.GetComponent<SphereCollider>();
            collider.SetActive(true);

            var occlusionSphere = GameObject.Instantiate(GameObject.Find("TowerTwin_Body/SandSphere_Draining/OcclusionSphere"), sandGO.transform);
            
            var proxyShadowCasterGO = GameObject.Instantiate(GameObject.Find("TowerTwin_Body/SandSphere_Draining/ProxyShadowCaster"), sandGO.transform);
            var proxyShadowCaster = proxyShadowCasterGO.GetComponent<ProxyShadowCaster>();
            proxyShadowCaster.SetSuperGroup(sandGO.GetComponent<ProxyShadowCasterSuperGroup>());

            sandSphere.AddComponent<ChildColliderSettings>();

            if(module.Curve != null)
            {
                var levelController = sandGO.AddComponent<SandLevelController>();
                var curve = new AnimationCurve();
                foreach(var pair in module.Curve)
                {
                    curve.AddKey(new Keyframe(pair.Time, 2f * module.Size * pair.Value));
                }
                levelController._scaleCurve = curve;
            }

            sandGO.transform.parent = go.transform;
            sandGO.transform.localPosition = Vector3.zero;
            sandGO.transform.localScale = Vector3.one * module.Size;

            sandGO.SetActive(true);
        }
    }
}
