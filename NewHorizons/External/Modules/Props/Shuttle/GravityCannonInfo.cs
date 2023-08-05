using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Props.Shuttle
{
    [JsonObject]
    public class GravityCannonInfo : GeneralPropInfo
    {
        /// <summary>
        /// Unique ID for the shuttle that pairs with this gravity cannon
        /// </summary>
        public string shuttleID;

        /// <summary>
        /// Ship log fact revealed when retrieving the shuttle to this pad. Optional.
        /// </summary>
        public string retrieveReveal;

        /// <summary>
        /// Ship log fact revealed when launching from this pad. Optional.
        /// </summary>
        public string launchReveal;

        /// <summary>
        /// Hide the lattice cage around the platform. Defaults to true.
        /// </summary>
        [DefaultValue(true)] public bool detailed = true;

        /// <summary>
        /// Will create a modern Nomai computer linked to this gravity cannon.
        /// </summary>
        public NomaiComputerInfo computer;

        /// <summary>
        /// Position of the interface used to launc the shuttle
        /// </summary>
        public GeneralPropInfo controls;
    }
}
