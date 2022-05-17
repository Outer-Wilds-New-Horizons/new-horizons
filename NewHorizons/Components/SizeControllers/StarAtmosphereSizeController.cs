using NewHorizons.Builder.Body;
using UnityEngine;
namespace NewHorizons.Components.SizeControllers
{
    public class StarAtmosphereSizeController : SizeController
    {
        private PlanetaryFogController fog;
        public float initialSize;
        private MeshRenderer[] atmosphereRenderers;

        void Awake()
        {
            fog = GetComponentInChildren<PlanetaryFogController>();
            atmosphereRenderers = transform.Find("AtmoSphere").GetComponentsInChildren<MeshRenderer>();
        }

        protected new void FixedUpdate()
        {
            base.FixedUpdate();
            fog.fogRadius = initialSize * CurrentScale * StarBuilder.OuterRadiusRatio;
            fog.lodFadeDistance = initialSize * CurrentScale * (StarBuilder.OuterRadiusRatio - 1f);
            foreach (var lod in atmosphereRenderers)
            {
                lod.material.SetFloat("_InnerRadius", initialSize * CurrentScale);
                lod.material.SetFloat("_OuterRadius", initialSize * CurrentScale * StarBuilder.OuterRadiusRatio);
            }
        }
    }
}
