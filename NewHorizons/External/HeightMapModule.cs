using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class HeightMapModule : Module
    {
        public string HeightMap { get; set; }
        public string TextureMap { get; set; }
        public float MinHeight { get; set; }
        public float MaxHeight { get; set; }
    }
}
