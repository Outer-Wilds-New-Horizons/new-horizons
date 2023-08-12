using Newtonsoft.Json;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class VariableSizeModule
    {
        /// <summary>
        /// Scale this object over time. Time value is in minutes.
        /// </summary>
        public TimeValuePair[] curve;
    }
}