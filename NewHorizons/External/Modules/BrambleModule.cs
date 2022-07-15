using NewHorizons.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules
{
    
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
            /// The color of the fog inside this dimension. Leave blank for the default yellowish color: (113, 107, 81)
            /// </summary>
            public MColor fogTint;

            /// <summary>
            /// The name of the *node* that the player is taken to when exiting this dimension.
            /// </summary>
            public string linksTo;

            /// <summary>
            /// The internal radius (in meters) of the dimension. The default is 1705.
            /// </summary>
            [DefaultValue(1705f)] public float radius = 1705f;

            /// <summary>
            /// An array of integers from 0-5. By default, all entrances are allowed. To force this dimension to warp players in from only one point (like the anglerfish nest dimension in the base game) set this value to [3], [5], or similar. Values of 0-5 only.
            /// </summary>
            public int[] allowedEntrances;
        }

        
        [JsonObject]
        public class BrambleNodeInfo
        {
            /// <summary>
            /// The physical position of the node
            /// </summary>
            public MVector3 position;
            
            /// <summary>
            /// The physical rotation of the node
            /// </summary>
            public MVector3 rotation;

            /// <summary>
            /// The physical scale of the node, as a multiplier of the original size
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
            /// The color of the fog inside the node. Leave blank for the default yellowish color: (131, 124, 105, 255)
            /// </summary>
            public MColor fogTint;

            /// <summary>
            /// The color of the shafts of light coming from the entrances to the node. Leave blank for the default yellowish color: (131, 124, 105, 255)
            /// </summary>
            public MColor lightTint;

            /// <summary>
            /// The color a dimension's background fog turns when you approach this node (if it's in a dimension). If this node is not in a dimension, this does nothing. Leave blank for the default yellowish white color: (255, 245, 217, 255)
            /// </summary>
            public MColor farFogTint;

            /// <summary>
            /// An array of integers from 0-5. By default, all entrances are allowed. To force this node to warp players out from only one hole set this value to [3], [5], or similar. Values of 0-5 only.
            /// </summary>
            public int[] possibleExits;
        }
    }
}
