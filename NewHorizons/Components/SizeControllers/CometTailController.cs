using UnityEngine;

namespace NewHorizons.Components.SizeControllers
{
    public class CometTailController : SizeController
    {
        private Transform _primaryBody;
        private OWRigidbody _body;

        private bool _hasRotationOverride;
        private bool _hasPrimaryBody;

        public void Start()
        {
            _body = transform.GetAttachedOWRigidbody();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!_hasRotationOverride && _hasPrimaryBody)
            {
                transform.LookAt(_primaryBody, _body.GetVelocity().normalized);
            }
        }

        public void SetRotationOverride(Vector3 eulerAngles)
        {
            _hasRotationOverride = true;
            transform.localRotation = Quaternion.Euler(eulerAngles);
        }

        public void SetPrimaryBody(Transform primaryBody)
        {
            _hasPrimaryBody = true;
            _primaryBody = primaryBody;
        }
    }
}
