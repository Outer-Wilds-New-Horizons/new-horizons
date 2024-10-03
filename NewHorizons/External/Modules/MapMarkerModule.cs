using System.ComponentModel;
using NewHorizons.External.SerializableData;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class MapMarkerModule
    {
        /// <summary>
        /// If the body should have a marker on the map screen.
        /// </summary>
        public bool enabled;

        /// <summary>
        /// Lowest distance away from the body that the marker can be shown. This is automatically set to 0 for all bodies except focal points where it is 5,000.
        /// </summary>
        public float minDisplayDistanceOverride = -1;

        /// <summary>
        /// Highest distance away from the body that the marker can be shown. For planets and focal points the automatic value is 50,000. Moons and planets in focal points are 5,000. Stars are 1E+10 (10,000,000,000).
        /// </summary>
        public float maxDisplayDistanceOverride = -1;
    }
}