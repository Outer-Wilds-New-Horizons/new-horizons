using NewHorizons.OtherMods.AchievementsPlus.NH;

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
            SurveyorProbe requiredComponent = probeBody.GetRequiredComponent<SurveyorProbe>();
            if (requiredComponent.IsLaunched())
            {
                UnityEngine.Object.Destroy(requiredComponent.gameObject);
                ProbeLostAchievement.Earn();
            }
        }
    }
}

