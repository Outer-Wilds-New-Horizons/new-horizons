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
        /// This makes the alignment happen if the destination planet is BELOW you rather than above.
        /// </summary>
        public bool flipAlignment;
    }
}
