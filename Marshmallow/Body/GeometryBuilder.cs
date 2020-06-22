using UnityEngine;
using Logger = Marshmallow.Utility.Logger;

namespace Marshmallow.Body
{
    static class GeometryBuilder
    {
        public static void Make(GameObject body, float groundScale)
        {
            GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            groundGO.transform.parent = body.transform;
            groundGO.transform.localScale = new Vector3(groundScale / 2, groundScale / 2, groundScale / 2);
            groundGO.GetComponent<MeshFilter>().mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;
            groundGO.GetComponent<SphereCollider>().radius = 1f;
            groundGO.SetActive(true);

            /*
            GameObject sphere = new GameObject();
            Debug.LogError("1");
            sphere.SetActive(false);
            Debug.LogError("2");
            MeshFilter mf = sphere.AddComponent<MeshFilter>();
            Debug.LogError("3");
            mf.mesh = mesh;
            Debug.LogError("4");
            MeshRenderer mr = sphere.AddComponent<MeshRenderer>();
            Debug.LogError("5");
            mr.material = new Material(Shader.Find("Standard"));
            Debug.LogError("6");
            sphere.transform.parent = body.transform;
            Debug.LogError("7");
            sphere.transform.localScale = new Vector3(groundScale / 2, groundScale / 2, groundScale / 2);
            Debug.LogError("8");
            sphere.SetActive(true);
            Debug.LogError("9");
            */

            /*
            var geo = MainClass.assetBundle.LoadAsset<GameObject>("PLANET");
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            geo.GetComponent<Renderer>().material = temp.GetComponent<Renderer>().material;
            GameObject.Destroy(temp);
            geo.transform.parent = body.transform;
            geo.transform.localScale = new Vector3(1,1,1);
            geo.transform.localPosition = new Vector3(0, 0, 0);
            Debug.LogError(geo.name);
            Debug.LogError(geo.GetComponent<MeshFilter>().mesh.name);
            Debug.LogError(geo.transform.parent.name);
            geo.SetActive(true);
            
            */
            Logger.Log("Finished building geometry", Logger.LogType.Log);
        }
    }
}
