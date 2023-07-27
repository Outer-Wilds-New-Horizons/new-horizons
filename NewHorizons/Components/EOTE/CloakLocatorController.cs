using NewHorizons.Components.Stars;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components.EOTE
{
    internal class CloakLocatorController : MonoBehaviour
    {
        private float _currentAngle = float.MaxValue;
        private CloakFieldController _currentController;
        
        public void Start()
        {
            if (CloakHandler.Cloaks.Any())
            {
                // Enable and disable all cloaks, else Stranger state is weird at the start
                foreach (var cloak in CloakHandler.Cloaks)
                {
                    SetCurrentCloak(cloak);
                    cloak.enabled = false;
                }

                // Make sure a cloak is enabled
                SetCurrentCloak(CloakHandler.Cloaks.First());
            }
        }

        // Always makes sure the Locator's cloak field controller is the one that is between the player and the sun
        public void Update()
        {
            var sun = SunLightEffectsController.Instance?.transform;
            if (sun != null)
            {
                // Keep tracking the angle to the current cloak
                if (_currentController != null)
                {
                    _currentAngle = CalculateAngleToCloak(_currentController.transform, sun);
                }
                
                // Compare the current cloak to all the other ones
                foreach (var cloak in CloakHandler.Cloaks)
                {
                    if (cloak == _currentController) continue;

                    var angle = CalculateAngleToCloak(cloak.transform, sun);

                    if (angle < _currentAngle && cloak != _currentController)
                    {
                        _currentAngle = angle;
                        SetCurrentCloak(cloak);
                        NHLogger.LogVerbose($"Changed cloak controller to {_currentController.GetAttachedOWRigidbody().name} angle {_currentAngle}");
                    }
                }
            }
        }

        public void SetCurrentCloak(CloakFieldController cloak)
        {
            if (_currentController != null)
            {
                _currentController.enabled = false;
            }

            _currentController = cloak;

            if (_currentController != null)
            {
                _currentController.enabled = true;
                Locator.RegisterCloakFieldController(_currentController);
                _currentController.UpdateCloakVisualsState();
            }
        }

        private float CalculateAngleToCloak(Transform cloak, Transform sun)
        {
            var playerVector = Locator.GetPlayerTransform().position - sun.position;
            var cloakVector = cloak.position - sun.position;

            return Vector3.Angle(playerVector, cloakVector);
        }
    }
}
