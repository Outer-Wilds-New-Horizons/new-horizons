using UnityEngine;
using NewHorizons.Utility;
using NewHorizons.External.Configs;
using NewHorizons.Components;
using System.Linq;
using NewHorizons.Handlers;

namespace NewHorizons.Builder.Body
{
    public static class SupernovaEffectBuilder
    {
        public static NHSupernovaPlanetEffectController Make(GameObject planetGO, Sector sector, PlanetConfig config, GameObject heightMap, GameObject procGen, Light ambientLight, PlanetaryFogController fog, LODGroup atmosphere, Renderer atmosphereRenderer, Renderer fogImpostor)
        {
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
                    currentController._ambientLightOrigIntensity = config.Base.ambientLight;
                }

                if (currentController._atmosphere == null && atmosphere != null) currentController._atmosphere = atmosphere;

                if (currentController._fog == null && fog != null) currentController._fog = fog;

                return currentController;
            }
            else
            {
                var supernovaController = new GameObject("SupernovaController");
                supernovaController.transform.SetParent(sector?.transform ?? planetGO.transform, false);
                var supernovaEffectController = supernovaController.AddComponent<NHSupernovaPlanetEffectController>();
                supernovaEffectController._ambientLight = ambientLight;
                supernovaEffectController._ambientLightOrigIntensity = config.Base.ambientLight;
                supernovaEffectController._atmosphere = atmosphere;
                supernovaEffectController._atmosphereRenderer = atmosphereRenderer;
                supernovaEffectController._fog = fog;
                supernovaEffectController._fogImpostor = fogImpostor;

                var shockLayerGD = SearchUtilities.Find("GiantsDeep_Body/Shocklayer_GD");
                var shockLayer = new GameObject("ShockLayer");
                shockLayer.transform.SetParent(sector?.transform ?? planetGO.transform, false);
                shockLayer.AddComponent<MeshFilter>().sharedMesh = shockLayerGD.GetComponent<MeshFilter>().sharedMesh;

                var shockLayerMaterial = new Material(shockLayerGD.GetComponent<MeshRenderer>().sharedMaterial);
                shockLayerMaterial.name = "ShockLayer_mat";

                var shockLayerRenderer = shockLayer.AddComponent<MeshRenderer>();
                shockLayerRenderer.sharedMaterial = shockLayerMaterial;
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
                        if (singularity.size > biggestSize) biggestSize = singularity.size;
                    }
                }

                supernovaEffectController._shockLayerStartRadius = biggestSize;
                supernovaEffectController._shockLayerFullRadius = biggestSize * 10f;
                supernovaEffectController._shockLayerTrailFlare = 100;
                supernovaEffectController._shockLayerTrailLength = biggestSize < 600 ? 300 : 600;

                shockLayer.transform.position = planetGO.transform.position;
                shockLayer.transform.localScale = Vector3.one * biggestSize * 1.1f;

                if (!noMeshChange)
                {
                    if (heightMap != null)
                    {
                        shockLayer.GetComponent<MeshFilter>().sharedMesh = heightMap.GetComponent<MeshCollider>().sharedMesh;
                        shockLayer.transform.localScale = Vector3.one * 1.1f;
                        shockLayer.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(90, 0, 0));
                    }
                    else if (procGen != null)
                    {
                        shockLayer.GetComponent<MeshFilter>().sharedMesh = procGen.GetComponent<MeshFilter>().sharedMesh;
                        shockLayer.transform.localScale = Vector3.one * 1.1f;
                        shockLayer.transform.rotation = Quaternion.Euler(90, 0, 0);
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
            supernovaEffectController._ambientLight = vanillaController._ambientLight;
            supernovaEffectController._ambientLightOrigIntensity = vanillaController._ambientLightOrigIntensity;
            supernovaEffectController._atmosphere = vanillaController._atmosphere;
            supernovaEffectController._fog = vanillaController._fog;
            supernovaEffectController._fogOrigTint = vanillaController._fogOrigTint;
            supernovaEffectController._shockLayer = vanillaController._shockLayer;
            supernovaEffectController._shockLayerColor = vanillaController._shockLayerColor;
            supernovaEffectController._shockLayerFullRadius = vanillaController._shockLayerFullRadius;
            supernovaEffectController._shockLayerStartRadius = vanillaController._shockLayerStartRadius;
            supernovaEffectController._shockLayerTrailFlare = vanillaController._shockLayerTrailFlare;
            supernovaEffectController._shockLayerTrailLength = vanillaController._shockLayerTrailLength;
            supernovaEffectController._sunController = SearchUtilities.Find("Sun_Body").GetComponent<SunController>();
            Object.Destroy(vanillaController);
        }
    }
}
