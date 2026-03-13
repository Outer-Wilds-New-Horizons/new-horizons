using NewHorizons.Handlers;
using OWML.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace NewHorizons.Components.Volumes
{
    // RepairReceiver isn't set up for proper subclassing but a subclass is necessary for the first-person manipulator to detect it.
    public class NHRepairReceiver : RepairReceiver
    {
        public static Type RepairReceiverType = EnumUtils.Create<Type>("NewHorizons");

        public class RepairEvent : UnityEvent<NHRepairReceiver> { }

        public RepairEvent OnRepaired = new();
        public RepairEvent OnDamaged = new();

        public string displayName;
        public float repairTime;
        public string damagedCondition;
        public string repairedCondition;
        public string revealFact;

        float _repairFraction = 0f;
        UITextType _uiTextType = UITextType.None;

        public float repairFraction
        {
            get => _repairFraction;
            set
            {
                var prevValue = _repairFraction;
                _repairFraction = Mathf.Clamp01(value);
                if (prevValue < 1f && _repairFraction >= 1f)
                {
                    Repair();
                }
                else if (prevValue >= 1f && _repairFraction < 1f)
                {
                    Damage();
                }
            }
        }

        public new virtual bool IsRepairable() => IsDamaged();
        public new virtual bool IsDamaged() => _repairFraction < 1f;
        public new virtual float GetRepairFraction() => _repairFraction;

        protected new void Awake()
        {
            base.Awake();
            _type = RepairReceiverType;
            if (IsDamaged()) Damage();
            else Repair();
        }

        public new virtual void RepairTick()
        {
            if (!IsRepairable()) return;
            repairFraction += Time.deltaTime / repairTime;
        }

        public new virtual UITextType GetRepairableName()
        {
            if (_uiTextType != UITextType.None) return _uiTextType;
            var value = TranslationHandler.GetTranslation(displayName, TranslationHandler.TextType.UI);
            _uiTextType = (UITextType)TranslationHandler.AddUI(value, false);
            return _uiTextType;
        }

        void Damage()
        {
            if (!string.IsNullOrEmpty(damagedCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(damagedCondition, true);
            }
            if (!string.IsNullOrEmpty(repairedCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(repairedCondition, false);
            }
            OnDamaged.Invoke(this);
        }

        void Repair()
        {
            if (!string.IsNullOrEmpty(damagedCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(damagedCondition, false);
            }
            if (!string.IsNullOrEmpty(repairedCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(repairedCondition, true);
            }
            if (!string.IsNullOrEmpty(revealFact))
            {
                Locator.GetShipLogManager().RevealFact(revealFact);
            }
            OnRepaired.Invoke(this);
        }
    }
}
