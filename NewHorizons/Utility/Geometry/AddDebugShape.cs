using UnityEngine;
namespace NewHorizons.Utility.UnityUtilities
{
    public static class AddDebugShape
    {
        public static GameObject AddSphere(GameObject obj, float radius, Color color)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.name = "DebugSphere";

            try
            {
                sphere.GetComponent<SphereCollider>().enabled = false;
                sphere.transform.parent = obj.transform;
                sphere.transform.localScale = new Vector3(radius, radius, radius);

                sphere.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                sphere.GetComponent<MeshRenderer>().material.color = color;
            }
            catch
            {
                // Something went wrong so make sure the sphere is deleted
                Object.Destroy(sphere);
            }

            return sphere.gameObject;
        }
    }
}
