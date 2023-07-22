using NewHorizons.Builder.Body;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Components.SizeControllers
{
    public class SingularitySizeController : SizeController
    {
        public Material material;
        public float innerScale;
        public AudioSource audioSource;
        public AudioSource oneShotAudioSource;
        public SphereCollider sphereCollider;
        public WhiteHoleFluidVolume fluidVolume;
        public WhiteHoleVolume volume;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            material.SetFloat(SingularityBuilder.Radius, CurrentScale * innerScale);
            material.SetFloat(SingularityBuilder.MaxDistortRadius, CurrentScale);
            material.SetFloat(SingularityBuilder.DistortFadeDist, CurrentScale - CurrentScale * innerScale);

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

        protected override void Vanish()
        {
            if (audioSource != null)
            {
                audioSource.gameObject.SetActive(false);
                audioSource.Stop();
            }

            if (oneShotAudioSource != null)
            {
                oneShotAudioSource.gameObject.SetActive(false);
            }

            if (sphereCollider != null)
            {
                sphereCollider.gameObject.SetActive(false);
            }

            if (fluidVolume != null)
            {
                fluidVolume.gameObject.SetActive(false);
            }

            if (volume != null)
            {
                volume.gameObject.SetActive(false);
            }
        }

        protected override void Appear()
        {
            if (audioSource != null)
            {
                audioSource.gameObject.SetActive(true);
                Delay.FireOnNextUpdate(audioSource.Play);
            }

            if (oneShotAudioSource != null)
            {
                oneShotAudioSource.gameObject.SetActive(true);
            }

            if (sphereCollider != null)
            {
                sphereCollider.gameObject.SetActive(true);
            }

            if (fluidVolume != null)
            {
                fluidVolume.gameObject.SetActive(true);
            }

            if (volume != null)
            {
                volume.gameObject.SetActive(true);
            }
        }
    }
}
