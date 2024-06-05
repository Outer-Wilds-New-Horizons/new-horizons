using UnityEngine;

namespace NewHorizons.Components
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

        public void RemoveLockFromCoordinateOrb()
        {
            _coordinateInterfaceOrb.RemoveLock();
        }

        public void RemoveLockFromWarpOrb()
        {
            _coordinateInterfaceUpperOrb.RemoveLock();
        }

        public void RemoveLockFromPowerOrb()
        {
            _powerOrb.RemoveLock();
        }

        public void RemoveAllLocksFromCoordinateOrb()
        {
            _coordinateInterfaceOrb.RemoveAllLocks();
        }

        public void RemoveAllLocksFromWarpOrb()
        {
            _coordinateInterfaceUpperOrb.RemoveAllLocks();
        }

        public void RemoveAllLocksFromPowerOrb()
        {
            _powerOrb.RemoveAllLocks();
        }

        public void AddLock()
        {
            AddLockToCoordinateOrb();
            AddLockToWarpOrb();
            AddLockToPowerOrb();
        }

        public void RemoveLock()
        {
            RemoveLockFromCoordinateOrb();
            RemoveLockFromWarpOrb();
            RemoveLockFromPowerOrb();
        }

        public void RemoveAllLocks()
        {
            RemoveAllLocksFromCoordinateOrb();
            RemoveAllLocksFromWarpOrb();
            RemoveAllLocksFromPowerOrb();
        }
    }
}
