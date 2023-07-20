using NewHorizons.OtherMods.AchievementsPlus.NH;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    public class BlackHoleDestructionVolume : DestructionVolume
    {
        protected OWAudioSource _audio;

        public override void Awake()
        {
            base.Awake();
            _deathType = DeathType.BlackHole;
            _audio = GetComponent<OWAudioSource>();
        }

        public override void VanishProbe(OWRigidbody probeBody, RelativeLocationData entryLocation)
        {
            var probe = probeBody.GetRequiredComponent<SurveyorProbe>();

            _audio.PlayOneShot(AudioType.BH_BlackHoleEmission, 1f);

            if (probe.IsLaunched())
            {
                Destroy(probe.gameObject);
                ProbeLostAchievement.Earn();
            }
        }
    }
}

