using NewHorizons.Builder.Body.Geometry;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Object = UnityEngine.Object;
namespace NewHorizons.Builder.Body
{
    public static class HeightMapBuilder
    {
        public static Shader PlanetShader;

        public static void Make(GameObject planetGO, Sector sector, HeightMapModule module, IModBehaviour mod, int resolution = 51)
        {
            Texture2D heightMap, textureMap;
            try
            {
                if (module.heightMap == null) heightMap = Texture2D.whiteTexture;
                else
                {
                    heightMap = ImageUtilities.GetTexture(mod, module.heightMap);
                    // defer remove texture to next frame
                    Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => Object.Destroy(heightMap));
                }
                if (module.textureMap == null) textureMap = Texture2D.whiteTexture;
                else textureMap = ImageUtilities.GetTexture(mod, module.textureMap);
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
            //if (PlanetShader == null) PlanetShader = Shader.Find("Standard"); 

            var cubeSphereMR = cubeSphere.AddComponent<MeshRenderer>();
            var material = cubeSphereMR.material;
            material = new Material(PlanetShader);
            cubeSphereMR.material = material;
            material.name = textureMap.name;
            material.mainTexture = textureMap;

            // set up LOD stuff for textures
            var debounceRange = 50;
            var loadDistance = 250;
            var lodHandler = cubeSphere.AddComponent<LODImageLoader>();
            lodHandler.pathsToLoad = new List<string>{ module.textureMap };
            lodHandler.lodSwapEvent.AddListener((List<Texture2D> textures) => material.mainTexture = textures[0]);
            lodHandler.UnloadCondition = () => { return Vector3.Distance(Locator._playerBody.transform.position, planetGO.transform.position) > loadDistance+debounceRange; };
            lodHandler.LoadCondition   = () => { return Vector3.Distance(Locator._playerBody.transform.position, planetGO.transform.position) < loadDistance; };

            // mesh colliders
            var cubeSphereMC = cubeSphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = mesh;

            if (planetGO.GetComponent<ProxyShadowCasterSuperGroup>() != null) cubeSphere.AddComponent<ProxyShadowCaster>();

            // Fix rotation in the end
            cubeSphere.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(90, 0, 0));
            cubeSphere.transform.position = planetGO.transform.position;

            cubeSphere.SetActive(true);
        }
    }
}
