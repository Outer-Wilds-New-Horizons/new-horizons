using NewHorizons.Builder.Body.Geometry;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.IO;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.Body
{
    public static class HeightMapBuilder
    {
        public static Shader PlanetShader;
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        public static GameObject Make(GameObject planetGO, Sector sector, HeightMapModule module, IModBehaviour mod, int resolution, bool useLOD = false)
        {
            var deleteHeightmapFlag = false;

            Texture2D heightMap, textureMap, emissionMap;
            try
            {
                if (!string.IsNullOrEmpty(module.heightMap) && !File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, module.heightMap)))
                {
                    Logger.LogError($"Bad path for {planetGO.name} heightMap: {module.heightMap} couldn't be found.");
                    module.heightMap = null;
                }
                if (!string.IsNullOrEmpty(module.textureMap) && !File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, module.textureMap)))
                {
                    Logger.LogError($"Bad path for {planetGO.name} textureMap: {module.textureMap} couldn't be found.");
                    module.textureMap = null;
                }
                if (!string.IsNullOrEmpty(module.emissionMap) && !File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, module.emissionMap)))
                {
                    Logger.LogError($"Bad path for {planetGO.name} emissionMap: {module.emissionMap} couldn't be found.");
                    module.emissionMap = null;
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

                if (string.IsNullOrEmpty(module.textureMap))
                {
                    textureMap = Texture2D.whiteTexture;
                }
                else
                {
                    textureMap = ImageUtilities.GetTexture(mod, module.textureMap);
                }

                if (string.IsNullOrEmpty(module.emissionMap))
                {
                    emissionMap = Texture2D.blackTexture;
                }
                else
                {
                    emissionMap = ImageUtilities.GetTexture(mod, module.emissionMap);
                }

                // If the texturemap is the same as the heightmap don't delete it #176
                // Do the same with emissionmap
                if (textureMap == heightMap || emissionMap == heightMap) deleteHeightmapFlag = false;
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load HeightMap textures:\n{e}");
                return null;
            }

            var cubeSphere = new GameObject("CubeSphere");
            cubeSphere.SetActive(false);
            cubeSphere.transform.parent = sector?.transform ?? planetGO.transform;

            if (PlanetShader == null) PlanetShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/SphereTextureWrapper.shader");

            var stretch = module.stretch != null ? (Vector3)module.stretch : Vector3.one;

            var emissionColor = module.emissionColor != null ? module.emissionColor.ToColor() : Color.white;

            var level1 = MakeLODTerrain(cubeSphere, heightMap, textureMap, module.minHeight, module.maxHeight, resolution, stretch, 
                emissionMap, emissionColor);

            var cubeSphereMC = cubeSphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = level1.gameObject.GetComponent<MeshFilter>().mesh;

            if (useLOD)
            {
                var level2Res = (int)Mathf.Clamp(resolution / 2f, 1 /*cube moment*/, 100);
                var level2 = MakeLODTerrain(cubeSphere, heightMap, textureMap, module.minHeight, module.maxHeight, level2Res, stretch, 
                    emissionMap, emissionColor);

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

            // Fix rotation in the end
            // 90 degree rotation around x is because cube sphere uses Z as up, Unity uses Y
            cubeSphere.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(90, 0, 0));
            cubeSphere.transform.position = planetGO.transform.position;

            cubeSphere.SetActive(true);

            // Now that we've made the mesh we can delete the heightmap texture
            if (deleteHeightmapFlag) ImageUtilities.DeleteTexture(mod, module.heightMap, heightMap);

            return cubeSphere;
        }

        private static MeshRenderer MakeLODTerrain(GameObject root, Texture2D heightMap, Texture2D textureMap, float minHeight, float maxHeight, int resolution, Vector3 stretch, Texture2D emissionMap, Color emissionColor)
        {
            var LODCubeSphere = new GameObject("LODCubeSphere");

            LODCubeSphere.AddComponent<MeshFilter>().mesh = CubeSphere.Build(resolution, heightMap, minHeight, maxHeight, stretch);

            var cubeSphereMR = LODCubeSphere.AddComponent<MeshRenderer>();
            var material = new Material(PlanetShader);
            cubeSphereMR.material = material;
            material.name = textureMap.name;
            material.mainTexture = textureMap;
            material.SetTexture(EmissionMap, emissionMap);
            material.SetColor(EmissionColor, emissionColor);

            LODCubeSphere.transform.parent = root.transform;
            LODCubeSphere.transform.localPosition = Vector3.zero;

            return cubeSphereMR;
        }
    }
}
