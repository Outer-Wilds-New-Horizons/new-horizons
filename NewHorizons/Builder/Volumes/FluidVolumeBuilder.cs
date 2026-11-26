using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using OWML.Utils;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class FluidVolumeBuilder
    {
        public static FluidVolume Make(GameObject planetGO, Sector sector, FluidVolumeInfo info)
        {
            var type = EnumUtils.Parse(info.type.ToString(), FluidVolume.Type.NONE);
            FluidVolume volume = null;
            switch (type)
            {
                case FluidVolume.Type.PLASMA:
                case FluidVolume.Type.WATER:
                    var radialVolume = PriorityVolumeBuilder.Make<RadialFluidVolume>(planetGO, ref sector, info);
                    radialVolume._radius = info.radius;
                    volume = radialVolume;
                    break;
                case FluidVolume.Type.CLOUD:
                    volume = PriorityVolumeBuilder.Make<CloudLayerFluidVolume>(planetGO, ref sector, info);
                    break;
                default:
                    var sphericalVolume = PriorityVolumeBuilder.Make<SphericalFluidVolume>(planetGO, ref sector, info);
                    sphericalVolume.radius = info.radius;
                    volume = sphericalVolume;
                    break;
            }

            volume._density = info.density;
            volume._fluidType = type;
            volume._alignmentFluid = info.alignmentFluid;
            volume._allowShipAutoroll = info.allowShipAutoroll;
            volume._disableOnStart = info.disableOnStart;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
