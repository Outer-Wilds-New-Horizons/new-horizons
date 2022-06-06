using System.Collections.Generic;
using UnityEngine;
using NewHorizons.Utility;
using NewHorizons.External.Modules.VariableSize;

namespace NewHorizons.Builder.Body
{
    public static class LavaBuilder
    {
        private static readonly int HeightScale = Shader.PropertyToID("_HeightScale");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        public static void Make(GameObject planetGO, Sector sector, OWRigidbody rb, LavaModule module)
        {
            var heightScale = module.size;
            if (module.curve != null)
            {
                var modifier = 1f;
                foreach (var pair in module.curve)
                {
                    if (pair.value < modifier) modifier = pair.value;
                }
                heightScale = Mathf.Max(0.1f, heightScale * modifier);
            }

            var moltenCore = new GameObject("MoltenCore");
            moltenCore.SetActive(false);
            moltenCore.transform.parent = sector?.transform ?? planetGO.transform;
            moltenCore.transform.position = planetGO.transform.position;
            moltenCore.transform.localScale = Vector3.one * module.size;

            var lavaSphere = GameObject.Instantiate(SearchUtilities.Find("VolcanicMoon_Body/MoltenCore_VM/LavaSphere"), moltenCore.transform);
            lavaSphere.transform.localScale = Vector3.one;
            lavaSphere.transform.name = "LavaSphere";
            lavaSphere.GetComponent<MeshRenderer>().material.SetFloat(HeightScale, heightScale);
            if (module.tint != null) lavaSphere.GetComponent<MeshRenderer>().material.SetColor(EmissionColor, module.tint.ToColor());

            var sectorCullGroup = lavaSphere.GetComponent<SectorCullGroup>();
            sectorCullGroup.SetSector(sector);

            var moltenCoreProxy = GameObject.Instantiate(SearchUtilities.Find("VolcanicMoon_Body/MoltenCore_VM/MoltenCore_Proxy"), moltenCore.transform); ;
            moltenCoreProxy.name = "MoltenCore_Proxy";

            var proxyLavaSphere = moltenCoreProxy.transform.Find("LavaSphere (1)");
            proxyLavaSphere.transform.localScale = Vector3.one;
            proxyLavaSphere.name = "LavaSphere_Proxy";
            proxyLavaSphere.GetComponent<MeshRenderer>().material.SetFloat(HeightScale, heightScale);
            if (module.tint != null) proxyLavaSphere.GetComponent<MeshRenderer>().material.SetColor(EmissionColor, module.tint.ToColor());

            var sectorProxy = moltenCoreProxy.GetComponent<SectorProxy>();
            sectorProxy._renderers = new List<Renderer> { proxyLavaSphere.GetComponent<MeshRenderer>() };
            sectorProxy.SetSector(sector);

            var destructionVolume = GameObject.Instantiate(SearchUtilities.Find("VolcanicMoon_Body/MoltenCore_VM/DestructionVolume"), moltenCore.transform);
            destructionVolume.GetComponent<SphereCollider>().radius = 1;
            destructionVolume.SetActive(true);

            if (module.curve != null)
            {
                var levelController = moltenCore.AddComponent<SandLevelController>();
                var curve = new AnimationCurve();
                foreach (var pair in module.curve)
                {
                    curve.AddKey(new Keyframe(pair.time, module.size * pair.value));
                }
                levelController._scaleCurve = curve;
            }

            moltenCore.SetActive(true);
        }
    }
}
