using NewHorizons.External.Modules;
using UnityEngine;
using UnityEngine.UI;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Components
{
    public class ShipLogDetail : MonoBehaviour
    {
        private Image _revealedImage;
        private Image _outlineImage;
        private Material _greyScaleMaterial;
        private ShipLogModule.ShipLogDetailInfo _detailInfo;

        public void Init(ShipLogModule.ShipLogDetailInfo info, Image revealed, Image outline, Material greyScale)
        {
            _detailInfo = info;
            _revealedImage = revealed;
            _outlineImage = outline;
            _greyScaleMaterial = greyScale;
            _revealedImage.enabled = false;
            _outlineImage.enabled = false;
        }

        public void UpdateState(ShipLogEntry.State parentState)
        {
            switch (parentState)
            {
                case ShipLogEntry.State.Explored:
                    _outlineImage.enabled = false;
                    _revealedImage.enabled = true;
                    SetGreyScale(false);
                    break;
                case ShipLogEntry.State.Rumored:
                    _outlineImage.enabled = false;
                    _revealedImage.enabled = true;
                    SetGreyScale(true);
                    break;
                case ShipLogEntry.State.Hidden:
                    _revealedImage.enabled = false;
                    _outlineImage.enabled = !_detailInfo.invisibleWhenHidden;
                    break;
                case ShipLogEntry.State.None:
                    _revealedImage.enabled = false;
                    _outlineImage.enabled = false;
                    break;
                default:
                    Logger.LogError("Invalid ShipLogEntryState for " +
                                    _revealedImage.transform.parent.parent.gameObject.name);
                    break;
            }
        }

        private void SetGreyScale(bool greyScale)
        {
            _revealedImage.material = greyScale ? _greyScaleMaterial : null;
        }
    }
}