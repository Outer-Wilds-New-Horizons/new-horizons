using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using NewHorizons.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class CloakModule
    {
        /// <summary>
        /// Radius of the cloaking field around the planet. It's a bit finicky so experiment with different values. If you
        /// don't want a cloak, leave this as 0.
        /// </summary>
        public float radius;
    }
}