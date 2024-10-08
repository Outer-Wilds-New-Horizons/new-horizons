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
    public class AlarmTotemInfo : GeneralPropInfo
    {
        /// <summary>
        /// The maximum distance of the alarm's "vision cone".
        /// </summary>
        [DefaultValue(45f)] public float sightDistance = 45;

        /// <summary>
        /// The width of the alarm's "vision cone" in degrees.
        /// </summary>
        [DefaultValue(60f)] public float sightAngle = 60f;
    }
}
