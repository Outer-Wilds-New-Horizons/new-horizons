using NewHorizons.Utility;
namespace NewHorizons.External.Modules
{
    public class HeightMapModule
    {
        /// <summary>
        /// Relative filepath to the texture used for the terrain height.
        /// </summary>
        public string HeightMap;
        
        /// <summary>
        /// Relative filepath to the texture used for the terrain.
        /// </summary>
        public string TextureMap;
        
        /// <summary>
        /// The lowest points on your planet will be at this height.
        /// </summary>
        public float MinHeight;
        
        /// <summary>
        /// The highest points on your planet will be at this height.
        /// </summary>
        public float MaxHeight;
        
        /// <summary>
        /// The scale of the terrain.
        /// </summary>
        public MVector3 Stretch;
    }
}
