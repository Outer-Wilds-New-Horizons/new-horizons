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

        public static void Make(GameObject planetGO, Sector sector, HeightMapModule module, IModBehaviour mod,
            int resolution = 51)
        {
            Texture2D heightMap, textureMap;
            try
            {
                if (module.heightMap == null) heightMap = Texture2D.whiteTexture;
                else heightMap = ImageUtilities.GetTexture(mod, module.heightMap);
                if (module.textureMap == null) textureMap = Texture2D.whiteTexture;
                else textureMap = ImageUtilities.GetTexture(mod, module.textureMap);
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load HeightMap textures, {e.Message}, {e.StackTrace}");
                return;
            }

            var cubeSphere = new GameObject("CubeSphere");
            cubeSphere.SetActive(false);
            cubeSphere.transform.parent = sector?.transform ?? planetGO.transform;
            cubeSphere.transform.rotation = Quaternion.Euler(90, 0, 0);

            var stretch = module.stretch != null ? (Vector3)module.stretch : Vector3.one;
            var mesh = CubeSphere.Build(resolution, heightMap, module.minHeight, module.maxHeight, stretch);

            cubeSphere.AddComponent<MeshFilter>();
            cubeSphere.GetComponent<MeshFilter>().mesh = mesh;

            // TODO: fix UVs so we can switch to the default shader
            if (PlanetShader == null)
                PlanetShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/SphereTextureWrapper.shader");
            //if (PlanetShader == null) PlanetShader = Shader.Find("Standard"); 

            var cubeSphereMR = cubeSphere.AddComponent<MeshRenderer>();
            var material = cubeSphereMR.material;
            material = new Material(PlanetShader);
            cubeSphereMR.material = material;
            material.name = textureMap.name;
            material.mainTexture = textureMap;

            var cubeSphereMC = cubeSphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = mesh;

            if (planetGO.GetComponent<ProxyShadowCasterSuperGroup>() != null)
                cubeSphere.AddComponent<ProxyShadowCaster>();

            // Fix rotation in the end
            cubeSphere.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(90, 0, 0));
            cubeSphere.transform.position = planetGO.transform.position;

            cubeSphere.SetActive(true);
        }
    }
}