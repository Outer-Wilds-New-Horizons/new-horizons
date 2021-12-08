using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.General
{
    static class LavaBuilder
    {
        public static void Make(GameObject body, float lavaSize) 
        {
            var lava = GameObject.Instantiate(GameObject.Find("VolcanicMoon_Body/MoltenCore_VM"), body.transform);
            lava.transform.localPosition = Vector3.zero;
            lava.transform.Find("LavaSphere").transform.localScale = Vector3.one * lavaSize;
            lava.transform.Find("MoltenCore_Proxy/LavaSphere (1)").transform.localScale = Vector3.one * lavaSize;
            // Destruction volume is broken
            lava.transform.Find("DestructionVolume").GetComponent<SphereCollider>().radius = lavaSize;
        }
    }
}
