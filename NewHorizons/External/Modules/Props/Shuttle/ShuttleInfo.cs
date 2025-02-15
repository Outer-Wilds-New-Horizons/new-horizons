using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Props.Shuttle
{
    [JsonObject]
    public class ShuttleInfo : GeneralPropInfo
    {
        /// <summary>
        /// Unique ID for this shuttle
        /// </summary>
        public string id;
    }
}
