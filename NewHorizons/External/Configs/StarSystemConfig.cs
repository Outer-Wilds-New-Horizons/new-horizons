using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml;
using NewHorizons.External.Modules;
using NewHorizons.External.SerializableData;
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
        /// In this system should the player be able to rotate their map camera freely or be stuck above the plane of the solar system?
        /// </summary>
        public bool freeMapAngle;

        /// <summary>
        /// When well past the furthest orbit, should the player be summoned back to the star?
        /// </summary>
        public bool returnToSolarSystemWhenTooFar;

        /// <summary>
        /// An override value for the far clip plane. Allows you to see farther.
        /// </summary>
        public float farClipPlaneOverride;

        /// <summary>
        /// Whether this system can be warped to via the warp drive. If you set `factRequiredForWarp`, this will be true.
        /// Does NOT effect the base SolarSystem. For that, see `canExitViaWarpDrive` and `factRequiredToExitViaWarpDrive`
        /// </summary>
        [DefaultValue(true)] public bool canEnterViaWarpDrive = true;

        /// <summary>
        /// The FactID that must be revealed before it can be warped to. Don't set `canEnterViaWarpDrive` to `false` if
        /// you're using this, because it will be overwritten.
        /// </summary>
        public string factRequiredForWarp;

        /// <summary>
        /// Can you use the warp drive to leave this system? If you set `factRequiredToExitViaWarpDrive`
        /// this will be true.
        /// </summary>
        [DefaultValue(true)] public bool canExitViaWarpDrive = true;

        /// <summary>
        /// The FactID that must be revealed for you to warp back to the main solar system from here. Don't set `canWarpHome`
        /// to `false` if you're using this, because it will be overwritten.
        /// </summary>
        public string factRequiredToExitViaWarpDrive;

        /// <summary>
        /// Do you want a clean slate for this star system? Or will it be a modified version of the original.
        /// </summary>
        [DefaultValue(true)] public bool destroyStockPlanets = true;

        /// <summary>
        /// Should the time loop be enabled in this system?
        /// </summary>
        [DefaultValue(true)] public bool enableTimeLoop = true;

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
        /// Set to `true` if you want to spawn here after dying, not Timber Hearth. You can still warp back to the main star system.
        /// </summary>
        public bool startHere;

        /// <summary>
        /// Set to `true` if you want the player to stay in this star system if they die in it.
        /// </summary>
        public bool respawnHere;

        [Obsolete("travelAudioClip is deprecated, please use travelAudio instead")]
        public string travelAudioClip;

        [Obsolete("travelAudioFilePath is deprecated, please use travelAudio instead")]
        public string travelAudioFilePath;

        [Obsolete("travelAudio is deprecated, please use travelAudio instead")]
        public string travelAudio;

        /// <summary>
        /// Replace music that plays globally
        /// </summary>
        public GlobalMusicModule GlobalMusic;

        /// <summary>
        /// Configure warping to this system with the vessel
        /// </summary>
        public VesselModule Vessel;

        [Obsolete("coords is deprecated, please use Vessel.coords instead")]
        public NomaiCoordinates coords;

        [Obsolete("vesselPosition is deprecated, please use Vessel.vesselSpawn.position instead")]
        public MVector3 vesselPosition;

        [Obsolete("vesselRotation is deprecated, please use Vessel.vesselSpawn.rotation instead")]
        public MVector3 vesselRotation;

        [Obsolete("warpExitPosition is deprecated, please use Vessel.warpExit.position instead")]
        public MVector3 warpExitPosition;

        [Obsolete("warpExitRotation is deprecated, please use Vessel.warpExit.rotation instead")]
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
        /// The planet to focus on when entering the ship log for the first time in a loop. If not set this will be the planet at navtigation position (1, 0)
        /// </summary>
        public string shipLogStartingPlanetID;

        /// <summary>
        /// List colors of curiosity entries
        /// </summary>
        public CuriosityColorInfo[] curiosities;

        /// <summary>
        /// Extra data that may be used by extension mods
        /// </summary>
        public object extras;

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
        public class GlobalMusicModule
        {
            /// <summary>
            /// The audio that will play when travelling in space. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
            /// </summary>
            public string travelAudio;

            /// <summary>
            /// The audio that will play right before the loop ends. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
            /// </summary>
            public string endTimesAudio;

            /// <summary>
            /// The audio that will play right before the loop ends while inside the dreamworld. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
            /// </summary>
            public string endTimesDreamAudio;

            /// <summary>
            /// The audio that will play when travelling through a bramble dimension. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
            /// </summary>
            public string brambleDimensionAudio;

            /// <summary>
            /// The audio that will play when you leave the ash twin project after taking out the advanced warp core. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
            /// </summary>
            public string finalEndTimesIntroAudio;

            /// <summary>
            /// The audio that will loop after the final end times intro. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
            /// </summary>
            public string finalEndTimesLoopAudio;

            /// <summary>
            /// The audio that will loop after the final end times intro while inside a bramble dimension. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
            /// </summary>
            public string finalEndTimesBrambleDimensionAudio;
        }

        [JsonObject]
        public class VesselModule
        {
            /// <summary>
            /// Coordinates that the vessel can use to warp to your solar system.
            /// </summary>
            public NomaiCoordinates coords;

            /// <summary>
            /// A ship log fact which will make a prompt appear showing the coordinates when you're in the Vessel.
            /// </summary>
            public string promptFact;

            /// <summary>
            /// Whether the vessel should spawn in this system even if it wasn't used to warp to it. This will automatically power on the vessel.
            /// </summary>
            public bool alwaysPresent;

            /// <summary>
            /// Whether to always spawn the player on the vessel, even if it wasn't used to warp to the system.
            /// </summary>
            public bool spawnOnVessel;

            /// <summary>
            /// Whether the vessel should have physics enabled. Defaults to false if parentBody is set, and true otherwise.
            /// </summary>
            public bool? hasPhysics;

            /// <summary>
            /// Whether the vessel should have a zero-gravity volume around it. Defaults to false if parentBody is set, and true otherwise.
            /// </summary>
            public bool? hasZeroGravityVolume;

            /// <summary>
            /// The location that the vessel will warp to.
            /// </summary>
            public VesselInfo vesselSpawn;

            /// <summary>
            /// The location that you will be teleported to when you exit the vessel through the black hole.
            /// </summary>
            public WarpExitInfo warpExit;

            [Obsolete("vesselPosition is deprecated, use vesselSpawn.position instead")] public MVector3 vesselPosition;
            [Obsolete("vesselRotation is deprecated, use vesselSpawn.rotation instead")] public MVector3 vesselRotation;
            [Obsolete("warpExitPosition is deprecated, use vesselSpawn.position instead")] public MVector3 warpExitPosition;
            [Obsolete("warpExitRotation is deprecated, use vesselSpawn.rotation instead")] public MVector3 warpExitRotation;

            [JsonObject]
            public class VesselInfo : GeneralSolarSystemPropInfo
            {
            }

            [JsonObject]
            public class WarpExitInfo : GeneralSolarSystemPropInfo
            {
                /// <summary>
                /// If set, keeps the warp exit attached to the vessel. Overrides `parentPath`.
                /// </summary>
                public bool attachToVessel;
            }
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

            // False by default so if one is true go true
            mapRestricted = mapRestricted || otherConfig.mapRestricted;
            respawnHere = respawnHere || otherConfig.respawnHere;
            startHere = startHere || otherConfig.startHere;

            if (Vessel != null && otherConfig.Vessel != null)
            {
                Vessel.spawnOnVessel = Vessel.spawnOnVessel || otherConfig.Vessel.spawnOnVessel;
                Vessel.alwaysPresent = Vessel.alwaysPresent || otherConfig.Vessel.alwaysPresent;
                Vessel.hasPhysics = Vessel.hasPhysics ?? otherConfig.Vessel.hasPhysics;
                Vessel.hasZeroGravityVolume = Vessel.hasZeroGravityVolume ?? otherConfig.Vessel.hasZeroGravityVolume;
            }
            else
            {
                Vessel ??= otherConfig.Vessel;
            }

            if (GlobalMusic != null && otherConfig.GlobalMusic != null)
            {
                GlobalMusic.travelAudio = string.IsNullOrEmpty(GlobalMusic.travelAudio) ? otherConfig.GlobalMusic.travelAudio : GlobalMusic.travelAudio;
                GlobalMusic.endTimesAudio = string.IsNullOrEmpty(GlobalMusic.endTimesAudio) ? otherConfig.GlobalMusic.endTimesAudio : GlobalMusic.endTimesAudio;
                GlobalMusic.endTimesDreamAudio = string.IsNullOrEmpty(GlobalMusic.endTimesDreamAudio) ? otherConfig.GlobalMusic.endTimesDreamAudio : GlobalMusic.endTimesDreamAudio;
                GlobalMusic.brambleDimensionAudio = string.IsNullOrEmpty(GlobalMusic.brambleDimensionAudio) ? otherConfig.GlobalMusic.brambleDimensionAudio : GlobalMusic.brambleDimensionAudio;
                GlobalMusic.finalEndTimesIntroAudio = string.IsNullOrEmpty(GlobalMusic.finalEndTimesIntroAudio) ? otherConfig.GlobalMusic.finalEndTimesIntroAudio : GlobalMusic.finalEndTimesIntroAudio;
                GlobalMusic.finalEndTimesLoopAudio = string.IsNullOrEmpty(GlobalMusic.finalEndTimesLoopAudio) ? otherConfig.GlobalMusic.finalEndTimesLoopAudio : GlobalMusic.finalEndTimesLoopAudio;
                GlobalMusic.finalEndTimesBrambleDimensionAudio = string.IsNullOrEmpty(GlobalMusic.finalEndTimesBrambleDimensionAudio) ? otherConfig.GlobalMusic.finalEndTimesBrambleDimensionAudio : GlobalMusic.finalEndTimesBrambleDimensionAudio;
            }
            else
            {
                GlobalMusic ??= otherConfig.GlobalMusic;
            }

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
            // Backwards compatibility
            // Should be the only place that obsolete things are referenced
#pragma warning disable 612, 618
            if (!string.IsNullOrEmpty(travelAudioClip)) travelAudio = travelAudioClip;
            if (!string.IsNullOrEmpty(travelAudioFilePath)) travelAudio = travelAudioFilePath;
            if (!string.IsNullOrEmpty(travelAudio))
            {
                if (GlobalMusic == null) GlobalMusic = new GlobalMusicModule();
                if (string.IsNullOrEmpty(GlobalMusic.travelAudio)) GlobalMusic.travelAudio = travelAudio;
            }
            if (coords != null || vesselPosition != null || vesselRotation != null || warpExitPosition != null || warpExitRotation != null)
            {
                if (Vessel == null)
                {
                    Vessel = new VesselModule();
                }
                Vessel.coords ??= coords;
                Vessel.vesselPosition ??= vesselPosition;
                Vessel.vesselRotation ??= vesselRotation;
                Vessel.warpExitPosition ??= warpExitPosition;
                Vessel.warpExitRotation ??= warpExitRotation;
            }
            if (Vessel != null)
            {
                if (Vessel.vesselPosition != null || Vessel.vesselRotation != null)
                {
                    if (Vessel.vesselSpawn == null)
                    {
                        Vessel.vesselSpawn = new VesselModule.VesselInfo();
                    }
                    Vessel.vesselSpawn.position ??= Vessel.vesselPosition;
                    Vessel.vesselSpawn.rotation ??= Vessel.vesselRotation;
                }
                if (Vessel.warpExitPosition != null || Vessel.warpExitRotation != null)
                {
                    if (Vessel.warpExit == null)
                    {
                        Vessel.warpExit = new VesselModule.WarpExitInfo();
                    }
                    Vessel.warpExit.position ??= Vessel.warpExitPosition;
                    Vessel.warpExit.rotation ??= Vessel.warpExitRotation;
                    Vessel.warpExit.attachToVessel = true;
                }
            }
            if (!string.IsNullOrEmpty(factRequiredToExitViaWarpDrive))
            {
                canExitViaWarpDrive = true;
            }
        }
    }
}