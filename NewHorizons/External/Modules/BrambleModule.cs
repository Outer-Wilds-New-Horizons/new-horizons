using NewHorizons.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            /// Defines the inner radius of this dimension. Leave blank for the default of 
            /// </summary>
            public float radius;

            /// <summary>
            /// The color of the fog inside this dimension. Leave blank for the default yellowish color
            /// </summary>
            public MColor fogTint;
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
            /// The name of the planet that hosts the dimension this node links to
            /// </summary>
            public string linksTo;

            /// <summary>
            /// Set this to true to make this node a seed instead of a node the player can enter
            /// </summary>
            public bool seed;

            /// <summary>
            /// The color of the fog inside the node. Leave blank for the default yellowish color
            /// </summary>
            public MColor fogTint;

            /// <summary>
            /// The color of the shafts of light coming from the entrances to the node. Leave blank for the default yellowish color
            /// </summary>
            public MColor lightTint;
        }
    }
}
