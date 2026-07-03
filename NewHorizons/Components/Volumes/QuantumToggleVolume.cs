using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    public class QuantumToggleVolume : BaseVolume
    {
        public QuantumObject[] quantumObjects;
        public bool invert;
        public bool undoOnExit = true;

        public void Start()
        {
            SetIsQuantum(invert);
        }

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                SetIsQuantum(!invert);
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj)
        {
            if (undoOnExit && hitObj.CompareTag("PlayerDetector"))
            {
                SetIsQuantum(invert);
            }
        }

        public void SetIsQuantum(bool isQuantum)
        {
            foreach (var quantumObject in quantumObjects)
            {
                quantumObject.SetIsQuantum(isQuantum);
            }
        }
    }
}
