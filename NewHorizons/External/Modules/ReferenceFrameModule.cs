using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using NewHorizons.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class ReferenceFrameModule
    {
        /// <summary>
        /// Stop the object from being targeted on the map.
        /// </summary>
        public bool hideInMap;

        /// <summary>
        /// Radius of the brackets that show up when you target this. Defaults to the sphereOfInfluence.
        /// </summary>
        [DefaultValue(-1)] public float bracketRadius = -1;

        /// <summary>
        /// If it should be targetable even when super close.
        /// </summary>
        public bool targetWhenClose;

        /// <summary>
        /// The maximum distance that the reference frame can be targeted from. Defaults to double the sphereOfInfluence.
        /// </summary>
        [DefaultValue(-1)] public float maxTargetDistance = -1;
    }
}