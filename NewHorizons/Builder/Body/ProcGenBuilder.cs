using NewHorizons.Builder.Body.Geometry;
using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    static class ProcGenBuilder
    {
        public static void Make(GameObject go, ProcGenModule module)
        {
            GameObject icosphere = new GameObject("Icosphere");
            icosphere.transform.parent = go.transform;
            icosphere.transform.rotation = Quaternion.Euler(90, 0, 0);
            icosphere.transform.localPosition = Vector3.zero;

            Mesh mesh = Icosphere.Build(4, module.Scale, module.Scale * 1.2f);

            icosphere.AddComponent<MeshFilter>();
            icosphere.GetComponent<MeshFilter>().mesh = mesh;

            var cubeSphereMR = icosphere.AddComponent<MeshRenderer>();
            cubeSphereMR.material = new Material(Shader.Find("Standard"));
            cubeSphereMR.material.color = module.Color != null ? module.Color.ToColor() : Color.white;

            var cubeSphereMC = icosphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = mesh;
            icosphere.transform.localRotation = Quaternion.Euler(90, 0, 0);

            icosphere.AddComponent<ProxyShadowCaster>();
        }
    }
}
