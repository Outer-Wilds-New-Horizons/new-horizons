using NewHorizons.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Props
{

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
