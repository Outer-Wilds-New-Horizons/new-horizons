using NewHorizons.Builder.Props;
using NewHorizons.Handlers;
using UnityEngine;

namespace NewHorizons.Components.Props
{
    public class NHItem : OWItem
    {
        public string DisplayName;
        public bool Droppable;
        public AudioType PickupAudio;
        public AudioType DropAudio;
        public AudioType SocketAudio;
        public AudioType UnsocketAudio;
        public Vector3 HoldOffset;
        public Vector3 HoldRotation;
        public Vector3 SocketOffset;
        public Vector3 SocketRotation;
        public string PickupCondition;
        public bool ClearPickupConditionOnDrop;
        public string PickupFact;

        public ItemType ItemType
        {
            get => _type;
            set => _type = value;
        }

        public override string GetDisplayName()
        {
            return TranslationHandler.GetTranslation(DisplayName, TranslationHandler.TextType.UI);
        }

        public override bool CheckIsDroppable()
        {
            return Droppable;
        }

        public override void PickUpItem(Transform holdTranform)
        {
            base.PickUpItem(holdTranform);
            transform.localPosition = HoldOffset;
            transform.localEulerAngles = HoldRotation;
            TriggerPickupConditions();
            PlayCustomSound(PickupAudio);
        }

        public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
        {
            base.DropItem(position, normal, parent, sector, customDropTarget);
            TriggerDropConditions();
            PlayCustomSound(DropAudio);
        }

        public override void SocketItem(Transform socketTransform, Sector sector)
        {
            base.SocketItem(socketTransform, sector);
            transform.localPosition = SocketOffset;
            transform.localEulerAngles = SocketRotation;
            TriggerDropConditions();
            PlayCustomSound(SocketAudio);
        }

        public override void OnCompleteUnsocket()
        {
            base.OnCompleteUnsocket();
            TriggerPickupConditions();
            PlayCustomSound(UnsocketAudio);
        }

        internal void TriggerPickupConditions()
        {
            if (!string.IsNullOrEmpty(PickupCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(PickupCondition, true);
            }
            if (!string.IsNullOrEmpty(PickupFact))
            {
                Locator.GetShipLogManager().RevealFact(PickupFact);
            }
        }

        internal void TriggerDropConditions()
        {
            if (ClearPickupConditionOnDrop && !string.IsNullOrEmpty(PickupCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(PickupCondition, false);
            }
        }

        void PlayCustomSound(AudioType audioType)
        {
            if (ItemBuilder.IsCustomItemType(ItemType))
            {
                Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(audioType);
            }
            else
            {
                // Vanilla items play sounds via hard-coded ItemType switch statements
                // in the PlayerAudioController code, so there's no clean way to override them
            }
        }
    }
}
