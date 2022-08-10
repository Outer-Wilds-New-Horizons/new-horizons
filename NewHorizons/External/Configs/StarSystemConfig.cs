using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NewHorizons.Utility;
using Newtonsoft.Json;
using static NewHorizons.External.Modules.ShipLogModule;

namespace NewHorizons.External.Configs
{
    /// <summary>
    /// Configuration for a specific star system
    /// </summary>
    [JsonObject]
    public class StarSystemConfig
    {
        /// <summary>
        /// Whether this system can be warped to via the warp drive
        /// </summary>
        [DefaultValue(true)] public bool canEnterViaWarpDrive = true;

        /// <summary>
        /// Do you want a clean slate for this star system? Or will it be a modified version of the original.
        /// </summary>
        [DefaultValue(true)] public bool destroyStockPlanets = true;

        /// <summary>
        /// Should the time loop be enabled in this system?
        /// </summary>
        [DefaultValue(true)] public bool enableTimeLoop = true;

        /// <summary>
        /// Set to the FactID that must be revealed before it can be warped to. Don't set `CanEnterViaWarpDrive` to `false` if
        /// you're using this, that would make no sense.
        /// </summary>
        public string factRequiredForWarp;

        /// <summary>
        /// The duration of the time loop in minutes. This is the time the sun explodes. End Times plays 85 seconds before this time, and your memories get sent back about 40 seconds after this time.
        /// </summary>
        [DefaultValue(22f)] public float loopDuration = 22f;

        /// <summary>
        /// Should the player not be able to view the map in this system?
        /// </summary>
        public bool mapRestricted;

        /// <summary>
        /// Customize the skybox for this system
        /// </summary>
        public SkyboxModule Skybox;

        /// <summary>
        /// Set to `true` if you want to spawn here after dying, not Timber Hearth. You can still warp back to the main star
        /// system.
        /// </summary>
        public bool startHere;

        [Obsolete("travelAudioClip is deprecated, please use travelAudio instead")]
        public string travelAudioClip;

        [Obsolete("travelAudioFilePath is deprecated, please use travelAudio instead")]
        public string travelAudioFilePath;

        /// <summary>
        /// The audio that will play when travelling in space. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string travelAudio;

        /// <summary>
        /// Configure warping to this system with the vessel
        /// </summary>
        public VesselModule Vessel;

        [Obsolete("coords is deprecated, please use Vessel.coords instead")]
        public NomaiCoordinates coords;

        [Obsolete("vesselPosition is deprecated, please use Vessel.vesselPosition instead")]
        public MVector3 vesselPosition;

        [Obsolete("vesselRotation is deprecated, please use Vessel.vesselRotation instead")]
        public MVector3 vesselRotation;

        [Obsolete("warpExitPosition is deprecated, please use Vessel.warpExitPosition instead")]
        public MVector3 warpExitPosition;

        [Obsolete("warpExitRotation is deprecated, please use Vessel.warpExitRotation instead")]
        public MVector3 warpExitRotation;

        /// <summary>
        /// Manually layout ship log entries in detective mode
        /// </summary>
        public EntryPositionInfo[] entryPositions;

        /// <summary>
        /// A list of fact IDs to reveal when the game starts.
        /// </summary>
        public string[] initialReveal;

        /// <summary>
        /// List colors of curiosity entries
        /// </summary>
        public CuriosityColorInfo[] curiosities;

        public class NomaiCoordinates
        {
            [MinLength(2)]
            [MaxLength(6)]
            public int[] x;
            
            [MinLength(2)]
            [MaxLength(6)]
            public int[] y;
            
            [MinLength(2)]
            [MaxLength(6)]
            public int[] z;
        }

        [JsonObject]
        public class SkyboxModule
        {

            /// <summary>
            /// Whether to destroy the star field around the player
            /// </summary>
            public bool destroyStarField;

            /// <summary>
            /// Whether to use a cube for the skybox instead of a smooth sphere
            /// </summary>
            public bool useCube;

            /// <summary>
            /// Relative filepath to the texture to use for the skybox's positive X direction
            /// </summary>
            public string rightPath;

            /// <summary>
            /// Relative filepath to the texture to use for the skybox's negative X direction
            /// </summary>
            public string leftPath;

            /// <summary>
            /// Relative filepath to the texture to use for the skybox's positive Y direction
            /// </summary>
            public string topPath;

            /// <summary>
            /// Relative filepath to the texture to use for the skybox's negative Y direction
            /// </summary>
            public string bottomPath;

            /// <summary>
            /// Relative filepath to the texture to use for the skybox's positive Z direction
            /// </summary>
            public string frontPath;

            /// <summary>
            /// Relative filepath to the texture to use for the skybox's negative Z direction
            /// </summary>
            public string backPath;
        }

        [JsonObject]
        public class VesselModule
        {
            /// <summary>
            /// Coordinates that the vessel can use to warp to your solar system.
            /// </summary>
            public NomaiCoordinates coords;

            /// <summary>
            /// The position in the solar system the vessel will warp to.
            /// </summary>
            public MVector3 vesselPosition;

            /// <summary>
            /// Euler angles by which the vessel will be oriented.
            /// </summary>
            public MVector3 vesselRotation;

            /// <summary>
            /// The relative position to the vessel that you will be teleported to when you exit the vessel through the black hole.
            /// </summary>
            public MVector3 warpExitPosition;

            /// <summary>
            /// Euler angles by which the warp exit will be oriented.
            /// </summary>
            public MVector3 warpExitRotation;
        }

        /// <summary>
        /// Makes sure they are all numbers are unique and between 0 and 5.
        /// </summary>
        private static int[] FixAxis(int[] axis) => axis.Distinct().Where(i => (i >= 0 && i <= 5)).ToArray();

        public void FixCoordinates()
        {
            if (Vessel?.coords != null)
            {
                Vessel.coords.x = FixAxis(Vessel.coords.x);
                Vessel.coords.y = FixAxis(Vessel.coords.y);
                Vessel.coords.z = FixAxis(Vessel.coords.z);
            }
        }
        
        public void Merge(StarSystemConfig otherConfig)
        {
            // Imagine if this used reflection

            // True by default so if one is false go false
            canEnterViaWarpDrive = canEnterViaWarpDrive && otherConfig.canEnterViaWarpDrive;
            destroyStockPlanets = destroyStockPlanets && otherConfig.destroyStockPlanets;
            enableTimeLoop = enableTimeLoop && otherConfig.enableTimeLoop;
            loopDuration = loopDuration == 22f ? otherConfig.loopDuration : loopDuration;

            // If current one is null take the other
            factRequiredForWarp = string.IsNullOrEmpty(factRequiredForWarp) ? otherConfig.factRequiredForWarp : factRequiredForWarp;
            Skybox = Skybox == null ? otherConfig.Skybox : Skybox;
            travelAudio = string.IsNullOrEmpty(travelAudio) ? otherConfig.travelAudio : travelAudio;

            // False by default so if one is true go true
            mapRestricted = mapRestricted || otherConfig.mapRestricted;
            mapRestricted = mapRestricted || otherConfig.mapRestricted;
            startHere = startHere || otherConfig.startHere;

            Vessel = Vessel == null ? otherConfig.Vessel : Vessel;

            entryPositions = Concatenate(entryPositions, otherConfig.entryPositions);
            curiosities = Concatenate(curiosities, otherConfig.curiosities);
            initialReveal = Concatenate(initialReveal, otherConfig.initialReveal);
        }

        private T[] Concatenate<T>(T[] array1, T[] array2)
        {
            return (array1 ?? new T[0]).Concat(array2 ?? new T[0]).ToArray();
        }
        
        public void Migrate()
        {
            // Backwards compatability
            // Should be the only place that obsolete things are referenced
#pragma warning disable 612, 618
            if (!string.IsNullOrEmpty(travelAudioClip)) travelAudio = travelAudioClip;
            if (!string.IsNullOrEmpty(travelAudioFilePath)) travelAudio = travelAudioFilePath;
            if (coords != null || vesselPosition != null || vesselRotation != null || warpExitPosition != null || warpExitRotation != null)
            {
                if (Vessel == null)
                {
                    Vessel = new VesselModule();
                }
                Vessel.coords = Vessel.coords ?? coords;
                Vessel.vesselPosition = Vessel.vesselPosition ?? vesselPosition;
                Vessel.vesselRotation = Vessel.vesselRotation ?? vesselRotation;
                Vessel.warpExitPosition = Vessel.warpExitPosition ?? warpExitPosition;
                Vessel.warpExitRotation = Vessel.warpExitRotation ?? warpExitRotation;
            }
        }
    }
}