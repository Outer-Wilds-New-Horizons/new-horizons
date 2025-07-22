using NewHorizons.Builder.Props;
using NewHorizons.External;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility.OuterWilds;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class ForceVolumeBuilder
    {
        public static CylindricalForceVolume Make(GameObject planetGO, Sector sector, CylindricalForceVolumeInfo info)
        {
            var forceVolume = Make<CylindricalForceVolume>(planetGO, sector, info);

            forceVolume._acceleration = info.force;
            forceVolume._localAxis = info.normal ?? Vector3.up;
            forceVolume._playGravityCrystalAudio = info.playGravityCrystalAudio;

            forceVolume.gameObject.SetActive(true);

            return forceVolume;
        }

        public static DirectionalForceVolume Make(GameObject planetGO, Sector sector, DirectionalForceVolumeInfo info)
        {
            var forceVolume = Make<DirectionalForceVolume>(planetGO, sector, info);

            forceVolume._fieldDirection = info.normal ?? Vector3.up;
            forceVolume._fieldMagnitude = info.force;
            forceVolume._affectsAlignment = info.affectsAlignment;
            forceVolume._offsetCentripetalForce = info.offsetCentripetalForce;
            forceVolume._playGravityCrystalAudio = info.playGravityCrystalAudio;

            forceVolume.gameObject.SetActive(true);

            return forceVolume;
        }

        public static GravityVolume Make(GameObject planetGO, Sector sector, GravityVolumeInfo info)
        {
            var forceVolume = Make<GravityVolume>(planetGO, sector, info);

            forceVolume._isPlanetGravityVolume = false;
            forceVolume._setMass = false;
            forceVolume._surfaceAcceleration = info.force;
            forceVolume._upperSurfaceRadius = info.upperRadius;
            forceVolume._lowerSurfaceRadius = info.lowerRadius;
            forceVolume._cutoffAcceleration = info.minForce;
            forceVolume._cutoffRadius = info.minRadius;
            forceVolume._alignmentRadius = info.alignmentRadius ?? info.upperRadius * 1.5f;
            forceVolume._falloffType = info.fallOff switch
            {
                GravityFallOff.Linear => GravityVolume.FalloffType.linear,
                GravityFallOff.InverseSquared => GravityVolume.FalloffType.inverseSquared,
                _ => throw new NotImplementedException(),
            };

            forceVolume.gameObject.SetActive(true);

            return forceVolume;
        }

        public static PolarForceVolume Make(GameObject planetGO, Sector sector, PolarForceVolumeInfo info)
        {
            var forceVolume = Make<PolarForceVolume>(planetGO, sector, info);

            forceVolume._acceleration = info.force;
            forceVolume._localAxis = info.normal ?? Vector3.up;
            forceVolume._fieldMode = info.tangential ? PolarForceVolume.ForceMode.Tangential : PolarForceVolume.ForceMode.Polar;

            forceVolume.gameObject.SetActive(true);

            return forceVolume;
        }

        public static RadialForceVolume Make(GameObject planetGO, Sector sector, RadialForceVolumeInfo info)
        {
            var forceVolume = Make<RadialForceVolume>(planetGO, sector, info);

            forceVolume._acceleration = info.force;
            forceVolume._falloff = info.fallOff switch
            {
                RadialForceVolumeInfo.FallOff.Constant => RadialForceVolume.Falloff.Constant,
                RadialForceVolumeInfo.FallOff.Linear => RadialForceVolume.Falloff.Linear,
                RadialForceVolumeInfo.FallOff.InverseSquared => RadialForceVolume.Falloff.InvSqr,
                _ => throw new NotImplementedException(),
            };

            forceVolume.gameObject.SetActive(true);

            return forceVolume;
        }

        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, ForceVolumeInfo info) where TVolume : ForceVolume
        {
            var forceVolume = PriorityVolumeBuilder.Make<TVolume>(planetGO, sector, info);

            forceVolume._alignmentPriority = info.alignmentPriority;
            forceVolume._inheritable = info.inheritable;

            return forceVolume;
        }
    }
}
