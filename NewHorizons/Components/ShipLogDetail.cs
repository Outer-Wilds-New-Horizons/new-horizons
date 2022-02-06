using NewHorizons.External;
using OWML.Common;
using UnityEngine;
using UnityEngine.UI;

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

        public void UpdateState(bool parentRevealed)
        {
            if (parentRevealed)
            {
                revealedImage.enabled = true;
                outlineImage.enabled = false;
            }
            else
            {
                revealedImage.enabled = false;
                outlineImage.enabled = !detailInfo.invisibleWhenHidden;
            }
        }
    }
}