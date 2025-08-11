
using NewHorizons.External.SerializableData;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public abstract class BasePropInfo
    {
        /// <summary>
        /// The relative path from the planet to the parent of this object. Optional (will default to the planet's root sector).
        /// </summary>
        public string parentPath;

        /// <summary>
        /// The relative path from the planet to the sector that this object will be linked to. The special value "auto" will use the most specific sector in the parent path. Optional (will default to the planet's root sector).
        /// </summary>
        public string sectorPath;

        /// <summary>
        /// An optional rename of this object
        /// </summary>
        public string rename;
    }

    [JsonObject]
    public abstract class GeneralPointPropInfo : BasePropInfo
    {
        /// <summary>
        /// Position of the object
        /// </summary>
        public MVector3 position;

        /// <summary>
        /// Whether the positional and rotational coordinates are relative to parent instead of the root planet object.
        /// </summary>
        public bool isRelativeToParent;
    }

    [JsonObject]
    public class GeneralPropInfo : GeneralPointPropInfo
    {
        /// <summary>
        /// Rotation of the object
        /// </summary>
        public MVector3 rotation;

        /// <summary>
        /// Do we try to automatically align this object to stand upright relative to the body's center? Stacks with rotation.
        /// Defaults to true for geysers, tornados, and volcanoes, and false for everything else.
        /// </summary>
        public bool? alignRadial;
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
