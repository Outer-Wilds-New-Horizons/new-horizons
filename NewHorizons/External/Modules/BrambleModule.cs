using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VinePrefabType
    {
        [EnumMember(Value = @"none")] None = 0,

        [EnumMember(Value = @"hub")] Hub = 1,

        [EnumMember(Value = @"cluster")] Cluster = 2,

        [EnumMember(Value = @"smallNest")] SmallNest = 3,

        [EnumMember(Value = @"exitOnly")] ExitOnly = 4
    }

    [JsonObject]
    public class BrambleModule
    {
        /// <summary>
        /// Defining this value will make this body a bramble dimension. Leave it null to not do that.
        /// </summary>
        public BrambleDimensionInfo dimension;

        /// <summary>
        /// Place nodes/seeds that take you to other bramble dimensions
        /// </summary>
        public BrambleNodeInfo[] nodes;

        
        [JsonObject]
        public class BrambleDimensionInfo
        {
            /// <summary>
            /// The color of the fog inside this dimension. 
            /// Leave blank for the default grayish color: (84, 83, 73)
            /// The alpha value has no effect of the fog: Use fogDensity instead!
            /// </summary>
            public MColor fogTint;

            /// <summary>
            /// The density of the fog inside this dimension. The default is 6. If you want no fog, set this to 0.
            /// </summary>
            [DefaultValue(6f)] public float fogDensity = 6f;

            /// <summary>
            /// The name of the *node* that the player is taken to when exiting this dimension.
            /// </summary>
            public string linksTo;

            /// <summary>
            /// The internal radius (in meters) of the dimension. 
            /// The default is 750 for the Hub, Escape Pod, and Angler Nest dimensions, and 500 for the others.
            /// </summary>
            [DefaultValue(750f)] public float radius = 750f;

            /// <summary>
            /// The dimension the vines will be copied from.
            /// Only a handful are available due to batched colliders.
            /// </summary>
            [DefaultValue("hub")] public VinePrefabType vinePrefab = VinePrefabType.Hub;

            /// <summary>
            /// An array of integers from 0-5. By default, all entrances are allowed. To force this dimension to warp players in from only one point (like the anglerfish nest dimension in the base game) set this value to [3], [5], or similar. Values of 0-5 only.
            /// </summary>
            public int[] allowedEntrances;
        }

        
        [JsonObject]
        public class BrambleNodeInfo : GeneralPropInfo
        {
            /// <summary>
            /// The physical scale of the node, as a multiplier of the original size. 
            /// Nodes are 150m across, seeds are 10m across.
            /// </summary>
            [DefaultValue(1f)] public float scale = 1f;

            /// <summary>
            /// The name of the planet that hosts the dimension this node links to
            /// </summary>
            public string linksTo;

            /// <summary>
            /// The name of this node. Only required if this node should serve as an exit.
            /// </summary>
            public string name;

            /// <summary>
            /// Set this to true to make this node a seed instead of a node the player can enter
            /// </summary>
            [DefaultValue(false)] public bool isSeed = false;

            /// <summary>
            /// The color of the fog inside the node. 
            /// Leave blank for the default yellowish white color: (255, 245, 217, 255)
            /// </summary>
            public MColor fogTint;

            /// <summary>
            /// The color of the light from the node. Alpha controls brightness.
            /// Leave blank for the default white color.
            /// </summary>
            public MColor lightTint;

            /// <summary>
            /// Should this node have a point of light from afar? 
            /// By default, nodes will have a foglight, while seeds won't, and neither will if not in a dimension.
            /// </summary>
            public bool? hasFogLight;

            /// <summary>
            /// An array of integers from 0-5. By default, all exits are allowed. To force this node to warp players out from only one hole set this value to [3], [5], or similar. Values of 0-5 only.
            /// </summary>
            public int[] possibleExits;

            /// <summary>
            /// If your game hard crashes upon entering bramble, it's most likely because you have indirectly recursive dimensions, i.e. one leads to another that leads back to the first one.
            /// Set this to true for one of the nodes in the recursion to fix this, at the cost of it no longer showing markers for the scout, ship, etc.
            /// </summary>
            [DefaultValue(false)] public bool preventRecursionCrash = false;

            #region Obsolete

            [Obsolete("farFogTint is deprecated, please use fogTint instead")]
            public MColor farFogTint;

            #endregion Obsolete
        }
    }
}
