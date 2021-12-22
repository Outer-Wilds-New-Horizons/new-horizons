using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    static class GeometryBuilder
    {
        public static void Make(GameObject body, float groundScale)
        {
            GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            groundGO.transform.parent = body.transform;
            groundGO.transform.localScale = new Vector3(groundScale, groundScale, groundScale);
            groundGO.transform.localPosition = Vector3.zero;
            groundGO.GetComponent<MeshFilter>().mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;
            groundGO.GetComponent<SphereCollider>().radius = 1f;
            groundGO.SetActive(true);
        }
    }
}
