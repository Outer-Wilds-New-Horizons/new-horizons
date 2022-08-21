using UnityEngine;

namespace NewHorizons.OtherMods.OWRichPresence
{
    public interface IRichPresenceAPI
    {
        public void SetRichPresence(string message, int imageKey);
        public void SetTriggerActivation(bool active);
        public void CreateTrigger(GameObject parent, Sector sector, string details, string imageKey);
        public void SetCurrentRootPresence(string message, string imageKey);
    }
}
