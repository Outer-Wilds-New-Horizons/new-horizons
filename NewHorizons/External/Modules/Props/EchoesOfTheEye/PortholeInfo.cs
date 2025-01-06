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
    public class PortholeInfo : GeneralPropInfo
    {
        /// <summary>
        /// Fact IDs to reveal when peeking through the porthole.
        /// </summary>
        public string[] revealFacts;

        /// <summary>
        /// The field of view of the porthole camera.
        /// </summary>
        [DefaultValue(90f)] public float fieldOfView = 90f;

        /// <summary>
        /// The location of the camera when the player peeks through the porthole. Can be placed on a different planet.
        /// </summary>
        public PortholeTargetInfo target;
    }

    [JsonObject]
    public class PortholeTargetInfo : GeneralSolarSystemPropInfo
    {
        
    }
}
