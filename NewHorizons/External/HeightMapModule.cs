using NewHorizons.Utility;
using UnityEngine;

namespace NewHorizons.External
{
    public class HeightMapModule : Module
    {
        public string HeightMap { get; set; }
        public string TextureMap { get; set; }
        public float MinHeight { get; set; }
        public float MaxHeight { get; set; }
        public MVector3 Stretch { get; set; } = (MVector3)Vector3.one;
    }
}
