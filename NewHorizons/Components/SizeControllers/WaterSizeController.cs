using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.SizeControllers
{
    public class WaterSizeController : SizeController
    {
        public Material oceanFogMaterial;

        //private const float oceanFogR1Ratio = 500f / 550f;
        private const float oceanFogR1Ratio = 1f;
        //private const float oceanFogR2Ratio = 210f / 550f;
        private const float oceanFogR2Ratio = 0f;

        protected new void FixedUpdate()
        {
            base.FixedUpdate();
            if (oceanFogMaterial)
            {
                oceanFogMaterial.SetFloat("_Radius", oceanFogR1Ratio * CurrentScale);
                oceanFogMaterial.SetFloat("_Radius2", oceanFogR2Ratio * CurrentScale);
            }
        }
    }
}
