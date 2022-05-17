using NewHorizons.Utility;
namespace NewHorizons.External.Modules
{
    public class HeightMapModule : Module
    {
        public string HeightMap { get; set; }
        public string TextureMap { get; set; }
        public float MinHeight { get; set; }
        public float MaxHeight { get; set; }
        public MVector3 Stretch { get; set; }
    }
}
