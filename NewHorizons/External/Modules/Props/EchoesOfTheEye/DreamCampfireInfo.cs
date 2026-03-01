using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.EchoesOfTheEye
{
    [JsonObject]
    public class DreamCampfireInfo : CampfireInfo
    {
        /// <summary>
        /// Unique ID for this dream-world campfire
        /// </summary>
        public string id;
    }
}
