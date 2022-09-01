using NewHorizons.External.Modules;
using NewHorizons.Utility;
using System.Collections.Generic;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class AtmosphereBuilder
    {
        private static readonly int InnerRadius = Shader.PropertyToID("_InnerRadius");
        private static readonly int OuterRadius = Shader.PropertyToID("_OuterRadius");
        private static readonly int SkyColor = Shader.PropertyToID("_SkyColor");
        private static readonly int SunIntensity = Shader.PropertyToID("_SunIntensity");

        public static readonly List<(GameObject, Material)> Skys = new();

        public static void Init()
        {
            Skys.Clear();
        }

        public static GameObject Make(GameObject planetGO, Sector sector, AtmosphereModule atmosphereModule, float surfaceSize, bool proxy = false)
        {
            GameObject atmoGO = new GameObject("Atmosphere");
            atmoGO.SetActive(false);
            atmoGO.transform.parent = sector?.transform ?? planetGO.transform;

            if (atmosphereModule.useAtmosphereShader)
            {
                GameObject prefab;
                if (proxy) prefab = (SearchUtilities.Find("TimberHearth_DistantProxy", false) ?? SearchUtilities.Find("TimberHearth_DistantProxy(Clone)", false))?
                        .FindChild("Atmosphere_TH/Atmosphere_LOD3");
                else prefab = SearchUtilities.Find("TimberHearth_Body/Atmosphere_TH/AtmoSphere");

                if (prefab != null)
                {
                    GameObject atmo = GameObject.Instantiate(prefab, atmoGO.transform, true);
                    atmo.name = "Atmosphere";
                    atmo.transform.position = planetGO.transform.TransformPoint(Vector3.zero);

                    Material material;

                    if (proxy)
                    {
                        atmo.transform.localScale = Vector3.one * atmosphereModule.size * 1.2f * 2f;

                        var renderer = atmo.GetComponent<MeshRenderer>();
                        material = renderer.material; // makes a new material
                        renderer.sharedMaterial = material;
                    }
                    else
                    {
                        atmo.transform.localScale = Vector3.one * atmosphereModule.size * 1.2f;

                        var renderers = atmo.GetComponentsInChildren<MeshRenderer>();
                        material = renderers[0].material; // makes a new material
                        foreach (var renderer in renderers)
                        {
                            renderer.sharedMaterial = material;
                        }
                    }

                    material.SetFloat(InnerRadius, (atmosphereModule.clouds != null && atmosphereModule.clouds.cloudsPrefab != CloudPrefabType.Transparent) ? atmosphereModule.size : surfaceSize);
                    material.SetFloat(OuterRadius, atmosphereModule.size * 1.2f);
                    if (atmosphereModule.atmosphereTint != null) material.SetColor(SkyColor, atmosphereModule.atmosphereTint.ToColor());

                    atmo.SetActive(true);

                    if (atmosphereModule.atmosphereSunIntensity == 0)
                    {
                        // Do it based on distance
                        Skys.Add((planetGO, material));
                    }
                    else
                    {
                        // Use the override instead
                        material.SetFloat(SunIntensity, atmosphereModule.atmosphereSunIntensity);
                    }
                }
            }

            atmoGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
            atmoGO.SetActive(true);

            return atmoGO;
        }
    }
}
