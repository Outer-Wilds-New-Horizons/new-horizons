using UnityEngine;

namespace NewHorizons.Components
{
    public class NomaiWarpTransmitterCooldown : MonoBehaviour
    {
        private NomaiWarpTransmitter _transmitter;
        private NomaiWarpReceiver _receiver;

        private float _reenabledTime = 0f;
        private bool _cooldownActive;

        public void Start()
        {
            _transmitter = GetComponent<NomaiWarpTransmitter>();
            _transmitter.OnReceiveWarpedBody += _transmitter_OnReceiveWarpedBody;
        }

        public void OnDestroy()
        {
            if (_transmitter != null)
            {
                _transmitter.OnReceiveWarpedBody -= _transmitter_OnReceiveWarpedBody;
            }
        }

        private void _transmitter_OnReceiveWarpedBody(OWRigidbody warpedBody, NomaiWarpPlatform startPlatform, NomaiWarpPlatform targetPlatform)
        {
            _cooldownActive = true;

            _reenabledTime = Time.time + 5f;
            _receiver = _transmitter._targetReceiver;
            _transmitter._targetReceiver = null;
        }

        public void FixedUpdate()
        {
            if (_cooldownActive && Time.time > _reenabledTime)
            {
                _cooldownActive = false;
                _transmitter._targetReceiver = _receiver;
            }
        }
    }
}
