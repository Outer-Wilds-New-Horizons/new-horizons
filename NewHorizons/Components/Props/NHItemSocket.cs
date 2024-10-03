using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Props
{
    public class NHItemSocket : OWItemSocket
    {
        public bool UseGiveTakePrompts;
        public string InsertCondition;
        public bool ClearInsertConditionOnRemoval;
        public string InsertFact;
        public string RemovalCondition;
        public bool ClearRemovalConditionOnInsert;
        public string RemovalFact;

        public ItemType ItemType
        {
            get => _acceptableType;
            set => _acceptableType = value;
        }

        public override bool UsesGiveTakePrompts()
        {
            return UseGiveTakePrompts;
        }

        public override bool AcceptsItem(OWItem item)
        {
            if (item == null || item._type == ItemType.Invalid)
            {
                return false;
            }
            return ItemType == item._type;
        }

        public override bool PlaceIntoSocket(OWItem item)
        {
            if (base.PlaceIntoSocket(item))
            {
                TriggerInsertConditions();
                return true;
            }
            return false;
        }

        public override OWItem RemoveFromSocket()
        {
            var removedItem = base.RemoveFromSocket();
            if (removedItem != null)
            {
                TriggerRemovalConditions();
            }
            return removedItem;
        }

        internal void TriggerInsertConditions()
        {
            if (!string.IsNullOrEmpty(InsertCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(InsertCondition, true);
            }
            if (ClearRemovalConditionOnInsert && !string.IsNullOrEmpty(RemovalCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(RemovalCondition, false);
            }
            if (!string.IsNullOrEmpty(InsertFact))
            {
                Locator.GetShipLogManager().RevealFact(InsertFact);
            }
        }

        internal void TriggerRemovalConditions()
        {
            if (!string.IsNullOrEmpty(RemovalCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(RemovalCondition, true);
            }
            if (ClearInsertConditionOnRemoval && !string.IsNullOrEmpty(InsertCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(InsertCondition, false);
            }
            if (!string.IsNullOrEmpty(RemovalFact))
            {
                Locator.GetShipLogManager().RevealFact(RemovalFact);
            }
        }
    }
}
