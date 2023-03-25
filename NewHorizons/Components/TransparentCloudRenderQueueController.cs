using UnityEngine;

namespace NewHorizons.Components
{
    [RequireComponent(typeof(OWTriggerVolume))]
    public class TransparentCloudRenderQueueController : MonoBehaviour
    {
        public int insideQueue = 3001;
        public int outsideQueue = 2999;

        private OWTriggerVolume _triggerVolume;
        public Renderer renderer;

        public void Awake()
        {
            _triggerVolume = this.GetRequiredComponent<OWTriggerVolume>();
            if (_triggerVolume == null) return;
            _triggerVolume.OnEntry += OnTriggerVolumeEntry;
            _triggerVolume.OnExit += OnTriggerVolumeExit;
        }

        public void OnDestroy()
        {
            if (_triggerVolume == null) return;
            _triggerVolume.OnEntry -= OnTriggerVolumeEntry;
            _triggerVolume.OnExit -= OnTriggerVolumeExit;
        }

        public void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector")) SetQueueToInside();
        }

        public void OnTriggerVolumeExit(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector")) SetQueueToOutside();
        }

        public void SetQueueToInside()
        {
            if (renderer == null) return;
            renderer.sharedMaterial.renderQueue = insideQueue;
        }

        public void SetQueueToOutside()
        {
            if (renderer == null) return;
            renderer.sharedMaterial.renderQueue = outsideQueue;
        }
    }
}
