#region

using UnityEngine;

#endregion

namespace NewHorizons.Components
{
    public class CloakedAudioSignal : AudioSignal
    {
        public new void UpdateSignalStrength(Signalscope scope, float distToClosestScopeObstruction)
        {
            _canBePickedUpByScope = false;
            if (!PlayerState.InCloakingField())
            {
                _signalStrength = 0f;
                _degreesFromScope = 180f;
                return;
            }

            if (_sunController != null && _sunController.IsPointInsideSupernova(transform.position))
            {
                _signalStrength = 0f;
                _degreesFromScope = 180f;
                return;
            }

            if (Locator.GetQuantumMoon() != null && Locator.GetQuantumMoon().IsPlayerInside() &&
                _name != SignalName.Quantum_QM)
            {
                _signalStrength = 0f;
                _degreesFromScope = 180f;
                return;
            }

            if (!_active || !gameObject.activeInHierarchy ||
                _outerFogWarpVolume != PlayerState.GetOuterFogWarpVolume() ||
                (scope.GetFrequencyFilter() & _frequency) != _frequency)
            {
                _signalStrength = 0f;
                _degreesFromScope = 180f;
                return;
            }

            _scopeToSignal = transform.position - scope.transform.position;
            _distToScope = _scopeToSignal.magnitude;
            if (_outerFogWarpVolume == null && distToClosestScopeObstruction < 1000f && _distToScope > 1000f)
            {
                _signalStrength = 0f;
                _degreesFromScope = 180f;
                return;
            }

            _canBePickedUpByScope = true;
            if (_distToScope < _sourceRadius)
            {
                _signalStrength = 1f;
            }
            else
            {
                _degreesFromScope = Vector3.Angle(scope.GetScopeDirection(), _scopeToSignal);
                var t = Mathf.InverseLerp(2000f, 1000f, _distToScope);
                var a = Mathf.Lerp(45f, 90f, t);
                var a2 = 57.29578f * Mathf.Atan2(_sourceRadius, _distToScope);
                var b = Mathf.Lerp(Mathf.Max(a2, 5f), Mathf.Max(a2, 1f), scope.GetZoomFraction());
                _signalStrength = Mathf.Clamp01(Mathf.InverseLerp(a, b, _degreesFromScope));
            }

            if (_distToScope < _identificationDistance + _sourceRadius && _signalStrength > 0.9f)
            {
                if (!PlayerData.KnowsFrequency(_frequency) && !_preventIdentification) IdentifyFrequency();
                if (!PlayerData.KnowsSignal(_name) && !_preventIdentification) IdentifySignal();
                if (_revealFactID.Length > 0) Locator.GetShipLogManager().RevealFact(_revealFactID);
            }
        }
    }
}