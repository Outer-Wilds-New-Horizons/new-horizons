
using NewHorizons.Utility;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public abstract class GeneralPointPropInfo
    {
        /// <summary>
        /// Position of the object
        /// </summary>
        public MVector3 position;

        /// <summary>
        /// The relative path from the planet to the parent of this object. Optional (will default to the root sector).
        /// </summary>
        public string parentPath;

        /// <summary>
        /// Whether the positional and rotational coordinates are relative to parent instead of the root planet object.
        /// </summary>
        public bool isRelativeToParent;

        /// <summary>
        /// An optional rename of this object
        /// </summary>
        public string rename;
    }

    [JsonObject]
    public abstract class GeneralPropInfo : GeneralPointPropInfo
    {
        /// <summary>
        /// Rotation of the object
        /// </summary>
        public MVector3 rotation;
    }

    [JsonObject]
    public abstract class GeneralSolarSystemPropInfo : GeneralPropInfo
    {
        /// <summary>
        /// The name of the planet that will be used with `parentPath`. Must be set if `parentPath` is set.
        /// </summary>
        public string parentBody;
    }
}
