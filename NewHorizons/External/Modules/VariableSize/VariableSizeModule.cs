using NewHorizons.Utility;
using Newtonsoft.Json;
using UnityEngine;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class VariableSizeModule
    {
        /// <summary>
        /// Scale this object over time
        /// </summary>
        public TimeValuePair[] curve;
    }
}