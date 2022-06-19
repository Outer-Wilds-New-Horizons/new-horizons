using System.Collections.Generic;
using System.ComponentModel;
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
        /// Should the player not be able to view the map in this system?
        /// </summary>
        public bool mapRestricted;

        /// <summary>
        /// Customize the skybox for this system
        /// </summary>
        public SkyboxConfig skybox;

        /// <summary>
        /// Set to `true` if you want to spawn here after dying, not Timber Hearth. You can still warp back to the main star
        /// system.
        /// </summary>
        public bool startHere;

        /// <summary>
        /// Name of an existing AudioClip in the game that will play when travelling in space.
        /// </summary>
        public string travelAudioClip;

        /// <summary>
        /// Relative filepath to the .wav file to use as the audio. Mutually exclusive with travelAudioClip.
        /// </summary>
        public string travelAudioFilePath;

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
            public int[] x;
            public int[] y;
            public int[] z;
        }

        [JsonObject]
        public class SkyboxConfig
        {
            /// <summary>
            /// Path to the Unity asset bundle to load the skybox material from
            /// </summary>
            public string assetBundle;

            /// <summary>
            /// Whether to destroy the star field around the player
            /// </summary>
            public bool destroyStarField;

            /// <summary>
            /// Path to the material within the asset bundle specified by `assetBundle` to use for the skybox
            /// </summary>
            public string path;
        }

        public void FixCoordinates()
        {
            if (coords != null)
            {
                coords.x = coords.x.Distinct().ToArray();
                coords.y = coords.y.Distinct().ToArray();
                coords.z = coords.z.Distinct().ToArray();
            }
		}
		
        public void Merge(StarSystemConfig otherConfig)
        {
            // Imagine if this used reflection

            // True by default so if one is false go false
            canEnterViaWarpDrive = canEnterViaWarpDrive && otherConfig.canEnterViaWarpDrive;
            destroyStockPlanets = destroyStockPlanets && otherConfig.destroyStockPlanets;
            enableTimeLoop = enableTimeLoop && otherConfig.enableTimeLoop;

            // If current one is null take the other
            factRequiredForWarp = string.IsNullOrEmpty(factRequiredForWarp) ? otherConfig.factRequiredForWarp : factRequiredForWarp;
            skybox = skybox == null ? otherConfig.skybox : skybox;
            travelAudioClip = string.IsNullOrEmpty(travelAudioClip) ? otherConfig.travelAudioClip : travelAudioClip;
            travelAudioFilePath = string.IsNullOrEmpty(travelAudioFilePath) ? otherConfig.travelAudioFilePath : travelAudioFilePath;

            // False by default so if one is true go true
            mapRestricted = mapRestricted || otherConfig.mapRestricted;
            mapRestricted = mapRestricted || otherConfig.mapRestricted;
            startHere = startHere || otherConfig.startHere;

            entryPositions = Concatenate(entryPositions, otherConfig.entryPositions);
            curiosities = Concatenate(curiosities, otherConfig.curiosities);
            initialReveal = Concatenate(initialReveal, otherConfig.initialReveal);
        }

        private T[] Concatenate<T>(T[] array1, T[] array2)
        {
            return (array1 ?? new T[0]).Concat(array2 ?? new T[0]).ToArray();
        }
    }
}