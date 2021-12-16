using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.General
{
    static class BlackHoleBuilder
    {
        public static void Make(GameObject body)
        {
            var blackHole = GameObject.Instantiate(GameObject.Find("BrittleHollow_Body/BlackHole_BH"), body.transform);
            blackHole.name = "BlackHole";
            blackHole.GetComponentInChildren<MeshRenderer>().material.renderQueue = 3001;
            blackHole.transform.localPosition = Vector3.zero;
        }
    }
}
