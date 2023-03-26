using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Components.SizeControllers
{
    public class CometTailController : SizeController
    {
        private Transform _primaryBody;
        private OWRigidbody _body;

        private bool _hasRotationOverride;
        private bool _hasPrimaryBody;

        public GameObject gasTail;
        public GameObject dustTail;

        private Vector3 _gasTarget;
        private Vector3 _dustTarget;

        public void Start()
        {
            _body = transform.GetAttachedOWRigidbody();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!_hasRotationOverride && _hasPrimaryBody)
            {
                UpdateTargetPositions();

                dustTail?.LookDir(_dustTarget);
                gasTail?.LookDir(_gasTarget);
            }
        }

        private void UpdateTargetPositions()
        {
            var toPrimary = (_body.transform.position - _primaryBody.transform.position).normalized;
            var velocityDirection = -_body.GetVelocity(); // Accept that this is flipped ok

            var tangentVel = Vector3.ProjectOnPlane(velocityDirection, toPrimary) / velocityDirection.magnitude;

            _gasTarget = toPrimary;
            _dustTarget = (toPrimary + tangentVel).normalized;
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
