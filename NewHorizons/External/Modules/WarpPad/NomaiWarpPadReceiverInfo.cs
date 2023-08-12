using NewHorizons.External.Modules.Props;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.WarpPad
{
    [JsonObject]
    public class NomaiWarpReceiverInfo : NomaiWarpPadInfo
    {
        /// <summary>
        /// The body the transmitter must be aligned with to warp to this receiver.
        /// Defaults to the body the receiver is on.
        /// </summary>
        public string alignmentTargetBody;

        /// <summary>
        /// Will create a modern Nomai computer linked to this receiver.
        /// </summary>
        public NomaiComputerInfo computer;

        /// <summary>
        /// Set to true if you want to include Nomai ruin details around the warp pad.
        /// </summary>
        public bool detailed;
    }
}
