using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.WarpPad
{
    [JsonObject]
    public class NomaiWarpTransmitterInfo : NomaiWarpPadInfo
    {
        /// <summary>
        /// In degrees. Gives a margin of error for alignments.
        /// </summary>
        [DefaultValue(5f)] public float alignmentWindow = 5f;

        /// <summary>
        /// Is this transmitter upsidedown? Means alignment will be checked facing the other way.
        /// </summary>
        public bool upsideDown = false;
    }
}
