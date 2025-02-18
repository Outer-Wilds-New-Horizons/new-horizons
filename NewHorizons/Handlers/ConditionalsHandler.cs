using NewHorizons.External.Modules.Conditionals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Handlers
{
    public static class ConditionalsHandler
    {
        public static bool Check(ConditionalCheckConditionsInfo check)
        {
            var dcm = DialogueConditionManager.SharedInstance;

            var passed = true;
            if (check.allConditionsSet != null && check.allConditionsSet.Length > 0)
            {
                passed = passed && check.allConditionsSet.All(dcm.GetConditionState);
            }
            if (check.anyConditionsSet != null && check.anyConditionsSet.Length > 0)
            {
                passed = passed && check.anyConditionsSet.Any(dcm.GetConditionState);
            }
            if (check.allPersistentConditionsSet != null && check.allPersistentConditionsSet.Length > 0)
            {
                passed = passed && check.allPersistentConditionsSet.All(PlayerData.GetPersistentCondition);
            }
            if (check.anyPersistentConditionsSet != null && check.anyPersistentConditionsSet.Length > 0)
            {
                passed = passed && check.anyPersistentConditionsSet.Any(PlayerData.GetPersistentCondition);
            }
            if (check.allFactsRevealed != null && check.allFactsRevealed.Length > 0)
            {
                passed = passed && check.allFactsRevealed.All(ShipLogHandler.KnowsFact);
            }
            if (check.anyFactsRevealed != null && check.anyFactsRevealed.Length > 0)
            {
                passed = passed && check.anyFactsRevealed.Any(ShipLogHandler.KnowsFact);
            }
            if (check.invert)
            {
                passed = !passed;
            }

            return passed;
        }

        public static void ApplyEffects(ConditionalCheckEffectsInfo effects, bool checkPassed)
        {
            if ((checkPassed || effects.reversible) && effects.setConditions != null)
            {
                foreach (var condition in effects.setConditions)
                {
                    if (DialogueConditionManager.SharedInstance.GetConditionState(condition) != checkPassed)
                    {
                        DialogueConditionManager.SharedInstance.SetConditionState(condition, checkPassed);
                    }
                }
            }
            else if ((!checkPassed || effects.reversible) && effects.unsetConditions != null)
            {
                foreach (var condition in effects.unsetConditions)
                {
                    if (DialogueConditionManager.SharedInstance.GetConditionState(condition) != !checkPassed)
                    {
                        DialogueConditionManager.SharedInstance.SetConditionState(condition, !checkPassed);
                    }
                }
            }
            if ((checkPassed || effects.reversible) && effects.setPersistentConditions != null)
            {
                foreach (var condition in effects.setPersistentConditions)
                {
                    if (!PlayerData.GetPersistentCondition(condition) != checkPassed)
                    {
                        PlayerData.SetPersistentCondition(condition, checkPassed);
                    }
                }
            }
            else if ((!checkPassed || effects.reversible) && effects.unsetPersistentConditions != null)
            {
                foreach (var condition in effects.unsetPersistentConditions)
                {
                    if (PlayerData.GetPersistentCondition(condition) != !checkPassed)
                    {
                        PlayerData.SetPersistentCondition(condition, !checkPassed);
                    }
                }
            }
            if (checkPassed && effects.revealFacts != null)
            {
                foreach (var fact in effects.revealFacts)
                {
                    if (!ShipLogHandler.KnowsFact(fact))
                    {
                        Locator.GetShipLogManager().RevealFact(fact);
                    }
                }
            }
        }
    }
}
