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
            Texture2D tileBlendMap = null;
            Texture2D baseTextureTile = null;
            Texture2D baseSmoothnessTile = null;
            Texture2D baseNormalTile = null;
            Texture2D redTextureTile = null;
            Texture2D redSmoothnessTile = null;
            Texture2D redNormalTile = null;
            Texture2D greenTextureTile = null;
            Texture2D greenSmoothnessTile = null;
            Texture2D greenNormalTile = null;
            Texture2D blueTextureTile = null;
            Texture2D blueSmoothnessTile = null;
            Texture2D blueNormalTile = null;
            Texture2D alphaTextureTile = null;
            Texture2D alphaSmoothnessTile = null;
            Texture2D alphaNormalTile = null;
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
                    if (!File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, path)))
                    {
                        NHLogger.LogError($"Bad path for {planetGO.name} {name}: {path} couldn't be found.");
                        return null;
                    }
                    return ImageUtilities.GetTexture(mod, path, wrap: true, linear: linear);
                }
                textureMap = Load(module.textureMap, "textureMap", false);
                smoothnessMap = Load(module.smoothnessMap, "smoothnessMap", false);
                normalMap = Load(module.normalMap, "normalMap", true);
                emissionMap = Load(module.emissionMap, "emissionMap", false);

                if (useLOD)
                {
                    tileBlendMap = Load(module.tileBlendMap, "tileBlendMap", false);
                    if (module.baseTile != null)
                    {
                        baseTextureTile = Load(module.baseTile.textureTile, "baseTile textureTile", false);
                        baseSmoothnessTile = Load(module.baseTile.smoothnessTile, "baseTile smoothnessTile", false);
                        baseNormalTile = Load(module.baseTile.normalTile, "baseTile normalTile", true);
                    }
                    if (module.redTile != null)
                    {
                        redTextureTile = Load(module.redTile.textureTile, "redTile textureTile", false);
                        redSmoothnessTile = Load(module.redTile.smoothnessTile, "redTile smoothnessTile", false);
                        redNormalTile = Load(module.redTile.normalTile, "redTile normalTile", true);
                    }
                    if (module.greenTile != null)
                    {
                        greenTextureTile = Load(module.greenTile.textureTile, "greenTile textureTile", false);
                        greenSmoothnessTile = Load(module.greenTile.smoothnessTile, "greenTile smoothnessTile", false);
                        greenNormalTile = Load(module.greenTile.normalTile, "greenTile normalTile", true);
                    }
                    if (module.blueTile != null)
                    {
                        blueTextureTile = Load(module.blueTile.textureTile, "blueTile textureTile", false);
                        blueSmoothnessTile = Load(module.blueTile.smoothnessTile, "blueTile smoothnessTile", false);
                        blueNormalTile = Load(module.blueTile.normalTile, "blueTile normalTile", true);
                    }
                    if (module.alphaTile != null)
                    {
                        alphaTextureTile = Load(module.alphaTile.textureTile, "alphaTile textureTile", false);
                        alphaSmoothnessTile = Load(module.alphaTile.smoothnessTile, "alphaTile smoothnessTile", false);
                        alphaNormalTile = Load(module.alphaTile.normalTile, "alphaTile normalTile", true);
                    }
                }

                // If the texturemap is the same as the heightmap don't delete it #176
                // Do the same with emissionmap
                // todo? maybe do this with the other maps, if someone were to ever use heightmap for them (altho that wouldnt make any sense if they did)
                if (textureMap == heightMap || emissionMap == heightMap) deleteHeightmapFlag = false;
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

            var level1 = MakeLODTerrain(resolution, useLOD);

            var cubeSphereMC = cubeSphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = level1.gameObject.GetComponent<MeshFilter>().mesh;

            if (useLOD)
            {
                var level2Res = (int)Mathf.Clamp(resolution / 2f, 1 /*cube moment*/, 100);
                var level2 = MakeLODTerrain(level2Res, false);

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
            cubeSphereSC.radius = Mathf.Min(module.minHeight, module.maxHeight) * Mathf.Min(stretch.x, stretch.y, stretch.z);

            var superGroup = planetGO.GetComponent<ProxyShadowCasterSuperGroup>();
            if (superGroup != null) cubeSphere.AddComponent<ProxyShadowCaster>()._superGroup = superGroup;

            // rotate for back compat :P
            cubeSphere.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(0, -90, 0));
            cubeSphere.transform.position = planetGO.transform.position;

            cubeSphere.SetActive(true);

            // Now that we've made the mesh we can delete the heightmap texture
            if (deleteHeightmapFlag) ImageUtilities.DeleteTexture(mod, module.heightMap, heightMap);

            return cubeSphere;



            MeshRenderer MakeLODTerrain(int resolution, bool useTriplanar)
            {
                var LODCubeSphere = new GameObject("LODCubeSphere");

                LODCubeSphere.AddComponent<MeshFilter>().mesh = CubeSphere.Build(resolution, heightMap, module.minHeight, module.maxHeight, stretch);

                var cubeSphereMR = LODCubeSphere.AddComponent<MeshRenderer>();
                var material = new Material(PlanetShader);
                cubeSphereMR.material = material;
                material.name = textureMap.name;

                material.mainTexture = textureMap;
                material.SetFloat("_Smoothness", module.smoothness);
                material.SetFloat("_Metallic", module.metallic);
                material.SetTexture("_SmoothnessMap", smoothnessMap);
                material.SetFloat("_BumpStrength", module.normalStrength);
                material.SetTexture("_BumpMap", normalMap);
                material.SetColor("_EmissionColor", emissionColor);
                material.SetTexture("_EmissionMap", emissionMap);

                if (useTriplanar)
                {
                    material.SetTexture("_BlendMap", tileBlendMap);
                    if (module.baseTile != null)
                    {
                        material.EnableKeyword("BASE_TILE");
                        material.SetFloat("_BaseTileScale", 1 / module.baseTile.size);
                        material.SetTexture("_BaseTileAlbedo", baseTextureTile);
                        material.SetTexture("_BaseTileSmoothnessMap", baseSmoothnessTile);
                        material.SetFloat("_BaseTileBumpStrength", module.baseTile.normalStrength);
                        material.SetTexture("_BaseTileBumpMap", baseNormalTile);
                    }
                    else
                    {
                        material.DisableKeyword("BASE_TILE");
                    }
                    if (module.redTile != null)
                    {
                        material.EnableKeyword("RED_TILE");
                        material.SetFloat("_RedTileScale", 1 / module.redTile.size);
                        material.SetTexture("_RedTileAlbedo", redTextureTile);
                        material.SetTexture("_RedTileSmoothnessMap", redSmoothnessTile);
                        material.SetFloat("_RedTileBumpStrength", module.redTile.normalStrength);
                        material.SetTexture("_RedTileBumpMap", redNormalTile);
                    }
                    else
                    {
                        material.DisableKeyword("RED_TILE");
                    }
                    if (module.greenTile != null)
                    {
                        material.EnableKeyword("GREEN_TILE");
                        material.SetFloat("_GreenTileScale", 1 / module.greenTile.size);
                        material.SetTexture("_GreenTileAlbedo", greenTextureTile);
                        material.SetTexture("_GreenTileSmoothnessMap", greenSmoothnessTile);
                        material.SetFloat("_GreenTileBumpStrength", module.greenTile.normalStrength);
                        material.SetTexture("_GreenTileBumpMap", greenNormalTile);
                    }
                    else
                    {
                        material.DisableKeyword("GREEN_TILE");
                    }
                    if (module.blueTile != null)
                    {
                        material.EnableKeyword("BLUE_TILE");
                        material.SetFloat("_BlueTileScale", 1 / module.blueTile.size);
                        material.SetTexture("_BlueTileAlbedo", blueTextureTile);
                        material.SetTexture("_BlueTileSmoothnessMap", blueSmoothnessTile);
                        material.SetFloat("_BlueTileBumpStrength", module.blueTile.normalStrength);
                        material.SetTexture("_BlueTileBumpMap", blueNormalTile);
                    }
                    else
                    {
                        material.DisableKeyword("BLUE_TILE");
                    }
                    if (module.alphaTile != null)
                    {
                        material.EnableKeyword("ALPHA_TILE");
                        material.SetFloat("_AlphaTileScale", 1 / module.alphaTile.size);
                        material.SetTexture("_AlphaTileAlbedo", alphaTextureTile);
                        material.SetTexture("_AlphaTileSmoothnessMap", alphaSmoothnessTile);
                        material.SetFloat("_AlphaTileBumpStrength", module.alphaTile.normalStrength);
                        material.SetTexture("_AlphaTileBumpMap", alphaNormalTile);
                    }
                    else
                    {
                        material.DisableKeyword("ALPHA_TILE");
                    }
                }

                LODCubeSphere.transform.parent = cubeSphere.transform;
                LODCubeSphere.transform.localPosition = Vector3.zero;

                return cubeSphereMR;
            }
        }
    }
}
