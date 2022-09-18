using NewHorizons.Components.SizeControllers;
using UnityEngine;
namespace NewHorizons.Components.Stars
{
    public class StarFluidVolume : SimpleFluidVolume
    {
        private StarEvolutionController _starEvolutionController;

        public override void Awake()
        {
            _fluidType = Type.PLASMA;
            _density = 0.1f;
            base.Awake();
        }

        public override void OnEffectVolumeEnter(GameObject hitObj)
        {
            if (_starEvolutionController != null && _starEvolutionController.HasSupernovaStarted()) return;
            base.OnEffectVolumeEnter(hitObj);
        }

        public override void OnEffectVolumeExit(GameObject hitObj)
        {
            if (_starEvolutionController != null && _starEvolutionController.HasSupernovaStarted()) return;
            base.OnEffectVolumeExit(hitObj);
        }

        public void SetStarEvolutionController(StarEvolutionController starEvolutionController)
        {
            _starEvolutionController = starEvolutionController;
        }
    }
}
