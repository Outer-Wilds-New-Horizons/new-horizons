using UnityEngine;

namespace NewHorizons.OtherMods.OWRichPresence
{
    public interface IRichPresenceAPI
    {
        public void SetRichPresence(string message, int imageKey);
        public void SetRichPresence(string message, string imageKey);
        public void SetTriggerActivation(bool active);
        public GameObject CreateTrigger(GameObject parent, string message, string imageKey);
        public GameObject CreateTrigger(GameObject parent, Sector sector, string message, string imageKey);
        public void SetCurrentRootPresence(string message, string imageKey);
    }
}
