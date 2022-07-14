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
        /// Angle between the rings and the equatorial plane of the planet.
        /// </summary>
        public float inclination;

        /// <summary>
        /// Lowest distance from the planet asteroids can spawn
        /// </summary>
        [Range(0f, double.MaxValue)] public float innerRadius;

        /// <summary>
        /// Angle defining the point where the rings rise up from the planet's equatorial plane if inclination is nonzero.
        /// </summary>
        public float longitudeOfAscendingNode;

        /// <summary>
        /// Maximum size of the asteroids.
        /// </summary>
        [Range(0f, double.MaxValue)] [DefaultValue(50)]
        public float maxSize = 50f;

        /// <summary>
        /// Minimum size of the asteroids.
        /// </summary>
        [Range(0f, double.MaxValue)] [DefaultValue(20)]
        public float minSize = 20;

        /// <summary>
        /// Greatest distance from the planet asteroids can spawn
        /// </summary>
        [Range(0f, double.MaxValue)] public float outerRadius;

        /// <summary>
        /// How the asteroids are generated
        /// </summary>
        public ProcGenModule procGen;

        /// <summary>
        /// Number used to randomize asteroid positions
        /// </summary>
        public int randomSeed;
    }
}