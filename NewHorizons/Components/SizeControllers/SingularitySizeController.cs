using NewHorizons.Builder.Body;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.SizeControllers
{
    public class SingularitySizeController : SizeController
    {
        public Material material;
        public float innerScale;
        public Light light;
        public AudioSource audioSource;
        public AudioSource oneShotAudioSource;
        public SphereCollider sphereCollider;
        public WhiteHoleFluidVolume fluidVolume;
        public WhiteHoleVolume volume;

        protected new void FixedUpdate()
        {
            base.FixedUpdate();

            material.SetFloat(SingularityBuilder.Radius, CurrentScale * innerScale);
            material.SetFloat(SingularityBuilder.MaxDistortRadius, CurrentScale);
            material.SetFloat(SingularityBuilder.DistortFadeDist, CurrentScale - CurrentScale * innerScale);

            if (light != null)
            {
                light.range = CurrentScale * 7f;
            }

            if (audioSource != null)
            {
                audioSource.maxDistance = CurrentScale * 2.5f;
                audioSource.minDistance = CurrentScale * innerScale;
            }

            if(oneShotAudioSource != null)
            {
                oneShotAudioSource.maxDistance = CurrentScale * 3f;
                oneShotAudioSource.minDistance = CurrentScale * innerScale;
            }

            if (sphereCollider != null)
            {
                sphereCollider.radius = CurrentScale * innerScale;
            }

            if (fluidVolume != null)
            {
                fluidVolume._innerRadius = CurrentScale * innerScale;
                fluidVolume._outerRadius = CurrentScale;
            }

            if (volume != null)
            {
                volume._debrisDistMax = CurrentScale * 6.5f;
                volume._debrisDistMin = CurrentScale * 2f;
                volume._radius = CurrentScale * innerScale;
            }
        }
    }
}
