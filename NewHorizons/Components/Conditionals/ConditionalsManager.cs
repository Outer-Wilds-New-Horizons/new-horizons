using NewHorizons.External.Modules.Conditionals;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Conditionals
{
    public class ConditionalsManager : MonoBehaviour
    {
        public const int MAX_RECURSION = 100;

        List<ConditionalCheckInfo> _checks = new();
        int _recursionCount = 0;
        bool _avoidRecursionLogSpam = false;

        public void AddCheck(ConditionalCheckInfo check)
        {
            _checks.Add(check);
        }

        public void RemoveCheck(ConditionalCheckInfo check)
        {
            _checks.Remove(check);
        }

        protected void Awake()
        {
            GlobalMessenger.AddListener("DialogueConditionsReset", CalculateChecks);
            GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", CalculateChecks);
            GlobalMessenger<string, bool>.AddListener("NHPersistentConditionChanged", CalculateChecks);
            GlobalMessenger.AddListener("ShipLogUpdated", CalculateChecks);
        }

        protected void OnDestroy()
        {
            GlobalMessenger.RemoveListener("DialogueConditionsReset", CalculateChecks);
            GlobalMessenger<string, bool>.RemoveListener("DialogueConditionChanged", CalculateChecks);
            GlobalMessenger<string, bool>.RemoveListener("NHPersistentConditionChanged", CalculateChecks);
            GlobalMessenger.RemoveListener("ShipLogUpdated", CalculateChecks);
        }

        protected void Update()
        {
            _recursionCount = 0;
            enabled = false;
        }

        void CalculateChecks()
        {
            if (_recursionCount >= MAX_RECURSION)
            {
                if (!_avoidRecursionLogSpam)
                {
                    NHLogger.LogError($"Possible infinite loop detected while processing conditional checks. This is likely caused by a mod using conflicting conditional checks that both set and unset the same condition.");
                    _avoidRecursionLogSpam = true;
                }
                return;
            }

            foreach (var check in _checks)
            {
                bool checkPassed = ConditionalsHandler.Check(check.check);
                if (checkPassed)
                {
                    ConditionalsHandler.ApplyEffects(check.then, checkPassed);
                }
            }
            _recursionCount++;
            enabled = true;
        }

        // We could theoretically filter checks by conditionName here and only do the checks that matter, but the performance gain is likely not significant enough to be worth the extra complexity
        void CalculateChecks(string conditionName, bool conditionState)
        {
            CalculateChecks();
        }
    }
}
