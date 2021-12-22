using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    static class BlackHoleBuilder
    {
        public static void Make(GameObject body, BaseModule module, Sector sector)
        {
            var blackHole = GameObject.Instantiate(GameObject.Find("BrittleHollow_Body/BlackHole_BH"), body.transform);
            blackHole.name = "BlackHole";
            blackHole.transform.localPosition = Vector3.zero;
            //blackHole.transform.localScale = Vector3.one; //* module.BlackHoleSize;

            var blackHoleRenderer = blackHole.transform.Find("BlackHoleRenderer");
            //blackHoleRenderer.transform.localScale = Vector3.one;

            var singularityLOD = blackHoleRenderer.GetComponent<SingularityLOD>();
            singularityLOD.SetSector(sector);

            /*
            var meshRenderer = blackHoleRenderer.GetComponent<MeshRenderer>();
            meshRenderer.material.SetFloat("_Radius", module.BlackHoleSize * 0.4f);

            var owRenderer = blackHoleRenderer.gameObject.AddComponent<OWRenderer>();
            var propID_Radius = Shader.PropertyToID("_Radius");
            owRenderer.SetMaterialProperty(propID_Radius, module.BlackHoleSize * 0.4f);
            */
        }
    }
}
