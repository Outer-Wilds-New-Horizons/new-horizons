using UnityEngine;

namespace NewHorizons.Components.EyeOfTheUniverse
{
    public class QuantumInstrumentTrigger : MonoBehaviour
    {
        public string gatherCondition;

        private QuantumInstrument _quantumInstrument;

        private void Awake()
        {
            _quantumInstrument = GetComponent<QuantumInstrument>();
            _quantumInstrument.OnFinishGather += OnFinishGather;
        }

        private void OnDestroy()
        {
            _quantumInstrument.OnFinishGather -= OnFinishGather;
        }

        private void OnFinishGather()
        {
            if (!string.IsNullOrEmpty(gatherCondition))
            {
                DialogueConditionManager.SharedInstance.SetConditionState(gatherCondition, true);
            }
        }
    }
}
