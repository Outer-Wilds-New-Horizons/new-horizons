using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.EyeOfTheUniverse
{
    [JsonObject]
    public class InstrumentZoneInfo : DetailInfo
    {
        /// <summary>
        /// The unique ID of the Eye Traveler associated with this instrument zone.
        /// </summary>
        public string id;
    }
}
