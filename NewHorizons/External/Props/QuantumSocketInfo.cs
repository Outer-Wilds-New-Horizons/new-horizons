using System.ComponentModel;
using Newtonsoft.Json;

namespace NewHorizons.External.Props
{
    [JsonObject]
    public class QuantumSocketInfo : GeneralPropInfo
    {
        /// <summary>
        /// The probability any props that are part of this group will occupy this socket
        /// </summary>
        [DefaultValue(1f)] public float probability = 1f;
    }
}
