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
    public class DreamCandleInfo : GeneralPropInfo
    {
        /// <summary>
        /// The type of dream candle this is.
        /// </summary>
        [DefaultValue("ground")] public DreamCandleType type = DreamCandleType.Ground;

        /// <summary>
        /// Whether the candle should start lit or extinguished.
        /// </summary>
        public bool startLit;
    }
}
