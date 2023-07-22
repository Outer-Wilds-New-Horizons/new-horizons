using Newtonsoft.Json;

namespace NewHorizons.External.Modules.VariableSize
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
