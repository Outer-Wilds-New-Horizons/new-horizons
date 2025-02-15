using NewHorizons.Handlers;

namespace NewHorizons.Components.Volumes
{
    public class BlackHoleWarpVolume : BlackHoleDestructionVolume
    {
        public string TargetSolarSystem { get; set; }
        public string TargetSpawnID { get; set; }

        public override void Awake()
        {
            base.Awake();
            _deathType = DeathType.Meditation;
        }

        public override void VanishShip(OWRigidbody shipBody, RelativeLocationData entryLocation)
        {
            if (PlayerState.IsInsideShip()) Main.Instance.ChangeCurrentStarSystem(TargetSolarSystem, true);
        }

        public override void VanishPlayer(OWRigidbody playerBody, RelativeLocationData entryLocation)
        {
            Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.BH_BlackHoleEmission);
            FadeHandler.FadeOut(0.2f, false);
            Main.Instance.ChangeCurrentStarSystem(TargetSolarSystem, PlayerState.AtFlightConsole());
            PlayerSpawnHandler.TargetSpawnID = TargetSpawnID;
        }
    }
}
