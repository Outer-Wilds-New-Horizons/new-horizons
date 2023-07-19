using System.Collections.Generic;
using UnityEngine;
using NewHorizons.Utility;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Components.SizeControllers;

namespace NewHorizons.Builder.Body
{
    public static class LavaBuilder
    {
        public static readonly int HeightScale = Shader.PropertyToID("_HeightScale");
        public static readonly int EdgeFade = Shader.PropertyToID("_EdgeFade");
        public static readonly int TexHeight = Shader.PropertyToID("_TexHeight");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private static GameObject _lavaSphere;
        private static GameObject _moltenCoreProxy;
        private static GameObject _destructionVolume;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_lavaSphere == null) _lavaSphere = SearchUtilities.Find("VolcanicMoon_Body/MoltenCore_VM/LavaSphere").InstantiateInactive().Rename("Prefab_VM_LavaSphere").DontDestroyOnLoad();
            if (_moltenCoreProxy == null) _moltenCoreProxy = SearchUtilities.Find("VolcanicMoon_Body/MoltenCore_VM/MoltenCore_Proxy").InstantiateInactive().Rename("Prefab_VM_MoltenCore_Proxy").DontDestroyOnLoad();
            if (_destructionVolume == null) _destructionVolume = SearchUtilities.Find("VolcanicMoon_Body/MoltenCore_VM/DestructionVolume").InstantiateInactive().Rename("Prefab_VM_DestructionVolume").DontDestroyOnLoad();
        }

        public static void Make(GameObject planetGO, Sector sector, OWRigidbody rb, LavaModule module)
        {
            InitPrefabs();

            var multiplier = module.size / 100f;

            var moltenCore = new GameObject("MoltenCore");
            moltenCore.SetActive(false);
            moltenCore.transform.parent = sector?.transform ?? planetGO.transform;
            moltenCore.transform.position = planetGO.transform.position;
            moltenCore.transform.localScale = Vector3.one * module.size;

            var lavaSphere = Object.Instantiate(_lavaSphere, moltenCore.transform);
            lavaSphere.transform.localScale = Vector3.one;
            lavaSphere.transform.name = "LavaSphere";
            lavaSphere.GetComponent<MeshRenderer>().material.SetFloat(HeightScale, 150f * multiplier);
            lavaSphere.GetComponent<MeshRenderer>().material.SetFloat(EdgeFade, 15f * multiplier);
            lavaSphere.GetComponent<MeshRenderer>().material.SetFloat(TexHeight, 15f * multiplier);
            if (module.tint != null) lavaSphere.GetComponent<MeshRenderer>().material.SetColor(EmissionColor, module.tint.ToColor());
            lavaSphere.SetActive(true);

            var sectorCullGroup = lavaSphere.GetComponent<SectorCullGroup>();
            sectorCullGroup.SetSector(sector);

            var moltenCoreProxy = Object.Instantiate(_moltenCoreProxy, moltenCore.transform); ;
            moltenCoreProxy.name = "MoltenCore_Proxy";
            moltenCoreProxy.SetActive(true);

            var proxyLavaSphere = moltenCoreProxy.transform.Find("LavaSphere (1)");
            proxyLavaSphere.transform.localScale = Vector3.one;
            proxyLavaSphere.name = "LavaSphere_Proxy";
            proxyLavaSphere.GetComponent<MeshRenderer>().material.SetFloat(HeightScale, 150f * multiplier);
            proxyLavaSphere.GetComponent<MeshRenderer>().material.SetFloat(EdgeFade, 15f * multiplier);
            proxyLavaSphere.GetComponent<MeshRenderer>().material.SetFloat(TexHeight, 15f * multiplier);
            if (module.tint != null) proxyLavaSphere.GetComponent<MeshRenderer>().material.SetColor(EmissionColor, module.tint.ToColor());

            var sectorProxy = moltenCoreProxy.GetComponent<SectorProxy>();
            sectorProxy._renderers = new List<Renderer> { proxyLavaSphere.GetComponent<MeshRenderer>() };
            sectorProxy.SetSector(sector);

            var destructionVolume = Object.Instantiate(_destructionVolume, moltenCore.transform);
            destructionVolume.name = "DestructionVolume";
            destructionVolume.GetComponent<SphereCollider>().radius = 1;
            destructionVolume.SetActive(true);

            if (module.curve != null)
            {
                var sizeController = moltenCore.AddComponent<LavaSizeController>();
                sizeController.SetScaleCurve(module.curve);
                sizeController.size = module.size;
                sizeController.material = lavaSphere.GetComponent<MeshRenderer>().material;
                sizeController.proxyMaterial = proxyLavaSphere.GetComponent<MeshRenderer>().material;
            }

            moltenCore.SetActive(true);
        }
    }
}
