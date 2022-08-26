using Newtonsoft.Json;

namespace NewHorizons.Utility
{
    [JsonObject]
    public class TimeValuePair
    {
        /// <summary>
        /// A specific point in time
        /// </summary>
        public float time;

        /// <summary>
        /// The value for this point in time
        /// </summary>
        public float value;
    }
}
