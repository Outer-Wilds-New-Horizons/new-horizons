using NewHorizons.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewHorizons.External.Props;

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
            /// </summary>
            public MColor fogTint;

            /// <summary>
            /// The density of the fog inside this dimension. The default is 6.
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

    }
}
