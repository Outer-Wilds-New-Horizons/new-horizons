using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class AsteroidBeltModule
    {
        /// <summary>
        /// Lowest distance from the planet asteroids can spawn
        /// </summary>
        public float InnerRadius;
        
        /// <summary>
        /// Greatest distance from the planet asteroids can spawn
        /// </summary>
        public float OuterRadius;
        
        /// <summary>
        /// Minimum size of the asteroids.
        /// </summary>
        [System.ComponentModel.DefaultValue(20)]
        public float MinSize = 20;
        
        /// <summary>
        /// Maximum size of the asteroids.
        /// </summary>
        [System.ComponentModel.DefaultValue(50)]
        public float MaxSize = 50f;
        
        /// <summary>
        /// Amount of asteroids to create.
        /// </summary>
        [System.ComponentModel.DefaultValue(-1)]
        public int Amount = -1;
        
        /// <summary>
        /// Angle between the rings and the equatorial plane of the planet.
        /// </summary>
        public float Inclination;
        
        /// <summary>
        /// Angle defining the point where the rings rise up from the planet's equatorial plane if inclination is nonzero.
        /// </summary>
        public float LongitudeOfAscendingNode;
        
        /// <summary>
        /// Number used to randomize asteroid positions
        /// </summary>
        public int RandomSeed;
        
        /// <summary>
        /// How the asteroids are generated
        /// </summary>
        public ProcGenModule ProcGen;
    }
}
