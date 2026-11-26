using UnityEngine;

namespace NewHorizons.Utility.DebugTools
{
    public struct DebugRaycastData
    {
        public bool hit;
        public Vector3 pos;
        public Vector3 norm;
        public Quaternion rot;
        public DebugRaycastPlane plane;

        public string colliderPath;
        public string bodyPath;
        public GameObject hitBodyGameObject;
        public GameObject hitObject;
    }

    public struct DebugRaycastPlane
    {
        public Vector3 origin;
        public Vector3 normal;
        public Vector3 u; // the "+x" direction on the plane
        public Vector3 v; // the "+y" direction on the plane
    }
}
