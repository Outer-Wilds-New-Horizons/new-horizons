using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Components.Quantum
{
    public class NHQuantumShrine : QuantumStructure
    {
        [SerializeField]
        public NomaiGateway gate;

        [SerializeField]
        public MeshRenderer floorRenderer;

        [SerializeField]
        public Material[] floorMaterials = new Material[0];

        [SerializeField]
        public OWRenderer exteriorRenderer;

        [SerializeField]
        public NomaiInterfaceOrb[] childOrbs = new NomaiInterfaceOrb[0];

        [SerializeField]
        public OWLightController exteriorLightController;

        private bool _orbsLocked;

        public override void OnQuantumPlanetStateChanged(int state)
        {
            if (exteriorRenderer != null)
            {
                exteriorRenderer.SetActivation(state != -1);
            }
            if (!_orbsLocked && state == -1)
            {
                _orbsLocked = true;
                foreach (var childOrb in childOrbs)
                {
                    childOrb.AddLock();
                }
            }
            else if (_orbsLocked && state != -1)
            {
                _orbsLocked = false;
                foreach (var childOrb in childOrbs)
                {
                    childOrb.RemoveLock();
                }
            }
            if (floorRenderer != null && state != -1)
            {
                floorRenderer.material = floorMaterials[state];
            }
        }

        public override float GetOpenFraction()
        {
            return gate != null ? gate.GetOpenFraction() : base.GetOpenFraction();
        }

        public override void OnPlayerEntry(GameObject playerDetector)
        {
            if (exteriorLightController != null)
            {
                exteriorLightController.FadeTo(0f, 1f);
            }
        }

        public override void OnPlayerExit(GameObject playerDetector)
        {
            if (exteriorLightController != null)
            {
                exteriorLightController.FadeTo(1f, 1f);
            }
        }
    }
}
