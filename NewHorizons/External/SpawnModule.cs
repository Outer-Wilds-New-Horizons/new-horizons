using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class SpawnModule : Module
    {
        public MVector3 PlayerSpawnPoint { get; set; }
        public MVector3 ShipSpawnPoint { get; set; }
        public bool StartWithSuit { get; set; }
    }
}
