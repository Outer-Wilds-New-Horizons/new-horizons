using Newtonsoft.Json;

namespace NewHorizons.External.Modules.WarpPad
{
    [JsonObject]
    public abstract class NomaiWarpPadInfo : GeneralPropInfo
    {
        /// <summary>
        /// This can be anything. To have a warp pad transmitter send you to a receiver you must give them the same frequency. 
        /// Try to make it something unique so it does not overlap with other warp pad pairs.
        /// Futhermore, multiple transmitters can send you to the same receiver if they all have the same frequency.
        /// </summary>
        public string frequency;
    }
}
