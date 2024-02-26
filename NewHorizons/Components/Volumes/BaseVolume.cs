using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    [RequireComponent(typeof(OWTriggerVolume))]
    public abstract class BaseVolume : SectoredMonoBehaviour
    {
        private OWTriggerVolume _triggerVolume;

        public override void Awake()
        {
            base.Awake();
            _triggerVolume = this.GetRequiredComponent<OWTriggerVolume>();
            _triggerVolume.OnEntry += OnTriggerVolumeEntry;
            _triggerVolume.OnExit += OnTriggerVolumeExit;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (_triggerVolume != null)
            {
                _triggerVolume.OnEntry -= OnTriggerVolumeEntry;
                _triggerVolume.OnExit -= OnTriggerVolumeExit;
            }
        }

        public abstract void OnTriggerVolumeEntry(GameObject hitObj);

        public abstract void OnTriggerVolumeExit(GameObject hitObj);
    }
}
