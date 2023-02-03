using UnityEngine;
namespace NewHorizons.Components.SizeControllers
{
    public class WaterSizeController : SizeController
    {
        public Material oceanFogMaterial;
        public RadialFluidVolume fluidVolume;

        private void Start()
        {
            oceanFogMaterial.SetFloat("_Radius2", 0);
        }

        protected new void FixedUpdate()
        {
            base.FixedUpdate();
            if (oceanFogMaterial)
            {
                oceanFogMaterial.SetFloat("_Radius", CurrentScale);
            }
            fluidVolume._radius = CurrentScale;
        }
    }
}
