namespace NewHorizons.Components
{
    public class BlackHoleDestructionVolume : DestructionVolume
    {
        public override void Awake()
        {
            base.Awake();
            _deathType = DeathType.BlackHole;
        }

        public override void VanishProbe(OWRigidbody probeBody, RelativeLocationData entryLocation)
        {
            var requiredComponent = probeBody.GetRequiredComponent<SurveyorProbe>();
            if (requiredComponent.IsLaunched()) Destroy(requiredComponent.gameObject);
        }
    }
}