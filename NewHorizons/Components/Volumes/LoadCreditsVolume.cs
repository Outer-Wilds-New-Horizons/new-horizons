using NewHorizons.External.Modules;
using NewHorizons.External.Volumes;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    internal class LoadCreditsVolume : BaseVolume
    {
        public LoadCreditsVolumeInfo.CreditsType creditsType = LoadCreditsVolumeInfo.CreditsType.Fast;

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                switch(creditsType)
                {
                    case LoadCreditsVolumeInfo.CreditsType.Fast:
                        LoadManager.LoadScene(OWScene.Credits_Fast, LoadManager.FadeType.ToBlack);
                        break;
                    case LoadCreditsVolumeInfo.CreditsType.Final:
                        LoadManager.LoadScene(OWScene.Credits_Final, LoadManager.FadeType.ToBlack);
                        break;
                    case LoadCreditsVolumeInfo.CreditsType.Kazoo:
                        TimelineObliterationController.s_hasRealityEnded = true;
                        LoadManager.LoadScene(OWScene.Credits_Fast, LoadManager.FadeType.ToBlack);
                        break;
                }
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj)
        {

        }
    }
}
