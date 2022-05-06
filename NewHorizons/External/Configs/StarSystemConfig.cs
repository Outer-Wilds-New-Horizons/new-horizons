using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Configs
{
    public class StarSystemConfig : Config
    {
        public bool canEnterViaWarpDrive = true;
        public bool startHere = false;
        public bool destroyStockPlanets = true;
        public string factRequiredForWarp;
        public NomaiCoordinates coords;

        public class NomaiCoordinates
        {
            public int[] x;
            public int[] y;
            public int[] z;
        }

        public StarSystemConfig(Dictionary<string, object> dict) : base(dict) { }
    }
}
