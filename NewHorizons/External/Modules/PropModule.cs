using NewHorizons.External.Modules.VariableSize;
using NewHorizons.External.Modules.WarpPad;
using NewHorizons.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

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
        public SignalModule.SignalInfo[] signals;

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

        [Obsolete("reveal is deprecated. Use Volumes->revealVolumes instead.")] public VolumesModule.RevealVolumeInfo[] reveal;

        [Obsolete("audioVolumes is deprecated. Use Volumes->audioVolumes instead.")] public VolumesModule.AudioVolumeInfo[] audioVolumes;

        [JsonObject]
        public class ScatterInfo
        {
            /// <summary>
            /// Relative filepath to an asset-bundle
            /// </summary>
            public string assetBundle;

            /// <summary>
            /// Number of props to scatter
            /// </summary>
            public int count;

            /// <summary>
            /// Offset this prop once it is placed
            /// </summary>
            public MVector3 offset;

            /// <summary>
            /// Either the path in the scene hierarchy of the item to copy or the path to the object in the supplied asset bundle
            /// </summary>
            public string path;

            /// <summary>
            /// Rotate this prop once it is placed
            /// </summary>
            public MVector3 rotation;

            /// <summary>
            /// Scale this prop once it is placed
            /// </summary>
            [DefaultValue(1f)] public float scale = 1f;

            /// <summary>
            /// Scale each axis of the prop. Overrides `scale`.
            /// </summary>
            public MVector3 stretch;

            /// <summary>
            /// The number used as entropy for scattering the props
            /// </summary>
            public int seed;

            /// <summary>
            /// The lowest height that these object will be placed at (only relevant if there's a heightmap)
            /// </summary>
            public float? minHeight;

            /// <summary>
            /// The highest height that these objects will be placed at (only relevant if there's a heightmap)
            /// </summary>
            public float? maxHeight;

            /// <summary>
            /// Should we try to prevent overlap between the scattered details? True by default. If it's affecting load times turn it off.
            /// </summary>
            [DefaultValue(true)] public bool preventOverlap = true;
            
            /// <summary>
            /// Should this detail stay loaded even if you're outside the sector (good for very large props)
            /// </summary>
            public bool keepLoaded;
        }

        [JsonObject]
        public class DetailInfo : GeneralPropInfo
        {
            public DetailInfo() { }

            public DetailInfo(GeneralPointPropInfo info)
            {
                JsonConvert.PopulateObject(JsonConvert.SerializeObject(info), this);
            }

            /// <summary>
            /// Relative filepath to an asset-bundle to load the prefab defined in `path` from
            /// </summary>
            public string assetBundle;

            /// <summary>
            /// Either the path in the scene hierarchy of the item to copy or the path to the object in the supplied asset bundle
            /// </summary>
            public string path;

            /// <summary>
            /// A list of children to remove from this detail
            /// </summary>
            public string[] removeChildren;

            /// <summary>
            /// Do we reset all the components on this object? Useful for certain props that have dialogue components attached to
            /// them.
            /// </summary>
            public bool removeComponents;

            /// <summary>
            /// Scale the prop
            /// </summary>
            [DefaultValue(1f)] public float scale = 1f;

            /// <summary>
            /// Scale each axis of the prop. Overrides `scale`.
            /// </summary>
            public MVector3 stretch;

            /// <summary>
            /// If this value is not null, this prop will be quantum. Assign this field to the id of the quantum group it should be a part of. The group it is assigned to determines what kind of quantum object it is
            /// </summary>
            public string quantumGroupID;

            /// <summary>
            /// Should this detail stay loaded even if you're outside the sector (good for very large props)
            /// </summary>
            public bool keepLoaded;

            /// <summary>
            /// Should this object dynamically move around?
            /// This tries to make all mesh colliders convex, as well as adding a sphere collider in case the detail has no others.
            /// </summary>
            public bool hasPhysics;

            /// <summary>
            /// The mass of the physics object.
            /// Most pushable props use the default value, which matches the player mass.
            /// </summary>
            [DefaultValue(0.001f)] public float physicsMass = 0.001f;

            /// <summary>
            /// The radius that the added sphere collider will use for physics collision.
            /// If there's already good colliders on the detail, you can make this 0.
            /// </summary>
            [DefaultValue(1f)] public float physicsRadius = 1f;

            [Obsolete("alignToNormal is deprecated. Use alignRadial instead")] public bool alignToNormal;
        }

        [JsonObject]
        public class RaftInfo : GeneralPropInfo
        {
            /// <summary>
            /// Acceleration of the raft. Default acceleration is 5.
            /// </summary>
            [DefaultValue(5f)] public float acceleration = 5f;
        }

        [JsonObject]
        public class GeyserInfo : GeneralPropInfo
        {
            /// <summary>
            /// Vertical offset of the geyser. From 0, the bubbles start at a height of 10, the shaft at 67, and the spout at 97.5.
            /// </summary>
            [DefaultValue(-97.5f)] public float offset = -97.5f;

            /// <summary>
            /// Force of the geyser on objects
            /// </summary>
            [DefaultValue(55f)] public float force = 55f;

            /// <summary>
            /// Time in seconds eruptions last for
            /// </summary>
            [DefaultValue(10f)] public float activeDuration = 10f;

            /// <summary>
            /// Time in seconds between eruptions
            /// </summary>
            [DefaultValue(19f)] public float inactiveDuration = 19f;

            /// <summary>
            /// Color of the geyser. Alpha sets the particle density.
            /// </summary>
            public MColor tint;

            /// <summary>
            /// Disable the individual particle systems of the geyser
            /// </summary>
            public bool disableBubbles, disableShaft, disableSpout;

            /// <summary>
            /// Loudness of the geyser
            /// </summary>
            [DefaultValue(0.7f)] public float volume = 0.7f;
        }

        [JsonObject]
        public class TornadoInfo : GeneralPropInfo
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum TornadoType
            {
                [EnumMember(Value = @"upwards")] Upwards = 0,

                [EnumMember(Value = @"downwards")] Downwards = 1,

                [EnumMember(Value = @"hurricane")] Hurricane = 2
            }

            [Obsolete("Downwards is deprecated. Use Type instead.")] public bool downwards;

            /// <summary>
            /// Alternative to setting the position. Will choose a random place at this elevation.
            /// </summary>
            public float elevation;

            /// <summary>
            /// The height of this tornado.
            /// </summary>
            [DefaultValue(30f)] public float height = 30f;

            /// <summary>
            /// The colour of the tornado.
            /// </summary>
            public MColor tint;

            /// <summary>
            /// What type of cyclone should this be? Upwards and downwards are both tornados and will push in that direction.
            /// </summary>
            [DefaultValue("upwards")] public TornadoType type = TornadoType.Upwards;

            /// <summary>
            /// Angular distance from the starting position that it will wander, in terms of the angle around the x-axis.
            /// </summary>
            [DefaultValue(45f)] public float wanderDegreesX = 45f;

            /// <summary>
            /// Angular distance from the starting position that it will wander, in terms of the angle around the z-axis.
            /// </summary>
            [DefaultValue(45f)] public float wanderDegreesZ = 45f;

            /// <summary>
            /// The rate at which the tornado will wander around the planet. Set to 0 for it to be stationary. Should be around
            /// 0.1.
            /// </summary>
            public float wanderRate;

            /// <summary>
            /// The maximum distance at which you'll hear the sounds of the cyclone. If not set it will scale relative to the size of the cyclone.
            /// </summary>
            public float audioDistance;

            /// <summary>
            /// Fluid type for sounds/effects when colliding with this tornado.
            /// </summary>
            [DefaultValue("cloud")] public FluidType fluidType = FluidType.Cloud;
        }

        [JsonObject]
        public class VolcanoInfo : GeneralPropInfo
        {
            /// <summary>
            /// The colour of the meteor's lava.
            /// </summary>
            public MColor lavaTint;

            /// <summary>
            /// Maximum time between meteor launches.
            /// </summary>
            [DefaultValue(20f)]
            public float maxInterval = 20f;

            /// <summary>
            /// Maximum random speed at which meteors are launched.
            /// </summary>
            [DefaultValue(150f)]
            public float maxLaunchSpeed = 150f;

            /// <summary>
            /// Minimum time between meteor launches.
            /// </summary>
            [DefaultValue(5f)]
            public float minInterval = 5f;

            /// <summary>
            /// Minimum random speed at which meteors are launched.
            /// </summary>
            [DefaultValue(50f)]
            public float minLaunchSpeed = 50f;

            /// <summary>
            /// Scale of the meteors.
            /// </summary>
            public float scale = 1;

            /// <summary>
            /// The colour of the meteor's stone.
            /// </summary>
            public MColor stoneTint;
        }

        [JsonObject]
        public class DialogueInfo : GeneralPointPropInfo
        {
            /// <summary>
            /// Prevents the dialogue from being created after a specific persistent condition is set. Useful for remote dialogue
            /// triggers that you want to have happen only once.
            /// </summary>
            public string blockAfterPersistentCondition;

            /// <summary>
            /// If a pathToAnimController is supplied, if you are within this distance the character will look at you. If it is set
            /// to 0, they will only look at you when spoken to.
            /// </summary>
            public float lookAtRadius;

            /// <summary>
            /// If this dialogue is meant for a character, this is the relative path from the planet to that character's
            /// CharacterAnimController, TravelerController, TravelerEyeController (eye of the universe), FacePlayerWhenTalking, or SolanumAnimController.
            /// 
            /// If none of those components are present it will add a FacePlayerWhenTalking component.
            /// </summary>
            public string pathToAnimController;

            /// <summary>
            /// Radius of the spherical collision volume where you get the "talk to" prompt when looking at. If you use a
            /// remoteTrigger, you can set this to 0 to make the dialogue only trigger remotely.
            /// </summary>
            public float radius = 1f;

            /// <summary>
            /// Distance from radius the prompt appears
            /// </summary>
            [DefaultValue(2f)] public float range = 2f;

            /// <summary>
            /// Allows you to trigger dialogue from a distance when you walk into an area.
            /// </summary>
            public RemoteTriggerInfo remoteTrigger;

            [Obsolete("remoteTriggerPosition is deprecated. Use remoteTrigger.position instead")] public MVector3 remoteTriggerPosition;
            [Obsolete("remoteTriggerRadius is deprecated. Use remoteTrigger.radius instead")] public float remoteTriggerRadius;
            [Obsolete("remoteTriggerPrereqCondition is deprecated. Use remoteTrigger.prereqCondition instead")] public string remoteTriggerPrereqCondition;

            /// <summary>
            /// Relative path to the xml file defining the dialogue.
            /// </summary>
            public string xmlFile;

            /// <summary>
            /// What type of flashlight toggle to do when dialogue is interacted with
            /// </summary>
            [DefaultValue("none")] public FlashlightToggle flashlightToggle = FlashlightToggle.None;

            [JsonConverter(typeof(StringEnumConverter))]
            public enum FlashlightToggle
            {
                [EnumMember(Value = @"none")] None = -1,
                [EnumMember(Value = @"turnOff")] TurnOff = 0,
                [EnumMember(Value = @"turnOffThenOn")] TurnOffThenOn = 1,
            }

            [JsonObject]
            public class RemoteTriggerInfo : GeneralPointPropInfo
            {
                /// <summary>
                /// The radius of the remote trigger volume.
                /// </summary>
                public float radius;
                /// <summary>
                /// This condition must be met for the remote trigger volume to trigger.
                /// </summary>
                public string prereqCondition;
            }
        }

        [JsonObject]
        public class EntryLocationInfo : GeneralPointPropInfo
        {
            /// <summary>
            /// Whether this location is cloaked
            /// </summary>
            public bool cloaked;

            /// <summary>
            /// ID of the entry this location relates to
            /// </summary>
            public string id;
        }

        [JsonObject]
        public class TranslatorTextInfo : GeneralPropInfo
        {
            /// <summary>
            /// Additional information about each arc in the text
            /// </summary>
            public NomaiTextArcInfo[] arcInfo;

            /// <summary>
            /// The random seed used to pick what the text arcs will look like.
            /// </summary>
            public int seed;

            /// <summary>
            /// Only for wall text. Aligns wall text to face towards the given direction, with 'up' oriented relative to its current rotation or alignment.
            /// </summary>
            public MVector3 normal;

            /// <summary>
            /// The type of object this is.
            /// </summary>
            [DefaultValue("wall")] public NomaiTextType type = NomaiTextType.Wall;

            /// <summary>
            /// The location of this object. Arcs will be blue if their locations match the wall, else orange.
            /// </summary>
            [DefaultValue("unspecified")] public NomaiTextLocation location = NomaiTextLocation.UNSPECIFIED;

            /// <summary>
            /// The relative path to the xml file for this object.
            /// </summary>
            public string xmlFile;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum NomaiTextType
        {
            [EnumMember(Value = @"wall")] Wall = 0,

            [EnumMember(Value = @"scroll")] Scroll = 1,

            [EnumMember(Value = @"computer")] Computer = 2,

            [EnumMember(Value = @"cairn")] Cairn = 3,

            [EnumMember(Value = @"recorder")] Recorder = 4,

            [EnumMember(Value = @"preCrashRecorder")] PreCrashRecorder = 5,

            [EnumMember(Value = @"preCrashComputer")] PreCrashComputer = 6,

            [EnumMember(Value = @"trailmarker")] Trailmarker = 7,

            [EnumMember(Value = @"cairnVariant")] CairnVariant = 8,

            [EnumMember(Value = @"whiteboard")] Whiteboard = 9,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum NomaiTextLocation
        {
            [EnumMember(Value = @"unspecified")] UNSPECIFIED = 0,

            [EnumMember(Value = @"a")] A = 1,

            [EnumMember(Value = @"b")] B = 2
        }

        [JsonObject]
        public class NomaiTextInfo : GeneralPointPropInfo
        {

            /// <summary>
            /// Additional information about each arc in the text
            /// </summary>
            public NomaiTextArcInfo[] arcInfo;
            
            /// <summary>
            /// The normal vector for this object. Used for writing on walls and positioning computers.
            /// </summary>
            public MVector3 normal;

            /// <summary>
            /// The euler angle rotation of this object. Not required if setting the normal. Computers and cairns will orient
            /// themselves to the surface of the planet automatically.
            /// </summary>
            public MVector3 rotation;

            /// <summary>
            /// The random seed used to pick what the text arcs will look like.
            /// </summary>
            public int seed; // For randomizing arcs

            /// <summary>
            /// The type of object this is.
            /// </summary>
            [DefaultValue("wall")] public NomaiTextType type = NomaiTextType.Wall;

            /// <summary>
            /// The location of this object. Arcs will be blue if their locations match the wall, else orange.
            /// </summary>
            [DefaultValue("unspecified")] public NomaiTextLocation location = NomaiTextLocation.UNSPECIFIED;

            /// <summary>
            /// The relative path to the xml file for this object.
            /// </summary>
            public string xmlFile;
        }

        [JsonObject]
        public class NomaiTextArcInfo
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum NomaiTextArcType
            {
                [EnumMember(Value = @"adult")] Adult = 0,

                [EnumMember(Value = @"child")] Child = 1,

                [EnumMember(Value = @"stranger")] Stranger = 2
            }
            
            /// <summary>
            /// Whether to flip the spiral from left-curling to right-curling or vice versa. If not specified, will use auto spiral generated value.
            /// </summary>
            public bool? mirror;

            /// <summary>
            /// The local position of this object on the wall. If not specified, will use auto spiral generated value.
            /// </summary>
            public MVector2 position;

            /// <summary>
            /// The type of text to display.
            /// </summary>
            [DefaultValue("adult")] public NomaiTextArcType type = NomaiTextArcType.Adult;

            /// <summary>
            /// Which variation of the chosen type to place. If not specified, a random variation will be selected based on the seed provided in the parent module.
            /// </summary>
            [Obsolete("only used in old nomai text")]
            [DefaultValue(-1)] public int variation = -1;

            /// <summary>
            /// The z euler angle for this arc. If not specified, will use auto spiral generated value.
            /// </summary>
            [Range(0f, 360f)] public float? zRotation;
        }

        [JsonObject]
        public class ProjectionInfo : GeneralPropInfo
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum SlideShowType
            {
                [EnumMember(Value = @"slideReel")] SlideReel = 0,

                [EnumMember(Value = @"autoProjector")] AutoProjector = 1,

                [EnumMember(Value = @"visionTorchTarget")] VisionTorchTarget = 2,

                [EnumMember(Value = @"standingVisionTorch")] StandingVisionTorch = 3,

            }

            /// <summary>
            /// The ship log facts revealed after finishing this slide reel.
            /// </summary>
            public string[] reveals;

            /// <summary>
            /// The ship log facts that make the reel play when they are displayed in the computer (by selecting entries or arrows).
            /// You should probably include facts from `reveals` here.
            /// If you only specify a rumor fact, then it would only play in its ship log entry if this has revealed only
            /// rumor facts because an entry with revealed explore facts doesn't display rumor facts.
            /// </summary>
            public string[] playWithShipLogFacts;

            /// <summary>
            /// The list of slides for this object.
            /// </summary>
            public SlideInfo[] slides;

            /// <summary>
            /// The type of object this is.
            /// </summary>
            [DefaultValue("slideReel")] public SlideShowType type = SlideShowType.SlideReel;
        }

        [JsonObject]
        public class SlideInfo
        {
            /// <summary>
            /// Ambient light colour when viewing this slide.
            /// </summary>
            public MColor ambientLightColor;


            // SlideAmbientLightModule

            /// <summary>
            /// Ambient light intensity when viewing this slide.
            /// </summary>
            public float ambientLightIntensity;

            /// <summary>
            /// Ambient light range when viewing this slide.
            /// </summary>
            public float ambientLightRange;

            // SlideBackdropAudioModule

            /// <summary>
            /// The name of the AudioClip that will continuously play while watching these slides
            /// </summary>
            public string backdropAudio;

            /// <summary>
            /// The time to fade into the backdrop audio
            /// </summary>
            public float backdropFadeTime;

            // SlideBeatAudioModule

            /// <summary>
            /// The name of the AudioClip for a one-shot sound when opening the slide.
            /// </summary>
            public string beatAudio;

            /// <summary>
            /// The time delay until the one-shot audio
            /// </summary>
            public float beatDelay;


            // SlideBlackFrameModule

            /// <summary>
            /// Before viewing this slide, there will be a black frame for this many seconds.
            /// </summary>
            public float blackFrameDuration;

            /// <summary>
            /// The path to the image file for this slide.
            /// </summary>
            public string imagePath;


            // SlidePlayTimeModule

            /// <summary>
            /// Play-time duration for auto-projector slides.
            /// </summary>
            public float playTimeDuration;


            // SlideShipLogEntryModule

            /// <summary>
            /// Ship log fact revealed when viewing this slide
            /// </summary>
            public string reveal;

            /// <summary>
            /// Spotlight intensity modifier when viewing this slide.
            /// </summary>
            public float spotIntensityMod;
        }




        [JsonConverter(typeof(StringEnumConverter))]
        public enum QuantumGroupType
        {
            [EnumMember(Value = @"sockets")] Sockets = 0,

            [EnumMember(Value = @"states")] States = 1,

            FailedValidation = 10
        }

        [JsonObject]
        public class QuantumGroupInfo
        {
            /// <summary>
            /// What type of group this is: does it define a list of states a single quantum object could take or a list of sockets one or more quantum objects could share?
            /// </summary>
            public QuantumGroupType type;

            /// <summary>
            /// A unique string used by props (that are marked as quantum) use to refer back to this group
            /// </summary>
            public string id;

            /// <summary>
            /// Only required if type is `sockets`. This lists all the possible locations for any props assigned to this group.
            /// </summary>
            public QuantumSocketInfo[] sockets;

            /// <summary>
            /// Optional. Only used if type is `states`. If this is true, then the first prop made part of this group will be used to construct a visibility box for an empty game object, which will be considered one of the states.
            /// </summary>
            public bool hasEmptyState;

            /// <summary>
            /// Optional. Only used if type is `states`. If this is true, then the states will be presented in order, rather than in a random order
            /// </summary>
            public bool sequential;

            /// <summary>
            /// Optional. Only used if type is `states` and `sequential` is true. If this is false, then after the last state has appeared, the object will no longer change state
            /// </summary>
            [DefaultValue(true)] public bool loop = true;
        }

        [JsonObject]
        public class QuantumSocketInfo : GeneralPropInfo
        {
            /// <summary>
            /// The probability any props that are part of this group will occupy this socket
            /// </summary>
            [DefaultValue(1f)] public float probability = 1f;
        }

        [JsonObject]
        public class RemoteInfo
        {
            /// <summary>
            /// The unique remote id
            /// </summary>
            public string id;

            /// <summary>
            /// Icon that the will show on the stone, pedastal of the whiteboard, and pedastal of the platform.
            /// </summary>
            public string decalPath;

            /// <summary>
            /// Whiteboard that the stones can put text onto
            /// </summary>
            public WhiteboardInfo whiteboard;

            /// <summary>
            /// Camera platform that the stones can project to and from
            /// </summary>
            public PlatformInfo platform;

            /// <summary>
            /// Projection stones
            /// </summary>
            public StoneInfo[] stones;

            [JsonObject]
            public class WhiteboardInfo : GeneralPropInfo
            {
                /// <summary>
                /// The text for each stone
                /// </summary>
                public SharedNomaiTextInfo[] nomaiText;

                /// <summary>
                /// Disable the wall, leaving only the pedestal and text.
                /// </summary>
                public bool disableWall;

                [JsonObject]
                public class SharedNomaiTextInfo
                {
                    /// <summary>
                    /// The id of the stone this text will appear for
                    /// </summary>
                    public string id;

                    /// <summary>
                    /// Additional information about each arc in the text
                    /// </summary>
                    public NomaiTextArcInfo[] arcInfo;

                    /// <summary>
                    /// The random seed used to pick what the text arcs will look like.
                    /// </summary>
                    public int seed; // For randomizing arcs

                    /// <summary>
                    /// The location of this object. Arcs will be blue if their locations match the wall, else orange.
                    /// </summary>
                    [DefaultValue("unspecified")] public NomaiTextLocation location = NomaiTextLocation.UNSPECIFIED;

                    /// <summary>
                    /// The relative path to the xml file for this object.
                    /// </summary>
                    public string xmlFile;

                    /// <summary>
                    /// An optional rename of this object
                    /// </summary>
                    public string rename;
                }
            }

            [JsonObject]
            public class PlatformInfo : GeneralPropInfo
            {
                /// <summary>
                /// A ship log fact to reveal when the platform is connected to.
                /// </summary>
                [DefaultValue("")] public string reveals = "";

                /// <summary>
                /// Disable the structure, leaving only the pedestal.
                /// </summary>
                public bool disableStructure;

                /// <summary>
                /// Disable the pool that rises when you place a stone.
                /// </summary>
                public bool disablePool;
            }

            [JsonObject]
            public class StoneInfo : GeneralPropInfo
            {

            }
        }
    }
}
