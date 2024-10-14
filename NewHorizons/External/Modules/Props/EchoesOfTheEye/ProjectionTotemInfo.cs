using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Props.EchoesOfTheEye
{
    [JsonObject]
    public class ProjectionTotemInfo : GeneralPropInfo
    {
        /// <summary>
        /// Whether the totem should start lit or extinguished.
        /// </summary>
        public bool startLit;

        /// <summary>
        /// Whether the projection totem should be able to extinguished but not be able to be lit again with the artifact. Mainly useful if `startLit` is set to true.
        /// </summary>
        public bool extinguishOnly;

        /// <summary>
        /// A relative path from this planet to an alarm totem that will be activated or deactivated based on whether this totem is lit.
        /// </summary>
        public string pathToAlarmTotem;

        /// <summary>
        /// Relative paths from this planet to objects containing dream candles that will be activated or deactivated based on whether this totem is lit. All dream candles in the selected objects will be connected to this totem, so they do not need to be specified individually if a parent object is specified.
        /// </summary>
        public string[] pathsToDreamCandles;

        /// <summary>
        /// Relative paths from this planet to projection totems that will be deactivated if this totem is extinguished. All projection totems in the selected objects will be connected to this totem, so they do not need to be specified individually if a parent object is specified.
        /// </summary>
        public string[] pathsToProjectionTotems;

        /// <summary>
        /// Relative paths from this planet to objects that will appear or disappear when this totem is lit or extinguished. Some types of objects and effects are not supported and will remain visible and active.
        /// </summary>
        public string[] pathsToProjectedObjects;
    }
}
