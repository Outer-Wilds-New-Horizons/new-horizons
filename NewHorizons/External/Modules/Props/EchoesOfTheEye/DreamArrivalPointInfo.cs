using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Props.EchoesOfTheEye
{
    [JsonObject]
    public class DreamArrivalPointInfo : GeneralPropInfo
    {
        /// <summary>
        /// Unique ID for this dream-world arrival point
        /// </summary>
        public string id;
        /// <summary>
        /// Whether to generate simulation meshes (the models used in the "tronworld" or "matrix" view) for most objects on the current planet by cloning the existing meshes and applying the simulation materials. Leave this off if you are building your own simulation meshes or using existing objects which have them.
        /// </summary>
        public bool generateSimulationMeshes;
    }
}
