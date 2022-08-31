using UnityEngine;

namespace NewHorizons.Components
{
    [RequireComponent(typeof(OWTriggerVolume))]
    public abstract class BaseVolume : MonoBehaviour
    {
        private OWTriggerVolume _triggerVolume;

        public virtual void Awake()
        {
            _triggerVolume = this.GetRequiredComponent<OWTriggerVolume>();
            _triggerVolume.OnEntry += OnTriggerVolumeEntry;
            _triggerVolume.OnExit += OnTriggerVolumeExit;
        }

        public virtual void OnDestroy()
        {
            if (_triggerVolume == null) return;
            _triggerVolume.OnEntry -= OnTriggerVolumeEntry;
            _triggerVolume.OnExit -= OnTriggerVolumeExit;
        }

        public abstract void OnTriggerVolumeEntry(GameObject hitObj);

        public abstract void OnTriggerVolumeExit(GameObject hitObj);
    }
}
