using NewHorizons.Utility;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class ProcGenModule
    {
        public MColor color;

        [Range(0, double.MaxValue)] public float scale;
    }
}