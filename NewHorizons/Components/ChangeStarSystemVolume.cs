using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Components
{
    public class ChangeStarSystemVolume : BlackHoleDestructionVolume
    {
        public string TargetSolarSystem { get; set; }

        public override void VanishPlayer(OWRigidbody playerBody, RelativeLocationData entryLocation)
        {
            Main.Instance.ChangeCurrentStarSystem(TargetSolarSystem);
        }
    }
}
