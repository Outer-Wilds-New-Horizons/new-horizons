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
        public float multiplier;

        protected new void FixedUpdate()
        {
            base.FixedUpdate();

            material.SetFloat(LavaBuilder.HeightScale, 150f * multiplier * CurrentScale);
            material.SetFloat(LavaBuilder.EdgeFade, 15f * multiplier * CurrentScale);
            material.SetFloat(LavaBuilder.TexHeight, 15f * multiplier * CurrentScale);
        }
    }
}
