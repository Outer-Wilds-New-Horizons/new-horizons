using NewHorizons.Builder.Body;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.SizeControllers
{
    public class BlackHoleSizeController : SizeController
    {
        public Material material;
        public AudioSource audioSource;
        public AudioSource oneShotAudioSource;
        public SphereCollider sphereCollider;

        protected new void FixedUpdate()
        {
            base.FixedUpdate();

            material.SetFloat(SingularityBuilder.Radius, CurrentScale * 0.4f);
            material.SetFloat(SingularityBuilder.MaxDistortRadius, CurrentScale * 0.95f);
            material.SetFloat(SingularityBuilder.DistortFadeDist, CurrentScale * 0.55f);

            if (audioSource != null)
            {
                audioSource.maxDistance = CurrentScale * 2.5f;
                audioSource.minDistance = CurrentScale * 0.4f;
            }

            if(oneShotAudioSource != null)
            {
                oneShotAudioSource.maxDistance = CurrentScale * 3f;
                oneShotAudioSource.minDistance = CurrentScale * 0.4f;
            }

            if (sphereCollider != null)
            {
                sphereCollider.radius = CurrentScale * 0.4f;
            }
        }
    }
}
