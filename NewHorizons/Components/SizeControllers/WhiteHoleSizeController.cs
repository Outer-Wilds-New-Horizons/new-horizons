using NewHorizons.Builder.Body;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.SizeControllers
{
    public class WhiteHoleSizeController : SizeController
    {
        public Material material;
        public Light light;
        public SphereCollider sphereCollider, zeroGSphereCollider;
        public WhiteHoleFluidVolume fluidVolume;
        public WhiteHoleVolume volume;
        public GameObject rulesetVolume;

        public new void FixedUpdate()
        {
            base.FixedUpdate();

            // The "size" parameter set in the config got multiplied by 2.8 so we gotta divide by that
            var trueSize = CurrentScale / 2.8f;

            material.SetFloat(SingularityBuilder.Radius, trueSize * 0.4f);
            material.SetFloat(SingularityBuilder.DistortFadeDist, trueSize);
            material.SetFloat(SingularityBuilder.MaxDistortRadius, trueSize * 2.8f);

            if (light != null)
            {
                light.range = trueSize * 7f;
            }

            if (sphereCollider != null) sphereCollider.radius = trueSize;

            if (fluidVolume != null)
            {
                fluidVolume._innerRadius = trueSize * 0.5f;
                fluidVolume._outerRadius = trueSize;
            }
            
            if (volume != null)
            {
                volume._debrisDistMax = trueSize * 6.5f;
                volume._debrisDistMin = trueSize * 2f;
                volume._radius = trueSize * 0.5f;
            }

            if (zeroGSphereCollider != null) zeroGSphereCollider.radius = trueSize * 10f;
            if (rulesetVolume != null) rulesetVolume.transform.localScale = Vector3.one * trueSize / 100f;
        }
    }
}
