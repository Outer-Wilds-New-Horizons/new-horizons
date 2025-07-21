using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Props.EchoesOfTheEye
{
    public class DreamLightConditionInfo
    {
        /// <summary>
        /// The name of the dialogue condition or persistent condition to set when the light is lit.
        /// </summary>
        public string condition;

        /// <summary>
        /// If true, the condition will persist across all future loops until unset.
        /// </summary>
        public bool persistent;

        /// <summary>
        /// Whether to unset the condition when the light is extinguished again.
        /// </summary>
        public bool reversible;

        /// <summary>
        /// Whether to set the condition when the light is extinguished instead. If `reversible` is true, the condition will be unset when the light is lit again.
        /// </summary>
        public bool onExtinguish;
    }
}
