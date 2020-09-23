using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Marshmallow.Utility
{
    static class AddDebugShape
    {
        public static void AddSphere(GameObject obj, float radius, Color color)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.GetComponent<SphereCollider>().enabled = false;
            sphere.transform.parent = obj.transform;
            sphere.transform.localScale = new Vector3(radius, radius, radius);

            sphere.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
            sphere.GetComponent<MeshRenderer>().material.color = color;

            sphere.AddComponent<MakeMeshDoubleFaced>();
        }
    }
}
