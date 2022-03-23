using NewHorizons.External.Configs;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class SystemCreationHandler
    {
        public static void LoadSystem(NewHorizonsSystem system)
        {


            var skybox = GameObject.Find("Skybox/Starfield");

            /*
            skybox.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
            */

            /*
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject.Destroy(sphere.GetComponent<SphereCollider>());

            sphere.transform.parent = skybox.transform;
            sphere.transform.localPosition = Vector3.zero;

            var meshFilter = sphere.GetComponent<MeshFilter>();
            meshFilter.mesh.triangles = meshFilter.mesh.triangles.Reverse().ToArray();

            var meshRenderer = sphere.GetComponent<MeshRenderer>();
            meshRenderer.material.color = Color.green;
            */

        }
    }
}
