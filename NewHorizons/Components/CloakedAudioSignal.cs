using UnityEngine;
namespace NewHorizons.Components
{
    public class CloakedAudioSignal : AudioSignal
    {
        public new void UpdateSignalStrength(Signalscope scope, float distToClosestScopeObstruction)
        {
            this._canBePickedUpByScope = false;
            if (!PlayerState.InCloakingField())
            {
                this._signalStrength = 0f;
                this._degreesFromScope = 180f;
                return;
            }
            if (this._sunController != null && this._sunController.IsPointInsideSupernova(base.transform.position))
            {
                this._signalStrength = 0f;
                this._degreesFromScope = 180f;
                return;
            }
            if (Locator.GetQuantumMoon() != null && Locator.GetQuantumMoon().IsPlayerInside() && this._name != SignalName.Quantum_QM)
            {
                this._signalStrength = 0f;
                this._degreesFromScope = 180f;
                return;
            }
            if (!this._active || !base.gameObject.activeInHierarchy || this._outerFogWarpVolume != PlayerState.GetOuterFogWarpVolume() || (scope.GetFrequencyFilter() & this._frequency) != this._frequency)
            {
                this._signalStrength = 0f;
                this._degreesFromScope = 180f;
                return;
            }
            this._scopeToSignal = base.transform.position - scope.transform.position;
            this._distToScope = this._scopeToSignal.magnitude;
            if (this._outerFogWarpVolume == null && distToClosestScopeObstruction < 1000f && this._distToScope > 1000f)
            {
                this._signalStrength = 0f;
                this._degreesFromScope = 180f;
                return;
            }
            this._canBePickedUpByScope = true;
            if (this._distToScope < this._sourceRadius)
            {
                this._signalStrength = 1f;
            }
            else
            {
                this._degreesFromScope = Vector3.Angle(scope.GetScopeDirection(), this._scopeToSignal);
                float t = Mathf.InverseLerp(2000f, 1000f, this._distToScope);
                float a = Mathf.Lerp(45f, 90f, t);
                float a2 = 57.29578f * Mathf.Atan2(this._sourceRadius, this._distToScope);
                float b = Mathf.Lerp(Mathf.Max(a2, 5f), Mathf.Max(a2, 1f), scope.GetZoomFraction());
                this._signalStrength = Mathf.Clamp01(Mathf.InverseLerp(a, b, this._degreesFromScope));
            }
            if (this._distToScope < this._identificationDistance + this._sourceRadius && this._signalStrength > 0.9f)
            {
                if (!PlayerData.KnowsFrequency(this._frequency) && !this._preventIdentification)
                {
                    this.IdentifyFrequency();
                }
                if (!PlayerData.KnowsSignal(this._name) && !this._preventIdentification)
                {
                    this.IdentifySignal();
                }
                if (this._revealFactID.Length > 0)
                {
                    Locator.GetShipLogManager().RevealFact(this._revealFactID, true, true);
                }
            }
        }
    }
}
