using NewHorizons.Components.Orbital;
using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Orbital
{
    static class FocalPointBuilder
    {
        public static void Make(GameObject go, FocalPointModule module)
        {
            var binary = go.AddComponent<BinaryFocalPoint>();
            binary.PrimaryName = module.Primary;
            binary.SecondaryName = module.Secondary;
        }
    }
}
