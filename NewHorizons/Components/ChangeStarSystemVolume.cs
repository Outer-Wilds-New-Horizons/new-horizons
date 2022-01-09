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

        public override void Awake()
        {
            base.Awake();
            _deathType = DeathType.Meditation;
        }

        public override void VanishShip(OWRigidbody shipBody, RelativeLocationData entryLocation)
        {
            if(PlayerState.IsInsideShip()) Main.Instance.ChangeCurrentStarSystem(TargetSolarSystem, true);
        }

        public override void VanishPlayer(OWRigidbody playerBody, RelativeLocationData entryLocation)
        {
            Main.Instance.ChangeCurrentStarSystem(TargetSolarSystem, PlayerState.AtFlightConsole());
            //Main.Instance.ChangeCurrentStarSystem(TargetSolarSystem, false);
        }
    }
}
