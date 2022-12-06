using UnityEngine;
namespace NewHorizons.Utility
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
                GameObject.Destroy(sphere);
            }

            return sphere.gameObject;
        }

        public static GameObject AddRect(GameObject obj, float width, float height, Color color)
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.name = "DebugPlane";

            try
            {
                plane.GetComponent<MeshCollider>().enabled = false;
                plane.transform.parent = obj.transform;
                plane.transform.localScale = new Vector3(width*0.1f, 0.1f, height*0.1f); // the plane mesh is 10x10, so to get an acurate size, it must be divided by 10
                plane.transform.localEulerAngles = new Vector3(90, 0, 0);

                plane.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                plane.GetComponent<MeshRenderer>().material.color = color;
            }
            catch
            {
                // Something went wrong so make sure the sphere is deleted
                GameObject.Destroy(plane);
            }

            return plane.gameObject;
        }
    }
}
