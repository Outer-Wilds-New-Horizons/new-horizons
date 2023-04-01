using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NewHorizons.External.Modules.Props.Audio;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class SignalModule
    {
        [Obsolete("signals is deprecated, please use Props->signals instead")]
        public SignalInfo[] signals;
    }
}