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
        public const int MAX_RECURSION = 120;

        List<ConditionalCheckInfo> _checks = new();
        bool _checksScheduled;
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
            GlobalMessenger.AddListener("DialogueConditionsReset", ScheduleChecks);
            GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", ScheduleChecks);
            GlobalMessenger<string, bool>.AddListener("NHPersistentConditionChanged", ScheduleChecks);
            GlobalMessenger.AddListener("ShipLogUpdated", ScheduleChecks);
        }

        protected void OnDestroy()
        {
            GlobalMessenger.RemoveListener("DialogueConditionsReset", ScheduleChecks);
            GlobalMessenger<string, bool>.RemoveListener("DialogueConditionChanged", ScheduleChecks);
            GlobalMessenger<string, bool>.RemoveListener("NHPersistentConditionChanged", ScheduleChecks);
            GlobalMessenger.RemoveListener("ShipLogUpdated", ScheduleChecks);
        }

        protected void LateUpdate()
        {
            if (_checksScheduled)
            {
                _checksScheduled = false;
                DoChecks();
            }
            else
            {
                // We had a frame without any checks being scheduled, so reset the recursion count and disable Update() again
                _recursionCount = 0;
                enabled = false;
            }
        }

        void DoChecks()
        {
            if (_recursionCount >= MAX_RECURSION)
            {
                if (!_avoidRecursionLogSpam)
                {
                    NHLogger.LogError($"Possible infinite loop detected while processing conditional checks; conditions were changed every single frame for {MAX_RECURSION} frames. This is likely caused by a mod using conflicting conditional checks that both set and unset the same condition.");
                    _avoidRecursionLogSpam = true;
                }
                return;
            }

            foreach (var check in _checks)
            {
                bool checkPassed = ConditionalsHandler.Check(check.check);
                ConditionalsHandler.ApplyEffects(check.then, checkPassed);
            }
            _recursionCount++;
            // Allow Update() to run
            enabled = true;
        }

        // We schedule checks for the end of the frame instead of doing them immediately because GlobalMessenger doesn't support recursion and will throw an error if we update a condition that fires off another GlobalMessenger event
        void ScheduleChecks()
        {
            _checksScheduled = true;
            // Allow Update() to run
            enabled = true;
        }

        // We could theoretically filter checks by conditionName here and only do the checks that matter, but the performance gain is likely not significant enough to be worth the extra complexity
        void ScheduleChecks(string conditionName, bool conditionState)
        {
            ScheduleChecks();
        }
    }
}
