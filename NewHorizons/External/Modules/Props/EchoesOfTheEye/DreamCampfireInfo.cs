using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Props.EchoesOfTheEye
{
    [JsonObject]
    public class DreamCampfireInfo : GeneralPropInfo
    {
        /// <summary>
        /// Unique ID for this dream-world campfire
        /// </summary>
        public string id;
    }
}
