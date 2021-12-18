using NewHorizons.Body.Geometry;
using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Body
{
    static class HeightMapBuilder
    {
        public static void Make(GameObject go, HeightMapModule module)
        {
            Texture2D heightMap, textureMap;
            try
            {
                heightMap = Main.Instance.CurrentAssets.GetTexture(module.HeightMap);
                textureMap = Main.Instance.CurrentAssets.GetTexture(module.TextureMap);
            }
            catch(Exception e)
            {
                Logger.LogError($"Couldn't load HeightMap textures, {e.Message}, {e.StackTrace}");
                return;
            }

            /*
            GameObject cubeSphere = new GameObject("CubeSphere");
            cubeSphere.transform.parent = go.transform;
            cubeSphere.transform.rotation = Quaternion.Euler(90, 0, 0);

            Mesh mesh = CubeSphere.Build(100, heightMap, module.MinHeight, module.MaxHeight);

            cubeSphere.AddComponent<MeshFilter>();
            cubeSphere.GetComponent<MeshFilter>().mesh = mesh;

            var cubeSphereMR = cubeSphere.AddComponent<MeshRenderer>();
            cubeSphereMR.material = new Material(Shader.Find("Standard"));
            cubeSphereMR.material.mainTexture = textureMap;

            var cubeSphereMC = cubeSphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = mesh;
            */

            GameObject icosphere = new GameObject("Icosphere");
            icosphere.transform.parent = go.transform;
            icosphere.transform.rotation = Quaternion.Euler(90, 0, 0);

            Mesh mesh = Icosphere.Build(5, heightMap, module.MinHeight, module.MaxHeight);

            icosphere.AddComponent<MeshFilter>();
            icosphere.GetComponent<MeshFilter>().mesh = mesh;

            var cubeSphereMR = icosphere.AddComponent<MeshRenderer>();
            cubeSphereMR.material = new Material(Shader.Find("Standard"));
            cubeSphereMR.material.mainTexture = textureMap;

            var cubeSphereMC = icosphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = mesh;
        }
    }
}
