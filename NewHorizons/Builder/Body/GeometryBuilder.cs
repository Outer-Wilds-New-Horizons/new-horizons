using UnityEngine;
using NewHorizons.Utility;
namespace NewHorizons.Builder.Body
{
    public static class GeometryBuilder
    {
        private static Mesh _topLayerMesh;

        internal static void InitPrefab()
        {
            if (_topLayerMesh == null) _topLayerMesh = SearchUtilities.Find("CloudsTopLayer_GD")?.GetComponent<MeshFilter>()?.mesh?.DontDestroyOnLoad();
        }

        public static GameObject Make(GameObject planetGO, Sector sector, float groundScale)
        {
            InitPrefab();

            GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            groundGO.transform.name = "GroundSphere";

            groundGO.transform.parent = sector?.transform ?? planetGO.transform;
            groundGO.transform.localScale = new Vector3(groundScale, groundScale, groundScale);
            groundGO.transform.position = planetGO.transform.position;
            if (_topLayerMesh != null)
            {
                groundGO.GetComponent<MeshFilter>().mesh = _topLayerMesh;
                groundGO.GetComponent<SphereCollider>().radius = 1f;
            }
            else
            {
                groundGO.transform.localScale *= 2; // Multiply by 2 to match top layer
            }
            
            var superGroup = planetGO.GetComponent<ProxyShadowCasterSuperGroup>();
            // idk if we need to set _superGroup manually since it does that in Awake, but it's done everywhere else so wtv
            if (superGroup != null) groundGO.AddComponent<ProxyShadowCaster>()._superGroup = superGroup;

            groundGO.SetActive(true);

            return groundGO;
        }
    }
}
