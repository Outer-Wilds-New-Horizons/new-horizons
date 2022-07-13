using NewHorizons.Builder.Body.Geometry;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using OWML.Common;
using System;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.Body
{
    public static class HeightMapBuilder
    {
        public static Shader PlanetShader;

        public static void Make(GameObject planetGO, Sector sector, HeightMapModule module, IModBehaviour mod, int resolution)
        {
            var deleteHeightmapFlag = false;

            Texture2D heightMap, textureMap;
            try
            {
                if (module.heightMap == null)
                {
                    heightMap = Texture2D.whiteTexture;
                }
                else
                {
                    // If we've loaded a new heightmap we'll delete the texture after
                    // Only delete it if it wasnt loaded before (something else is using it)
                    deleteHeightmapFlag = !ImageUtilities.IsTextureLoaded(mod, module.heightMap);
                    heightMap = ImageUtilities.GetTexture(mod, module.heightMap);
                }

                if (module.textureMap == null)
                {
                    textureMap = Texture2D.whiteTexture;
                }
                else
                {
                    textureMap = ImageUtilities.GetTexture(mod, module.textureMap);
                }

                // If the texturemap is the same as the heightmap don't delete it #176
                if (textureMap == heightMap) deleteHeightmapFlag = false;
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load HeightMap textures, {e.Message}, {e.StackTrace}");
                return;
            }

            GameObject cubeSphere = new GameObject("CubeSphere");
            cubeSphere.SetActive(false);
            cubeSphere.transform.parent = sector?.transform ?? planetGO.transform;
            cubeSphere.transform.rotation = Quaternion.Euler(90, 0, 0);

            Vector3 stretch = module.stretch != null ? (Vector3)module.stretch : Vector3.one;
            Mesh mesh = CubeSphere.Build(resolution, heightMap, module.minHeight, module.maxHeight, stretch);

            cubeSphere.AddComponent<MeshFilter>();
            cubeSphere.GetComponent<MeshFilter>().mesh = mesh;

            if (PlanetShader == null) PlanetShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/SphereTextureWrapper.shader");

            var cubeSphereMR = cubeSphere.AddComponent<MeshRenderer>();
            var material = new Material(PlanetShader);
            cubeSphereMR.material = material;
            material.name = textureMap.name;
            material.mainTexture = textureMap;

            var cubeSphereMC = cubeSphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = mesh;

            var cubeSphereSC = cubeSphere.AddComponent<SphereCollider>();
            cubeSphereSC.radius = Math.Min(module.minHeight, module.maxHeight);

            var superGroup = planetGO.GetComponent<ProxyShadowCasterSuperGroup>();
            if (superGroup != null) cubeSphere.AddComponent<ProxyShadowCaster>()._superGroup = superGroup;

            // Fix rotation in the end
            // 90 degree rotation around x is because cube sphere uses Z as up, Unity uses Y
            cubeSphere.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(90, 0, 0));
            cubeSphere.transform.position = planetGO.transform.position;

            cubeSphere.SetActive(true);

            // Now that we've made the mesh we can delete the heightmap texture
            if (deleteHeightmapFlag) ImageUtilities.DeleteTexture(mod, module.heightMap, heightMap);
        }
    }
}
