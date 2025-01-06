using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class AsteroidBeltModule
    {
        /// <summary>
        /// Amount of asteroids to create.
        /// </summary>
        [Range(-1, 200)] [DefaultValue(-1)] public int amount = -1;

        /// <summary>
        /// Angle between the belt and the equatorial plane of the planet.
        /// </summary>
        public float inclination;

        /// <summary>
        /// Lowest distance from the planet asteroids can spawn
        /// </summary>
        [Range(0f, float.MaxValue)] public float innerRadius;

        /// <summary>
        /// Angle defining the point where the belt rises up from the planet's equatorial plane if inclination is nonzero.
        /// </summary>
        public float longitudeOfAscendingNode;

        /// <summary>
        /// Maximum size of the asteroids.
        /// </summary>
        [Range(0f, float.MaxValue)] [DefaultValue(50)]
        public float maxSize = 50f;

        /// <summary>
        /// Minimum size of the asteroids.
        /// </summary>
        [Range(0f, float.MaxValue)] [DefaultValue(20)]
        public float minSize = 20;

        /// <summary>
        /// Greatest distance from the planet asteroids can spawn
        /// </summary>
        [Range(0f, float.MaxValue)] public float outerRadius;

        /// <summary>
        /// How the asteroids are generated, unless you supply a detail yourself using "assetBundle" and "path"
        /// </summary>
        public ProcGenModule procGen;

        /// <summary>
        /// Number used to randomize asteroid positions
        /// </summary>
        public int randomSeed;

        /// <summary>
        /// You can use this to load a custom asset or ingame object, instead of using ProcGen. It will be scaled by "minSize" and "maxSize", so ideally it should be near a 1 meter radius.
        /// This is a relative filepath to an asset-bundle to load the prefab defined in `path` from.
        /// </summary>
        public string assetBundle;

        /// <summary>
        /// You can use this to load a custom asset or ingame object, instead of using ProcGen. It will be scaled by "minSize" and "maxSize", so ideally it should be near a 1 meter radius.
        /// This is either the path in the scene hierarchy of the item to copy or the path to the object in the supplied asset bundle. 
        /// </summary>
        public string path;

        /// <summary>
        /// Surface gravity of the asteroids.
        /// </summary>
        [Range(0f, float.MaxValue)]
        [DefaultValue(1)]
        public float gravity = 1f;

        /// <summary>
        /// Should the detail of the asteroid be randomly oriented, or should it point towards the center.
        /// </summary>
        [DefaultValue(true)]
        public bool randomOrientation = true;
    }
}