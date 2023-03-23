using Newtonsoft.Json;

namespace NewHorizons.External.Modules.WarpPad
{
    [JsonObject]
    public abstract class NomaiWarpPadInfo : GeneralPropInfo
    {
        public string frequency;
    }
}
