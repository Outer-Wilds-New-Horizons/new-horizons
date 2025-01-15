using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Configs
{
    [JsonObject]
    public class MainMenuConfig
    {
        /// <summary>
        /// Colour of the text on the main menu
        /// </summary>
        public MColor menuTextTint;
    }
}
