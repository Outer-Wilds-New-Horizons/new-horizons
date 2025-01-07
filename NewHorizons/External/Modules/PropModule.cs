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
        /// Add rafts to this planet (requires Echoes of the Eye DLC)
        /// </summary>
        public RaftInfo[] rafts;

        /// <summary>
        /// Scatter props around this planet's surface
        /// </summary>
        public ScatterInfo[] scatter;

        /// <summary>
        /// Add slideshows to the planet (requires Echoes of the Eye DLC)
        /// </summary>
        public ProjectionInfo[] slideShows;

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
        /// Add a socket quantum object to a planet. Define the position of multiple "sockets" and multiple objects that jump between sockets. 
        /// If the number of sockets equals the number of objects, they will shuffle around.
        /// </summary>
        public SocketQuantumGroupInfo[] socketQuantumGroups;

        /// <summary>
        /// Add a state quantum object to a planet. Switches between displaying different objects in a single place.
        /// </summary>
        public StateQuantumGroupInfo[] stateQuantumGroups;

        /// <summary>
        /// Add quantum lightning to a planet. When lightning strikes, a different detail object is shown. The lightning will take the first defined position/rotation for all objects.
        /// </summary>
        public LightningQuantumGroupInfo[] lightningQuantumGroups;

        /// <summary>
        /// Add campfires that allow you to enter the dream world/simulation (requires Echoes of the Eye DLC). Must be paired with a dream arrival point, which can be placed on this planet or elsewhere.
        /// </summary>
        public DreamCampfireInfo[] dreamCampfires;

        /// <summary>
        /// Add the points you will arrive at when entering the dream world/simulation from a paired dream campfire (requires Echoes of the Eye DLC). The planet with the arrival point should be statically positioned to avoid issues with the simulation view materials.
        /// </summary>
        public DreamArrivalPointInfo[] dreamArrivalPoints;

        /// <summary>
        /// Adds dream world grapple totems to this planet (requires Echoes of the Eye DLC).
        /// </summary>
        public GrappleTotemInfo[] grappleTotems;

        /// <summary>
        /// Adds dream world alarm totems to this planet (requires Echoes of the Eye DLC).
        /// </summary>
        public AlarmTotemInfo[] alarmTotems;

        /// <summary>
        /// Adds portholes (the windows you can peek through in the Stranger) to this planet (requires Echoes of the Eye DLC).
        /// </summary>
        public PortholeInfo[] portholes;

        /// <summary>
        /// Adds dream world candles to this planet (requires Echoes of the Eye DLC).
        /// </summary>
        public DreamCandleInfo[] dreamCandles;

        /// <summary>
        /// Adds dream world projection totems (requires Echoes of the Eye DLC).
        /// </summary>
        public ProjectionTotemInfo[] projectionTotems;

        [Obsolete("reveal is deprecated. Use Volumes->revealVolumes instead.")] public RevealVolumeInfo[] reveal;

        [Obsolete("audioVolumes is deprecated. Use Volumes->audioVolumes instead.")] public AudioVolumeInfo[] audioVolumes;

        [Obsolete("quantumGroups is deprecated. Use stateQuantumGroups or socketQuantumGroups instead.")] public QuantumGroupInfo[] quantumGroups;
    }
}
