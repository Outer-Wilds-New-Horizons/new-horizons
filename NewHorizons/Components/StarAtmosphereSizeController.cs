using NewHorizons.Builder.Body;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
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
            foreach (var lod in atmosphereRenderers)
            {
                lod.material.SetFloat("_InnerRadius", initialSize * CurrentScale);
                lod.material.SetFloat("_OuterRadius", initialSize * CurrentScale * StarBuilder.OuterRadiusRatio);
            }
        }
    }
}
