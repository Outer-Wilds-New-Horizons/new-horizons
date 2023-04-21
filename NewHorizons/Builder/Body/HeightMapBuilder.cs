using NewHorizons.Builder.Body.Geometry;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.IO;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public static class HeightMapBuilder
    {
        public static Shader PlanetShader;

        public static GameObject Make(GameObject planetGO, Sector sector, HeightMapModule module, IModBehaviour mod, int resolution, bool useLOD = false)
        {
            var deleteHeightmapFlag = false;

            Texture2D heightMap;
            Texture2D textureMap;
            Texture2D smoothnessMap;
            Texture2D normalMap;
            Texture2D emissionMap;
            Texture2D tileBlendMap;
            Texture2D baseTextureTile;
            Texture2D baseSmoothnessTile;
            Texture2D baseNormalTile;
            Texture2D redTextureTile;
            Texture2D redSmoothnessTile;
            Texture2D redNormalTile;
            Texture2D greenTextureTile;
            Texture2D greenSmoothnessTile;
            Texture2D greenNormalTile;
            Texture2D blueTextureTile;
            Texture2D blueSmoothnessTile;
            Texture2D blueNormalTile;
            Texture2D alphaTextureTile;
            Texture2D alphaSmoothnessTile;
            Texture2D alphaNormalTile;
            try
            {
                if (!string.IsNullOrEmpty(module.heightMap) && !File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, module.heightMap)))
                {
                    NHLogger.LogError($"Bad path for {planetGO.name} heightMap: {module.heightMap} couldn't be found.");
                    module.heightMap = null;
                }
                if (string.IsNullOrEmpty(module.heightMap))
                {
                    heightMap = Texture2D.whiteTexture;
                }
                else
                {
                    // TODO we gotta get this working better
                    // If we've loaded a new heightmap we'll delete the texture after
                    // Only delete it if it wasnt loaded before (something else is using it)
                    deleteHeightmapFlag = !ImageUtilities.IsTextureLoaded(mod, module.heightMap);
                    heightMap = ImageUtilities.GetTexture(mod, module.heightMap);
                }

                Texture2D Load(string path, string name, bool linear)
                {
                    if (string.IsNullOrEmpty(path)) return null;
                    if (!File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, module.heightMap)))
                    {
                        NHLogger.LogError($"Bad path for {planetGO.name} {name}: {path} couldn't be found.");
                        return null;
                    }
                    return ImageUtilities.GetTexture(mod, module.textureMap, wrap: true, linear: linear);
                }
                textureMap = Load(module.textureMap, "textureMap", false);
                smoothnessMap = Load(module.smoothnessMap, "smoothnessMap", false);
                normalMap = Load(module.normalMap, "normalMap", true);
                emissionMap = Load(module.emissionMap, "emissionMap", false);
                tileBlendMap = Load(module.tileBlendMap, "tileBlendMap", false);
                baseTextureTile = Load(module.baseTile.textureTile, "baseTile textureTile", false);
                baseSmoothnessTile = Load(module.baseTile.smoothnessTile, "baseTile smoothnessTile", false);
                baseNormalTile = Load(module.baseTile.normalTile, "baseTile normalTile", true);
                redTextureTile = Load(module.redTile.textureTile, "redTile textureTile", false);
                redSmoothnessTile = Load(module.redTile.smoothnessTile, "redTile smoothnessTile", false);
                redNormalTile = Load(module.redTile.normalTile, "redTile normalTile", true);
                greenTextureTile = Load(module.greenTile.textureTile, "greenTile textureTile", false);
                greenSmoothnessTile = Load(module.greenTile.smoothnessTile, "greenTile smoothnessTile", false);
                greenNormalTile = Load(module.greenTile.normalTile, "greenTile normalTile", true);
                blueTextureTile = Load(module.blueTile.textureTile, "blueTile textureTile", false);
                blueSmoothnessTile = Load(module.blueTile.smoothnessTile, "blueTile smoothnessTile", false);
                blueNormalTile = Load(module.blueTile.normalTile, "blueTile normalTile", true);
                alphaTextureTile = Load(module.alphaTile.textureTile, "alphaTile textureTile", false);
                alphaSmoothnessTile = Load(module.alphaTile.smoothnessTile, "alphaTile smoothnessTile", false);
                alphaNormalTile = Load(module.alphaTile.normalTile, "alphaTile normalTile", true);

                // If the texturemap is the same as the heightmap don't delete it #176
                // Do the same with emissionmap
                // honestly, if you do this with other maps, thats on you
                if (heightMap == textureMap || heightMap == emissionMap) deleteHeightmapFlag = false;
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Couldn't load HeightMap textures:\n{e}");
                return null;
            }

            var cubeSphere = new GameObject("CubeSphere");
            cubeSphere.SetActive(false);
            cubeSphere.transform.parent = sector?.transform ?? planetGO.transform;

            if (PlanetShader == null) PlanetShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/SphereTextureWrapperTriplanar.shader");

            var stretch = module.stretch != null ? (Vector3)module.stretch : Vector3.one;

            var emissionColor = module.emissionColor?.ToColor() ?? Color.white;

            var level1 = MakeLODTerrain(
                cubeSphere, heightMap, module.minHeight, module.maxHeight, resolution, stretch, 
                textureMap, smoothnessMap, module.smoothness, module.metallic, normalMap, module.normalStrength, emissionMap, emissionColor,
                tileBlendMap,
                module.baseTile.scale, baseTextureTile, baseSmoothnessTile, baseNormalTile, module.baseTile.normalStrength,
                module.redTile.scale, redTextureTile, redSmoothnessTile, redNormalTile, module.redTile.normalStrength,
                module.greenTile.scale, greenTextureTile, greenSmoothnessTile, greenNormalTile, module.greenTile.normalStrength,
                module.blueTile.scale, blueTextureTile, blueSmoothnessTile, blueNormalTile, module.blueTile.normalStrength,
                module.alphaTile.scale, alphaTextureTile, alphaSmoothnessTile, alphaNormalTile, module.alphaTile.normalStrength
            );

            var cubeSphereMC = cubeSphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = level1.gameObject.GetComponent<MeshFilter>().mesh;

            if (useLOD)
            {
                var level2Res = (int)Mathf.Clamp(resolution / 2f, 1 /*cube moment*/, 100);
                var level2 = MakeLODTerrain(
                    cubeSphere, heightMap, module.minHeight, module.maxHeight, level2Res, stretch, 
                    textureMap, smoothnessMap, module.smoothness, module.metallic, normalMap, module.normalStrength, emissionMap, emissionColor,
                    default,
                    default, default, default, default, default,
                    default, default, default, default, default,
                    default, default, default, default, default,
                    default, default, default, default, default,
                    default, default, default, default, default
                );

                var LODGroup = cubeSphere.AddComponent<LODGroup>();
                LODGroup.size = module.maxHeight;

                LODGroup.SetLODs(new LOD[]
                {
                    new LOD(1 / 3f, new Renderer[] { level1 }),
                    new LOD(0, new Renderer[] { level2 })
                });

                level1.name += "0";
                level2.name += "1";

                LODGroup.RecalculateBounds();
            }

            var cubeSphereSC = cubeSphere.AddComponent<SphereCollider>();
            cubeSphereSC.radius = Mathf.Min(module.minHeight, module.maxHeight);

            var superGroup = planetGO.GetComponent<ProxyShadowCasterSuperGroup>();
            if (superGroup != null) cubeSphere.AddComponent<ProxyShadowCaster>()._superGroup = superGroup;

            // rotate for back compat :P
            cubeSphere.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(0, -90, 0));
            cubeSphere.transform.position = planetGO.transform.position;

            cubeSphere.SetActive(true);

            // Now that we've made the mesh we can delete the heightmap texture
            if (deleteHeightmapFlag) ImageUtilities.DeleteTexture(mod, module.heightMap, heightMap);

            return cubeSphere;
        }

        // lol fuck the stack
        private static MeshRenderer MakeLODTerrain(
            GameObject root, Texture2D heightMap, float minHeight, float maxHeight, int resolution, Vector3 stretch,
            Texture2D textureMap, Texture2D smoothnessMap, float smoothness, float metallic, Texture2D normalMap, float normalStrength, Texture2D emissionMap, Color emissionColor,
            Texture2D tileBlendMap,
            float baseScale, Texture2D baseTextureTile, Texture2D baseSmoothnessTile, Texture2D baseNormalTile, float baseNormalStrength,
            float redScale, Texture2D redTextureTile, Texture2D redSmoothnessTile, Texture2D redNormalTile, float redNormalStrength,
            float greenScale, Texture2D greenTextureTile, Texture2D greenSmoothnessTile, Texture2D greenNormalTile, float greenNormalStrength,
            float blueScale, Texture2D blueTextureTile, Texture2D blueSmoothnessTile, Texture2D blueNormalTile, float blueNormalStrength,
            float alphaScale, Texture2D alphaTextureTile, Texture2D alphaSmoothnessTile, Texture2D alphaNormalTile, float alphaNormalStrength
        )
        {
            var LODCubeSphere = new GameObject("LODCubeSphere");

            LODCubeSphere.AddComponent<MeshFilter>().mesh = CubeSphere.Build(resolution, heightMap, minHeight, maxHeight, stretch);

            var cubeSphereMR = LODCubeSphere.AddComponent<MeshRenderer>();
            var material = new Material(PlanetShader);
            cubeSphereMR.material = material;
            material.name = textureMap.name;
            // string based property lookup. cry about it
            material.mainTexture = textureMap;
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            material.SetTexture("_SmoothnessMap", smoothnessMap);
            material.SetFloat("_BumpStrength", normalStrength);
            material.SetTexture("_BumpMap", normalMap);
            material.SetColor("_EmissionColor", emissionColor);
            material.SetTexture("_EmissionMap", emissionMap);
            material.SetTexture("_BlendMap", tileBlendMap);
            if (baseTextureTile || baseSmoothnessTile || baseNormalTile)
            {
                material.EnableKeyword("BASE_TILE"); 
                material.SetFloat("_BaseTileScale", baseScale);
                material.SetTexture("_BaseTileAlbedo", baseTextureTile);
                material.SetTexture("_BaseTileSmoothnessMap", baseSmoothnessTile);
                material.SetFloat("_BaseTileBumpStrength", baseNormalStrength);
                material.SetTexture("_BaseTileBumpMap", baseNormalTile);
            }
            else
            {
                material.DisableKeyword("BASE_TILE");
            }
            if (redTextureTile || redSmoothnessTile || redNormalTile)
            {
                material.EnableKeyword("RED_TILE"); 
                material.SetFloat("_RedTileScale", redScale);
                material.SetTexture("_RedTileAlbedo", redTextureTile);
                material.SetTexture("_RedTileSmoothnessMap", redSmoothnessTile);
                material.SetFloat("_RedTileBumpStrength", redNormalStrength);
                material.SetTexture("_RedTileBumpMap", redNormalTile);
            }
            else
            {
                material.DisableKeyword("RED_TILE");
            }
            if (greenTextureTile || greenSmoothnessTile || greenNormalTile)
            {
                material.EnableKeyword("GREEN_TILE"); 
                material.SetFloat("_GreenTileScale", greenScale);
                material.SetTexture("_GreenTileAlbedo", greenTextureTile);
                material.SetTexture("_GreenTileSmoothnessMap", greenSmoothnessTile);
                material.SetFloat("_GreenTileBumpStrength", greenNormalStrength);
                material.SetTexture("_GreenTileBumpMap", greenNormalTile);
            }
            else
            {
                material.DisableKeyword("GREEN_TILE");
            }
            if (blueTextureTile || blueSmoothnessTile || blueNormalTile)
            {
                material.EnableKeyword("BLUE_TILE"); 
                material.SetFloat("_BlueTileScale", blueScale);
                material.SetTexture("_BlueTileAlbedo", blueTextureTile);
                material.SetTexture("_BlueTileSmoothnessMap", blueSmoothnessTile);
                material.SetFloat("_BlueTileBumpStrength", blueNormalStrength);
                material.SetTexture("_BlueTileBumpMap", blueNormalTile);
            }
            else
            {
                material.DisableKeyword("BLUE_TILE");
            }
            if (alphaTextureTile || alphaSmoothnessTile || alphaNormalTile)
            {
                material.EnableKeyword("ALPHA_TILE"); 
                material.SetFloat("_AlphaTileScale", alphaScale);
                material.SetTexture("_AlphaTileAlbedo", alphaTextureTile);
                material.SetTexture("_AlphaTileSmoothnessMap", alphaSmoothnessTile);
                material.SetFloat("_AlphaTileBumpStrength", alphaNormalStrength);
                material.SetTexture("_AlphaTileBumpMap", alphaNormalTile);
            }
            else
            {
                material.DisableKeyword("ALPHA_TILE");
            }

            LODCubeSphere.transform.parent = root.transform;
            LODCubeSphere.transform.localPosition = Vector3.zero;

            return cubeSphereMR;
        }
    }
}
