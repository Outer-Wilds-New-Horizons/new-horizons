using Newtonsoft.Json;
using UnityEngine;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class VariableSizeModule
    {
        /// <summary>
        /// Scale this module over time
        /// </summary>
        public TimeValuePair[] curve;

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
}