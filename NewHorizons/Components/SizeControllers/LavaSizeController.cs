using NewHorizons.Builder.Body;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.SizeControllers
{
    public class LavaSizeController : SizeController
    {
        public Material material;
        public Material proxyMaterial;

        protected new void FixedUpdate()
        {
            base.FixedUpdate();

            material.SetFloat(LavaBuilder.HeightScale, 150f * CurrentScale / 100f);
            material.SetFloat(LavaBuilder.EdgeFade, 15f * CurrentScale / 100f);
            material.SetFloat(LavaBuilder.TexHeight, 15f * CurrentScale / 100f);

            proxyMaterial.SetFloat(LavaBuilder.HeightScale, 150f * CurrentScale / 100f);
            proxyMaterial.SetFloat(LavaBuilder.EdgeFade, 15f * CurrentScale / 100f);
            proxyMaterial.SetFloat(LavaBuilder.TexHeight, 15f * CurrentScale / 100f);
        }
    }
}
