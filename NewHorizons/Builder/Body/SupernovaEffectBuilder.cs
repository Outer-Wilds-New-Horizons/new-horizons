using UnityEngine;
using NewHorizons.Utility;
using NewHorizons.External.Configs;
using NewHorizons.Components;
using System.Linq;
using NewHorizons.Handlers;
using OWML.Common;

namespace NewHorizons.Builder.Body
{
    public static class SupernovaEffectBuilder
    {
        private static Mesh _shockLayerMesh;
        private static Material _shockLayerMaterial;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_shockLayerMesh == null) _shockLayerMesh = SearchUtilities.Find("GiantsDeep_Body/Shocklayer_GD").GetComponent<MeshFilter>().sharedMesh.DontDestroyOnLoad();
            if (_shockLayerMaterial == null) _shockLayerMaterial = new Material(SearchUtilities.Find("GiantsDeep_Body/Shocklayer_GD").GetComponent<MeshRenderer>().sharedMaterial).Rename("ShockLayer_mat").DontDestroyOnLoad();
        }

        public static NHSupernovaPlanetEffectController Make(GameObject planetGO, Sector sector, PlanetConfig config, IModBehaviour mod, GameObject procGen, Light[] ambientLight, PlanetaryFogController fog, LODGroup atmosphere, Renderer atmosphereRenderer, Renderer fogImpostor)
        {
            InitPrefabs();

            var vanillaController = planetGO.GetComponentInChildren<SupernovaPlanetEffectController>();
            if (vanillaController != null)
            {
                ReplaceVanillaWithNH(vanillaController);
            }

            var currentController = planetGO.GetComponentInChildren<NHSupernovaPlanetEffectController>();
            if (currentController != null)
            {
                if (currentController._ambientLight == null && ambientLight != null)
                {
                    currentController._ambientLight = ambientLight;
                    currentController._ambientLightOrigIntensity = new float[ambientLight.Length];
                    for (int i = 0; i < ambientLight.Length; i++) currentController._ambientLightOrigIntensity[i] = ambientLight[i].intensity;
                }

                if (currentController._atmosphere == null && atmosphere != null) currentController._atmosphere = atmosphere;

                if (config.Atmosphere != null && config.Atmosphere.atmosphereSunIntensity != 0) currentController._atmosphereOrigSunIntensity = config.Atmosphere.atmosphereSunIntensity;

                if (currentController._fog == null && fog != null) currentController._fog = fog;

                return currentController;
            }
            else
            {
                var supernovaController = new GameObject("SupernovaController");
                supernovaController.transform.SetParent(sector?.transform ?? planetGO.transform, false);
                var supernovaEffectController = supernovaController.AddComponent<NHSupernovaPlanetEffectController>();
                if (ambientLight != null)
                {
                    supernovaEffectController._ambientLight = ambientLight;
                    supernovaEffectController._ambientLightOrigIntensity = new float[ambientLight.Length];
                    for (int i = 0; i < ambientLight.Length; i++) supernovaEffectController._ambientLightOrigIntensity[i] = ambientLight[i].intensity;
                }
                if (config.Atmosphere != null && config.Atmosphere.atmosphereSunIntensity != 0) supernovaEffectController._atmosphereOrigSunIntensity = config.Atmosphere.atmosphereSunIntensity;
                supernovaEffectController._atmosphere = atmosphere;
                supernovaEffectController._atmosphereRenderer = atmosphereRenderer;
                supernovaEffectController._fog = fog;
                supernovaEffectController._fogImpostor = fogImpostor;

                var shockLayer = new GameObject("ShockLayer");
                shockLayer.transform.SetParent(sector?.transform ?? planetGO.transform, false);
                shockLayer.AddComponent<MeshFilter>().sharedMesh = _shockLayerMesh;

                var shockLayerRenderer = shockLayer.AddComponent<MeshRenderer>();
                shockLayerRenderer.sharedMaterial = new Material(_shockLayerMaterial);
                supernovaEffectController._shockLayer = shockLayerRenderer;

                var biggestSize = config.Base.surfaceSize;
                var noMeshChange = false;

                if (config.Atmosphere != null)
                {
                    if (config.Atmosphere.size > biggestSize) biggestSize = config.Atmosphere.size;
                    if (config.Atmosphere.fogSize > biggestSize) biggestSize = config.Atmosphere.fogSize;
                    if (config.Atmosphere.clouds != null)
                    {
                        noMeshChange = true;
                        if (config.Atmosphere.clouds.innerCloudRadius > biggestSize) biggestSize = config.Atmosphere.clouds.innerCloudRadius;
                        if (config.Atmosphere.clouds.outerCloudRadius > biggestSize) biggestSize = config.Atmosphere.clouds.outerCloudRadius;
                    }
                }

                if (config.Base.groundSize > biggestSize)
                {
                    noMeshChange = true;
                    biggestSize = config.Base.groundSize;
                }

                if (config.HeightMap != null)
                {
                    if (config.HeightMap.minHeight > biggestSize) biggestSize = config.HeightMap.minHeight;
                    if (config.HeightMap.maxHeight > biggestSize) biggestSize = config.HeightMap.maxHeight;
                }

                if (config.ProcGen != null)
                {
                    if (config.ProcGen.scale > biggestSize) biggestSize = config.ProcGen.scale;
                }

                if (config.Lava != null)
                {
                    noMeshChange = true;
                    var lavaSize = config.Lava.size;
                    if (config.Lava.curve != null) lavaSize *= config.Lava.curve.Max(tvp => tvp.value);
                    if (lavaSize > biggestSize) biggestSize = lavaSize;
                }

                if (config.Water != null)
                {
                    noMeshChange = true;
                    var waterSize = config.Water.size;
                    if (config.Water.curve != null) waterSize *= config.Water.curve.Max(tvp => tvp.value);
                    if (waterSize > biggestSize) biggestSize = waterSize;
                }

                if (config.Sand != null)
                {
                    noMeshChange = true;
                    var sandSize = config.Sand.size;
                    if (config.Sand.curve != null) sandSize *= config.Sand.curve.Max(tvp => tvp.value);
                    if (sandSize > biggestSize) biggestSize = sandSize;
                }

                if (config.Props?.singularities != null)
                {
                    noMeshChange = true;
                    foreach (var singularity in config.Props.singularities)
                    {
                        if (singularity.distortRadius > biggestSize) biggestSize = singularity.distortRadius;
                    }
                }

                var radius = (config.ShockEffect?.radius != null && config.ShockEffect.radius.HasValue) ? config.ShockEffect.radius.Value : biggestSize * 1.1f;

                supernovaEffectController.shockLayerTrailFlare = radius < 500 ? 50 : 100; // Base game all uses 100, but sphere model looks bad if not 1:6 ratio with length like GD, so retain it instead
                supernovaEffectController.shockLayerTrailLength = radius < 500 ? 300 : 600; // GD is the only planet with 600 so ig do it if big

                shockLayer.transform.position = planetGO.transform.position;
                shockLayer.transform.localScale = Vector3.one * radius;

                if (!noMeshChange && config.ShockEffect?.radius == null && procGen != null)
                {
                    shockLayer.GetComponent<MeshFilter>().sharedMesh = procGen.GetComponent<MeshFilter>().sharedMesh;
                    shockLayer.transform.localScale = Vector3.one * 1.1f;
                    shockLayer.transform.rotation = Quaternion.Euler(90, 0, 0);
                    supernovaEffectController.shockLayerTrailFlare = 100;
                    supernovaEffectController.shockLayerTrailLength = 300;
                }

                if (config.ShockEffect != null)
                {
                    if (!string.IsNullOrWhiteSpace(config.ShockEffect.assetBundle) && !string.IsNullOrWhiteSpace(config.ShockEffect.meshPath))
                    {
                        var mesh = AssetBundleUtilities.Load<Mesh>(config.ShockEffect.assetBundle, config.ShockEffect.meshPath, mod);
                        if (mesh != null)
                        {
                            shockLayer.GetComponent<MeshFilter>().sharedMesh = mesh;
                            shockLayer.transform.localScale = Vector3.one * (config.ShockEffect.radius != null ? config.ShockEffect.radius.Value : 1);
                            shockLayer.transform.rotation = Quaternion.Euler(0, 0, 0);
                            supernovaEffectController.shockLayerTrailFlare = 100;
                            supernovaEffectController.shockLayerTrailLength = 300;
                        }
                    }
                }

                return supernovaEffectController;
            }
        }

        public static void ReplaceVanillaWithNH(SupernovaPlanetEffectController vanillaController)
        {
            if (vanillaController._shockLayer != null) vanillaController._shockLayer.gameObject.SetActive(true);
            var supernovaEffectController = vanillaController.gameObject.GetAddComponent<NHSupernovaPlanetEffectController>();
            supernovaEffectController._atmosphere = vanillaController._atmosphere;
            supernovaEffectController._ambientLight = new Light[] { vanillaController._ambientLight };
            supernovaEffectController._ambientLightOrigIntensity = new float[] { vanillaController._ambientLightOrigIntensity };
            supernovaEffectController._atmosphere = vanillaController._atmosphere;
            supernovaEffectController._fog = vanillaController._fog;
            supernovaEffectController._fogOrigTint = vanillaController._fogOrigTint;
            supernovaEffectController._shockLayer = vanillaController._shockLayer;
            supernovaEffectController._shockLayerColor = vanillaController._shockLayerColor;
            supernovaEffectController.shockLayerFullRadius = vanillaController._shockLayerFullRadius;
            supernovaEffectController.shockLayerStartRadius = vanillaController._shockLayerStartRadius;
            supernovaEffectController.shockLayerTrailFlare = vanillaController._shockLayerTrailFlare;
            supernovaEffectController.shockLayerTrailLength = vanillaController._shockLayerTrailLength;
            supernovaEffectController.SunController = SearchUtilities.Find("Sun_Body")?.GetComponent<SunController>();
            Object.Destroy(vanillaController);
        }
    }
}
