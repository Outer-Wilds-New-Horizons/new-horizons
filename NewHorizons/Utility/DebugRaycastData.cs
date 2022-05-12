using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility
{
    struct DebugRaycastData
    {
        public bool hit;
        public Vector3 pos;
        public Vector3 norm;

        public string bodyName;
        public string bodyPath;
        public GameObject hitObject;
    }
}
