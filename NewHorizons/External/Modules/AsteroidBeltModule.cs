using System.ComponentModel;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class AsteroidBeltModule
    {
        /// <summary>
        /// Amount of asteroids to create.
        /// </summary>
        [DefaultValue(-1)] public int amount = -1;

        /// <summary>
        /// Angle between the rings and the equatorial plane of the planet.
        /// </summary>
        public float inclination;

        /// <summary>
        /// Lowest distance from the planet asteroids can spawn
        /// </summary>
        public float innerRadius;

        /// <summary>
        /// Angle defining the point where the rings rise up from the planet's equatorial plane if inclination is nonzero.
        /// </summary>
        public float longitudeOfAscendingNode;

        /// <summary>
        /// Maximum size of the asteroids.
        /// </summary>
        [DefaultValue(50)] public float maxSize = 50f;

        /// <summary>
        /// Minimum size of the asteroids.
        /// </summary>
        [DefaultValue(20)] public float minSize = 20;

        /// <summary>
        /// Greatest distance from the planet asteroids can spawn
        /// </summary>
        public float outerRadius;

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