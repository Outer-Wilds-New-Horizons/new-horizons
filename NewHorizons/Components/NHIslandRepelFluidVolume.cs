using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class NHIslandRepelFluidVolume : IslandRepelFluidVolume
    {
        [SerializeField]
        private NHFluidVolume _nhFluidVolume;

        public void SetFluidVolume(NHFluidVolume nhFluidVolume)
        {
            _nhFluidVolume = nhFluidVolume;
        }

        public void SetFluidVolume(SphereOceanFluidVolume sphereOceanFluidVolume)
        {
            _oceanFluidVolume = sphereOceanFluidVolume;
        }

        public override Vector3 GetPointFluidVelocity(
          Vector3 worldPosition,
          FluidDetector detector)
        {
            var velocity = _attachedBody.GetPointVelocity(worldPosition);
            if (_nhFluidVolume != null) velocity += _nhFluidVolume.GetPointBarrierVelocity(worldPosition, detector);
            if (_oceanFluidVolume != null) velocity += _oceanFluidVolume.GetPointBarrierVelocity(worldPosition, detector);
            return velocity;
        }
    }
}
