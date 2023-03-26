using UnityEngine;

namespace NewHorizons.Components.Vessel
{
    [UsedInUnityProject]
    public class VesselOrbLocker : MonoBehaviour
    {
        public GameObject _coordinateInterfaceOrbObject;
        private NomaiInterfaceOrb _coordinateInterfaceOrb;

        public GameObject _coordinateInterfaceUpperOrbObject;
        private NomaiInterfaceOrb _coordinateInterfaceUpperOrb;

        public GameObject _powerOrbObject;
        private NomaiInterfaceOrb _powerOrb;

        private void Awake() => InitializeOrbs();

        public void InitializeOrbs()
        {
            _coordinateInterfaceOrb = _coordinateInterfaceOrbObject.GetComponent<NomaiInterfaceOrb>();
            _coordinateInterfaceUpperOrb = _coordinateInterfaceUpperOrbObject.GetComponent<NomaiInterfaceOrb>();
            _powerOrb = _powerOrbObject.GetComponent<NomaiInterfaceOrb>();
        }

        public void AddLocks()
        {
            _coordinateInterfaceOrb.AddLock();
            _coordinateInterfaceUpperOrb.AddLock();
            _powerOrb.AddLock();
        }

        public void AddLockToCoordinateOrb()
        {
            _coordinateInterfaceOrb.AddLock();
        }

        public void AddLockToWarpOrb()
        {
            _coordinateInterfaceUpperOrb.AddLock();
        }

        public void AddLockToPowerOrb()
        {
            _powerOrb.AddLock();
        }

        public void RemoveLocks()
        {
            _coordinateInterfaceOrb.RemoveAllLocks();
            _coordinateInterfaceUpperOrb.RemoveAllLocks();
            _powerOrb.RemoveAllLocks();
        }
    }
}
