using NewHorizons.OtherMods.AchievementsPlus;
using NewHorizons.Utility.OWUtilities;
using UnityEngine;

namespace NewHorizons.Components.Achievement
{
    public class AchievementObserveTrigger : MonoBehaviour, IObservable
    {
        public string achievementID;
        public float maxViewDistance = 2f;
        public float maxViewAngle = 180f;
        public bool disableColliderOnUnlockAchievement;
        public bool achievementUnlocked;

        private OWCollider _owCollider;

        private void Reset()
        {
            gameObject.layer = Layer.Interactible;
        }

        private void Awake()
        {
            if (disableColliderOnUnlockAchievement)
            {
                _owCollider = gameObject.GetAddComponent<OWCollider>();
            }
        }

        public void Observe(RaycastHit raycastHit, Vector3 raycastOrigin)
        {
            float num = Vector3.Angle(raycastHit.point - raycastOrigin, -transform.forward);
            if (!achievementUnlocked && raycastHit.distance < maxViewDistance && num < maxViewAngle)
            {
                if (disableColliderOnUnlockAchievement)
                {
                    _owCollider.SetActivation(false);
                }

                AchievementHandler.Earn(achievementID);

                achievementUnlocked = true;
            }
        }

        public void LoseFocus() { }
    }
}
