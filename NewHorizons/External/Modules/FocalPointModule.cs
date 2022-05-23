using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class FocalPointModule
    {
        /// <summary>
        /// Name of the primary planet in this binary system
        /// </summary>
        public string primary;

        /// <summary>
        /// Name of the secondary planet in this binary system
        /// </summary>
        public string secondary;
    }
}