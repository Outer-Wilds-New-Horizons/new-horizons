using NewHorizons.External;
using OWML.Common;
using UnityEngine;
using UnityEngine.UI;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Components
{
    public class ShipLogDetail : MonoBehaviour
    {
        private Image revealedImage;
        private Image outlineImage;
        private ShipLogModule.ShipLogDetailInfo detailInfo;
        
        public void Init(ShipLogModule.ShipLogDetailInfo info, Image revealed, Image outline)
        {
            detailInfo = info;
            revealedImage = revealed;
            outlineImage = outline;
            revealedImage.enabled = false;
            outlineImage.enabled = false;
        }

        public void UpdateState(ShipLogEntry.State parentState)
        {
            switch (parentState)
            {
                case ShipLogEntry.State.Explored:
                    outlineImage.enabled = false;
                    revealedImage.enabled = true;
                    break;
                case ShipLogEntry.State.Rumored:
                    revealedImage.enabled = false;
                    outlineImage.enabled = true;
                    break;
                case ShipLogEntry.State.Hidden:
                    revealedImage.enabled = false;
                    outlineImage.enabled = !detailInfo.invisibleWhenHidden;
                    break;
                case ShipLogEntry.State.None:
                    revealedImage.enabled = false;
                    outlineImage.enabled = false;
                    break;
            }
        }
    }
}