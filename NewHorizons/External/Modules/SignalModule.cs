using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NewHorizons.External.Props;
using NewHorizons.Utility;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class SignalModule
    {
        /// <summary>
        /// List of signals to add (Why did xen do it like this)
        /// </summary>
        public SignalInfo[] signals;
    }
}