using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components
{
    public class RingFluidVolume : SimpleFluidVolume
    {
        public override void OnEffectVolumeEnter(GameObject hitObj)
        {
            FluidDetector fluidDetector = hitObj.GetComponent<FluidDetector>();
            if (fluidDetector == null) return;

            ForceDetector forceDetector = hitObj.GetComponent<ForceDetector>();
            if (forceDetector != null && forceDetector._activeVolumes != null && forceDetector._activeVolumes.Count > 0 && forceDetector._activeVolumes.Where(activeVolume => activeVolume is ForceVolume).Select(activeVolume => activeVolume as ForceVolume).Any(activeVolume => activeVolume.GetAffectsAlignment(forceDetector._attachedBody))) return;
            
            fluidDetector.AddVolume(this);
        }
    }
}
