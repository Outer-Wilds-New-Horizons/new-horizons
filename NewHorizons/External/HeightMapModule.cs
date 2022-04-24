using UnityEngine;

namespace NewHorizons.External
{
    public class HeightMapModule : Module
    {
        public string HeightMap { get; set; }
        public string TextureMap { get; set; }
        public float MinHeight { get; set; }
        public float MaxHeight { get; set; }
        public Vector3 Stretch { get; set; } = Vector3.one;
    }
}
