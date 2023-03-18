using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    internal class LoadCreditsVolume : BaseVolume
    {
        public VolumesModule.LoadCreditsVolumeInfo.CreditsType creditsType = VolumesModule.LoadCreditsVolumeInfo.CreditsType.Fast;

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector") && enabled)
            {
                switch(creditsType)
                {
                    case VolumesModule.LoadCreditsVolumeInfo.CreditsType.Fast:
                        LoadManager.LoadScene(OWScene.Credits_Fast, LoadManager.FadeType.ToBlack);
                        break;
                    case VolumesModule.LoadCreditsVolumeInfo.CreditsType.Final:
                        LoadManager.LoadScene(OWScene.Credits_Final, LoadManager.FadeType.ToBlack);
                        break;
                    case VolumesModule.LoadCreditsVolumeInfo.CreditsType.Kazoo:
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
