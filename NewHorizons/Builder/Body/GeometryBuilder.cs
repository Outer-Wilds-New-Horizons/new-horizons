using UnityEngine;
using NewHorizons.Utility;
namespace NewHorizons.Builder.Body
{
    public static class GeometryBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, float groundScale)
        {
            GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            groundGO.transform.name = "GroundSphere";

            groundGO.transform.parent = sector?.transform ?? planetGO.transform;
            groundGO.transform.localScale = new Vector3(groundScale, groundScale, groundScale);
            groundGO.transform.position = planetGO.transform.position;
            groundGO.GetComponent<MeshFilter>().mesh = SearchUtilities.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;
            groundGO.GetComponent<SphereCollider>().radius = 1f;
            groundGO.SetActive(true);
        }
    }
}
