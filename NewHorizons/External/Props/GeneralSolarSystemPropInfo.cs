using Newtonsoft.Json;

namespace NewHorizons.External.Props
{
    [JsonObject]
    public abstract class GeneralSolarSystemPropInfo : GeneralPropInfo
    {
        /// <summary>
        /// The name of the planet that will be used with `parentPath`. Must be set if `parentPath` is set.
        /// </summary>
        public string parentBody;
    }
}
