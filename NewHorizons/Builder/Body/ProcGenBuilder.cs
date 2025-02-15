using NewHorizons.Builder.Body.Geometry;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using OWML.Common;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public static class ProcGenBuilder
    {
        private static Material _material;
        private static Shader _planetShader;

        private static Dictionary<ProcGenModule, Material> _materialCache = new();

        public static void ClearCache()
        {
            foreach (var material in _materialCache.Values)
            {
                Object.Destroy(material);
            }
            _materialCache.Clear();
        }

        private static Material MakeMaterial()
        {
            var material = new Material(_planetShader);

            var keyword = "BASE_TILE";
            var prefix = "_BaseTile";

            material.SetFloat(prefix, 1);
            material.EnableKeyword(keyword);

            material.SetTexture("_BlendMap", ImageUtilities.MakeSolidColorTexture(1, 1, Color.white));

            return material;
        }

        public static GameObject Make(IModBehaviour mod, GameObject planetGO, Sector sector, ProcGenModule module)
        {
            if (_planetShader == null) _planetShader = AssetBundleUtilities.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/SphereTextureWrapperTriplanar.shader");
            if (_material == null) _material = MakeMaterial();

            var icosphere = new GameObject("Icosphere");
            icosphere.SetActive(false);
            icosphere.transform.parent = sector?.transform ?? planetGO.transform;
            icosphere.transform.rotation = Quaternion.Euler(90, 0, 0);
            icosphere.transform.position = planetGO.transform.position;

            Mesh mesh = Icosphere.Build(4, module.scale, module.scale * 1.2f);

            icosphere.AddComponent<MeshFilter>().mesh = mesh;

            var cubeSphereMR = icosphere.AddComponent<MeshRenderer>();

            if (!_materialCache.TryGetValue(module, out var material))
            {
                material = new Material(_material);
                material.name = planetGO.name;
                if (module.material == ProcGenModule.Material.Default)
                {
                    if (!string.IsNullOrEmpty(module.texture))
                    {
                        material.SetTexture($"_BaseTileAlbedo", ImageUtilities.GetTexture(mod, module.texture, wrap: true));
                    }
                    else
                    {
                        material.mainTexture = ImageUtilities.MakeSolidColorTexture(1, 1, module.color?.ToColor() ?? Color.white);
                    }
                    if (!string.IsNullOrEmpty(module.smoothnessMap))
                    {
                        material.SetTexture($"_BaseTileSmoothnessMap", ImageUtilities.GetTexture(mod, module.smoothnessMap, wrap: true));
                    }
                    if (!string.IsNullOrEmpty(module.normalMap))
                    {
                        material.SetFloat($"_BaseTileBumpStrength", module.normalStrength);
                        material.SetTexture($"_BaseTileBumpMap", ImageUtilities.GetTexture(mod, module.normalMap, wrap: true));
                    }
                }
                else
                {
                    switch (module.material)
                    {
                        case ProcGenModule.Material.Ice:
                            material.SetTexture($"_BaseTileAlbedo", ImageUtilities.GetTexture(Main.Instance, "Assets/textures/Ice.png", wrap: true));
                            break;
                        case ProcGenModule.Material.Quantum:
                            material.SetTexture($"_BaseTileAlbedo", ImageUtilities.GetTexture(Main.Instance, "Assets/textures/Quantum.png", wrap: true));
                            break;
                        case ProcGenModule.Material.Rock:
                            material.SetTexture($"_BaseTileAlbedo", ImageUtilities.GetTexture(Main.Instance, "Assets/textures/Rocks.png", wrap: true));
                            break;
                        default:
                            break;
                    }
                    material.SetFloat($"_BaseTileScale", 5 / module.scale);
                    if (module.color != null)
                    {
                        material.color = module.color.ToColor();
                    }
                }

                material.SetFloat("_Smoothness", module.smoothness);
                material.SetFloat("_Metallic", module.metallic);

                _materialCache[module] = material;
            }

            cubeSphereMR.sharedMaterial = material;

            var cubeSphereMC = icosphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = mesh;
            icosphere.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(90, 0, 0));

            var cubeSphereSC = icosphere.AddComponent<SphereCollider>();
            cubeSphereSC.radius = module.scale;

            var superGroup = planetGO.GetComponent<ProxyShadowCasterSuperGroup>();
            if (superGroup != null) icosphere.AddComponent<ProxyShadowCaster>()._superGroup = superGroup;

            icosphere.SetActive(true);
            return icosphere;
        }
    }
}
