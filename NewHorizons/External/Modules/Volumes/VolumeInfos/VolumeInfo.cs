using NewHorizons.External.Modules.Props;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class VolumeInfo : GeneralPropInfo
    {
        /// <summary>
        /// The radius of this volume, if a shape is not specified.
        /// </summary>
        [DefaultValue(1f)] public float radius = 1f;

        /// <summary>
        /// The shape of this volume. Defaults to a sphere with a radius of `radius` if not specified.
        /// </summary>
        public ShapeInfo shape;
    }
}
