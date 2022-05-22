using NewHorizons.Utility;
using Newtonsoft.Json;
using System.ComponentModel;
using UnityEngine.Internal;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class StarModule : VariableSizeModule
    {
        /// <summary>
        /// Radius of the star.
        /// </summary>
        [System.ComponentModel.DefaultValue(2000f)]
        public float Size = 2000f;
        
        /// <summary>
        /// Colour of the star.
        /// </summary>
        public MColor Tint; 
        
        /// <summary>
        /// Colour of the star at the end of its life.
        /// </summary>
        public MColor EndTint; 
        
        /// <summary>
        /// The tint of the supernova this star creates when it dies.
        /// </summary>
        public MColor SupernovaTint;
        
        /// <summary>
        /// Colour of the light given off.
        /// </summary>
        public MColor LightTint;
        
        /// <summary>
        /// Relative strength of the light compared to the sun.
        /// </summary>
        [System.ComponentModel.DefaultValue(1f)]
        public float SolarLuminosity = 1f;
        
        /// <summary>
        /// The default sun has its own atmosphere that is different from regular planets. If you want that, set this to `true`.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool HasAtmosphere = true;
        
        /// <summary>
        /// Should this star explode after 22 minutes?
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool GoSupernova = true;
    }
}
