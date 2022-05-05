using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Components
{
    public class NHPlanetaryRaftFix : MonoBehaviour
    {
        private RaftController _raftController;
        private RaftFluidDetector _fluidDetector;
        private FluidVolume _fluidVolume;
        private RaftEffectsController _effectsController;

        public void Awake()
        {
            _raftController = gameObject.GetComponent<RaftController>();
            _fluidDetector = _raftController._fluidDetector;
            _fluidVolume = _fluidDetector._alignmentFluid;
            _effectsController = _raftController._effectsController;
        }

        public void FixedUpdate()
        {
            if (_raftController._raftBody.IsSuspended()) return;
            if (!_raftController._playerInEffectsRange) return;

            // Normally this part won't get called because in RaftController it checks how submerged we are in the Ringworld river
            // Just copy pasted it here using the actual fluid volume instead of making an ugly patch
            float num = _fluidDetector.InFluidType(FluidVolume.Type.WATER) ? _fluidVolume.GetFractionSubmerged(_fluidDetector) : 0f;
            bool allowMovement = num > 0.25f && num < 1f;
            Logger.Log($"AllowMovement? [{allowMovement}]");
            allowMovement = true;
            _effectsController.UpdateMovementAudio(allowMovement, _raftController._lightSensors);
            _effectsController.UpdateGroundedAudio(_fluidDetector);
        }
    }
}
