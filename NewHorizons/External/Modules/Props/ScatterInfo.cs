using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props
{
    [JsonObject]
    public class ScatterInfo
    {
        /// <summary>
        /// Relative filepath to an asset-bundle
        /// </summary>
        public string assetBundle;

        /// <summary>
        /// Number of props to scatter
        /// </summary>
        public int count;

        /// <summary>
        /// Offset this prop once it is placed
        /// </summary>
        public MVector3 offset;

        /// <summary>
        /// Either the path in the scene hierarchy of the item to copy or the path to the object in the supplied asset bundle
        /// </summary>
        public string path;

        /// <summary>
        /// Rotate this prop once it is placed
        /// </summary>
        public MVector3 rotation;

        /// <summary>
        /// Scale this prop once it is placed
        /// </summary>
        [DefaultValue(1f)] public float scale = 1f;

        /// <summary>
        /// Scale each axis of the prop. Overrides `scale`.
        /// </summary>
        public MVector3 stretch;

        /// <summary>
        /// The number used as entropy for scattering the props
        /// </summary>
        public int seed;

        /// <summary>
        /// The lowest height that these object will be placed at (only relevant if there's a heightmap)
        /// </summary>
        public float? minHeight;

        /// <summary>
        /// The highest height that these objects will be placed at (only relevant if there's a heightmap)
        /// </summary>
        public float? maxHeight;

        /// <summary>
        /// Should we try to prevent overlap between the scattered details? True by default. If it's affecting load times turn it off.
        /// </summary>
        [DefaultValue(true)] public bool preventOverlap = true;

        /// <summary>
        /// Should this detail stay loaded even if you're outside the sector (good for very large props)
        /// </summary>
        public bool keepLoaded;

        /// <summary>
        /// The relative path from the planet to the parent of this object. Optional (will default to the root sector). This parent should be at the position where you'd like to scatter (which would usually be zero).
        /// </summary>
        public string parentPath;
    }
}
