using Newtonsoft.Json;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class VariableSizeModule
    {
        /// <summary>
        /// Scale this object over time. Time is in minutes. Value is a multiplier of the size of the object.
        /// </summary>
        public TimeValuePair[] curve;
    }
}
