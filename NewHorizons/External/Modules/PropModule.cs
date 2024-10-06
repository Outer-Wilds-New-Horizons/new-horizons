using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.Audio;
using NewHorizons.External.Modules.Props.Dialogue;
using NewHorizons.External.Modules.Props.EchoesOfTheEye;
using NewHorizons.External.Modules.Props.Quantum;
using NewHorizons.External.Modules.Props.Remote;
using NewHorizons.External.Modules.Props.Shuttle;
using NewHorizons.External.Modules.TranslatorText;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.External.Modules.WarpPad;
using Newtonsoft.Json;
using System;

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
        public TranslatorTextInfo[] translatorText;

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
        public SingularityModule[] singularities;

        /// <summary>
        /// Add signalscope signals to this planet
        /// </summary>
        public SignalInfo[] signals;

        /// <summary>
        /// Add projection pools/platforms, whiteboards, and stones to this planet
        /// </summary>
        public RemoteInfo[] remotes;

        /// <summary>
        /// Add warp pad receivers to this planet. These are the warp pads you are sent to from Ash Twin.
        /// </summary>
        public NomaiWarpReceiverInfo[] warpReceivers;

        /// <summary>
        /// Add warp pad transmitters to this planet. These are the warp pads seen on the Ash Twin.
        /// </summary>
        public NomaiWarpTransmitterInfo[] warpTransmitters;

        /// <summary>
        /// Add audio point sources to this planet. For audio across an entire area, look for AudioVolumes under the Volumes module.
        /// </summary>
        public AudioSourceInfo[] audioSources;

        /// <summary>
        /// Add a gravity cannon to this planet. Must be paired to a new shuttle, which can be placed on this planet or elsewhere.
        /// </summary>
        public GravityCannonInfo[] gravityCannons;

        /// <summary>
        /// Add a Nomai shuttle to this planet. Can be paired to a gravity cannon on this planet or elsewhere.
        /// </summary>
        public ShuttleInfo[] shuttles;

        /// <summary>
        /// Add campfires that allow you to enter the dream world/simulation. Must be paired with a dream arrival point, which can be placed on this planet or elsewhere.
        /// </summary>
        public DreamCampfireInfo[] dreamCampfires;
        
        /// <summary>
        /// Add the points you will arrive at when entering the dream world/simulation from a paired dream campfire, which can be placed on this planet or elsewhere. The planet with the arrival point should be statically positioned to avoid issues with the simulation view materials.
        /// </summary>
        public DreamArrivalPointInfo[] dreamArrivalPoints;

        /// <summary>
        /// Adds dream world grapple totems to this planet.
        /// </summary>
        public GrappleTotemInfo[] grappleTotems;

        [Obsolete("reveal is deprecated. Use Volumes->revealVolumes instead.")] public RevealVolumeInfo[] reveal;

        [Obsolete("audioVolumes is deprecated. Use Volumes->audioVolumes instead.")] public AudioVolumeInfo[] audioVolumes;
    }
}
