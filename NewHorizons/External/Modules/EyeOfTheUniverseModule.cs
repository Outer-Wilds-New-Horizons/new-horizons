using NewHorizons.External.Modules.Props.EyeOfTheUniverse;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class EyeOfTheUniverseModule
    {
        /// <summary>
        /// Add custom travelers to the campfire sequence
        /// </summary>
        public EyeTravelerInfo[] eyeTravelers;

        /// <summary>
        /// Add instrument zones which contain puzzles to gather a quantum instrument. You can parent other props to these with `parentPath` 
        /// </summary>
        public InstrumentZoneInfo[] instrumentZones;

        /// <summary>
        /// Add quantum instruments which cause their associated eye traveler to appear and instrument zones to disappear
        /// </summary>
        public QuantumInstrumentInfo[] quantumInstruments;
    }
}
