using NewHorizons.Handlers;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Props
{
    public class NHItem : OWItem
    {
        public string DisplayName;
        public bool Droppable;
        public string PickupCondition;
        public bool ClearPickupConditionOnDrop;

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
            TriggerPickupConditions();
        }

        public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
        {
            base.DropItem(position, normal, parent, sector, customDropTarget);
            TriggerDropConditions();
        }

        internal void TriggerPickupConditions()
        {
            if (!string.IsNullOrEmpty(PickupCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(PickupCondition, true);
            }
        }

        internal void TriggerDropConditions()
        {
            if (ClearPickupConditionOnDrop && !string.IsNullOrEmpty(PickupCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(PickupCondition, false);
            }
        }
    }
}
