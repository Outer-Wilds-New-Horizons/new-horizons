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

        /// <summary>
        /// Path to the alarm bell this campfire is connected to.
        /// </summary>
        public string alarmBellPath;

        /// <summary>
        /// Trigger volumes to add the player to when exiting the dream world at this campfire. The player will be temporarily removed from the volumes when entering the dream world.
        /// </summary>
        public string[] entrywayVolumes;
    }
}
