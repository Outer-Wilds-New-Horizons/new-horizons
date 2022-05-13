using NewHorizons.Builder.Body.Geometry;
using NewHorizons.Builder.Props;
using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    public static class HeightMapBuilder
    {
        public static Shader PlanetShader;

        public static void Make(GameObject planetGO, Sector sector, HeightMapModule module, IModBehaviour mod)
        {
            Texture2D heightMap, textureMap;
            try
            {
                if (module.HeightMap == null) heightMap = Texture2D.whiteTexture;
                else heightMap = ImageUtilities.GetTexture(mod, module.HeightMap);
                if (module.TextureMap == null) textureMap = Texture2D.whiteTexture;
                else textureMap = ImageUtilities.GetTexture(mod, module.TextureMap);
            }
            catch(Exception e)
            {
                Logger.LogError($"Couldn't load HeightMap textures, {e.Message}, {e.StackTrace}");
                return;
            }

            GameObject cubeSphere = new GameObject("CubeSphere");
            cubeSphere.SetActive(false);
            cubeSphere.transform.parent = sector?.transform ?? planetGO.transform;
            cubeSphere.transform.rotation = Quaternion.Euler(90, 0, 0);

            Vector3 stretch = module.Stretch != null ? (Vector3)module.Stretch : Vector3.one;
            Mesh mesh = CubeSphere.Build(51, heightMap, module.MinHeight, module.MaxHeight, stretch);

            cubeSphere.AddComponent<MeshFilter>();
            cubeSphere.GetComponent<MeshFilter>().mesh = mesh;

            // TODO: fix UVs so we can switch to the default shader
            if (PlanetShader == null) PlanetShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/SphereTextureWrapper.shader");
            //if (PlanetShader == null) PlanetShader = Shader.Find("Standard"); 

            var cubeSphereMR = cubeSphere.AddComponent<MeshRenderer>();
            cubeSphereMR.material = new Material(PlanetShader);
            cubeSphereMR.material.name = textureMap.name;
            cubeSphereMR.material.mainTexture = textureMap;

            var cubeSphereMC = cubeSphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = mesh;

            if(planetGO.GetComponent<ProxyShadowCasterSuperGroup>() != null) cubeSphere.AddComponent<ProxyShadowCaster>();

            // Fix rotation in the end
            cubeSphere.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(90, 0, 0));
            cubeSphere.transform.position = planetGO.transform.position;

            cubeSphere.SetActive(true);
        }
    }
}
