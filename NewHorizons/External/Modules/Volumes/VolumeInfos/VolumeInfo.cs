using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class VolumeInfo : GeneralPointPropInfo
    {
        /// <summary>
        /// The radius of this volume.
        /// </summary>
        [DefaultValue(1f)] public float radius = 1f;
    }
}
