#region

using UnityEngine;

#endregion

namespace NewHorizons.Utility.DebugUtilities
{
    internal struct DebugRaycastData
    {
        public bool hit;
        public Vector3 pos;
        public Vector3 norm;

        public string bodyName;
        public string bodyPath;
        public GameObject hitObject;
    }
}