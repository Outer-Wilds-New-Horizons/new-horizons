using NewHorizons.Utility;
using Newtonsoft.Json;

namespace NewHorizons.External.Props
{
    [JsonObject]
    public abstract class GeneralPropInfo : GeneralPointPropInfo
    {
        /// <summary>
        /// Rotation of the object
        /// </summary>
        public MVector3 rotation;
    }
}
