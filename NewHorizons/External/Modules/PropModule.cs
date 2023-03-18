using Newtonsoft.Json;
using System;
using NewHorizons.External.Props;
using NewHorizons.External.Volumes;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class PropModule
    {
        /// <summary>
        /// Place props in predefined positions on the planet
        /// </summary>
        public DetailInfo[] details;

        /// <summary>
        /// Add dialogue triggers to this planet
        /// </summary>
        public DialogueInfo[] dialogue;

        /// <summary>
        /// Add ship log entry locations on this planet
        /// </summary>
        public EntryLocationInfo[] entryLocation;

        /// <summary>
        /// Add Geysers to this planet
        /// </summary>
        public GeyserInfo[] geysers;

        /// <summary>
        /// Add translatable text to this planet. (LEGACY - for use with pre-autospirals configs)
        /// </summary>
        [Obsolete("nomaiText is deprecated as of the release of auto spirals, instead please use translatorText with new configs.")]
        public NomaiTextInfo[] nomaiText;

        /// <summary>
        /// Add translatable text to this planet
        /// </summary>
        public NomaiTextInfo[] translatorText;

        /// <summary>
        /// Details which will be shown from 50km away. Meant to be lower resolution.
        /// </summary>
        public DetailInfo[] proxyDetails;

        /// <summary>
        /// Add rafts to this planet
        /// </summary>
        public RaftInfo[] rafts;

        /// <summary>
        /// Scatter props around this planet's surface
        /// </summary>
        public ScatterInfo[] scatter;

        /// <summary>
        /// Add slideshows (from the DLC) to the planet
        /// </summary>
        public ProjectionInfo[] slideShows;

        /// <summary>
        /// A list of quantum groups that props can be added to. An example of a group would be a list of possible locations for a QuantumSocketedObject.
        /// </summary>
        public QuantumGroupInfo[] quantumGroups;

        /// <summary>
        /// Add tornadoes to this planet
        /// </summary>
        public TornadoInfo[] tornados;

        /// <summary>
        /// Add volcanoes to this planet
        /// </summary>
        public VolcanoInfo[] volcanoes;

        /// <summary>
        /// Add black/white-holes to this planet
        /// </summary>
        public SingularityInfo[] singularities;

        /// <summary>
        /// Add signalscope signals to this planet
        /// </summary>
        public SignalInfo[] signals;

        /// <summary>
        /// Add projection pools/platforms, whiteboards, and stones to this planet
        /// </summary>
        public RemoteInfo[] remotes;

        [Obsolete("reveal is deprecated. Use Volumes->revealVolumes instead.")] public RevealVolumeInfo[] reveal;

        [Obsolete("audioVolumes is deprecated. Use Volumes->audioVolumes instead.")] public AudioVolumeInfo[] audioVolumes;
    }
}
