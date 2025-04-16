using NewHorizons.External.Modules;
using NewHorizons.Utility;
using System.Collections.Generic;
using System.Linq;
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

        private static GameObject _atmospherePrefab;
        private static GameObject _proxyAtmospherePrefab;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_atmospherePrefab == null) _atmospherePrefab = SearchUtilities.Find("TimberHearth_Body/Atmosphere_TH/AtmoSphere").InstantiateInactive().Rename("Atmosphere").DontDestroyOnLoad();
            if (_proxyAtmospherePrefab == null) _proxyAtmospherePrefab = Object.FindObjectOfType<DistantProxyManager>()._proxies.FirstOrDefault(apt => apt.astroName == AstroObject.Name.TimberHearth).proxyPrefab.FindChild("Atmosphere_TH/Atmosphere_LOD3").InstantiateInactive().Rename("ProxyAtmosphere").DontDestroyOnLoad();
        }

        public static GameObject Make(GameObject planetGO, Sector sector, AtmosphereModule atmosphereModule, float surfaceSize, bool proxy = false)
        {
            InitPrefabs();

            GameObject atmoGO = new GameObject("Atmosphere");
            atmoGO.SetActive(false);
            atmoGO.transform.parent = sector?.transform ?? planetGO.transform;

            if (atmosphereModule.useAtmosphereShader)
            {
                GameObject prefab = proxy ? _proxyAtmospherePrefab : _atmospherePrefab;

                if (prefab != null)
                {
                    GameObject atmo = Object.Instantiate(prefab, atmoGO.transform);
                    atmo.name = "Atmosphere";
                    atmo.transform.localPosition = Vector3.zero;
                    atmo.transform.localEulerAngles = Vector3.zero;
                    atmo.SetActive(true);

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

                        if (Main.Instance.CurrentStarSystem == "EyeOfTheUniverse") material.SetFloat(SunIntensity, 0.2f);
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

            // CullGroups have already set up their renderers when this is done so we need to add ourself to it
            // TODO: There are probably other builders where this is relevant
            // This in particular was a bug affecting hazy dreams
            if (sector != null && sector.gameObject.GetComponent<CullGroup>() is CullGroup cullGroup)
            {
                cullGroup.RecursivelyAddRenderers(atmoGO.transform, true);
                cullGroup.SetVisible(cullGroup.IsVisible());
            }

            return atmoGO;
        }
    }
}
