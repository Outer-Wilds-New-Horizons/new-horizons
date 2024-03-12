using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    /// <summary>
    /// Currently only relevant for Vanilla planets, which actually have streaming groups
    /// </summary>
    internal class StreamingWarpVolume : BaseVolume
    {
        public StreamingGroup streamingGroup;
        private bool _probeInVolume;
        private bool _playerInVolume;
        private bool _preloadingRequiredAssets;
        private bool _preloadingGeneralAssets;

        private SurveyorProbe _probe;

        public void Start()
        {
            _probe = Locator.GetProbe();
            base.enabled = false;
        }

        public void FixedUpdate()
        {
            // Bug report on Astral Codec mod page - Huge lag inside streaming warp volume, possible NRE?
            if (_probe == null)
            {
                _probe = Locator.GetProbe();
                if (_probe == null)
                {
                    NHLogger.LogError($"How is your scout probe null? Destroying {nameof(StreamingWarpVolume)}");
                    GameObject.DestroyImmediate(gameObject);
                }
            }

            if (streamingGroup == null)
            {
                NHLogger.LogError($"{nameof(StreamingWarpVolume)} has no streaming group. Destroying {nameof(StreamingWarpVolume)}");
                GameObject.DestroyImmediate(gameObject);
            }

            bool probeActive = _probe.IsLaunched() && !_probe.IsAnchored();

            bool shouldBeLoadingRequiredAssets = _playerInVolume || (_probeInVolume && probeActive);
            bool shouldBeLoadingGeneralAssets = _playerInVolume;

            UpdatePreloadingState(shouldBeLoadingRequiredAssets, shouldBeLoadingGeneralAssets);
        }

        private void UpdatePreloadingState(bool shouldBeLoadingRequiredAssets, bool shouldBeLoadingGeneralAssets)
        {
            if (!this._preloadingRequiredAssets && shouldBeLoadingRequiredAssets)
            {
                this.streamingGroup.RequestRequiredAssets(0);
                this._preloadingRequiredAssets = true;
            }
            else if (this._preloadingRequiredAssets && !shouldBeLoadingRequiredAssets)
            {
                this.streamingGroup.ReleaseRequiredAssets();
                this._preloadingRequiredAssets = false;
            }
            if (!this._preloadingGeneralAssets && shouldBeLoadingGeneralAssets)
            {
                this.streamingGroup.RequestGeneralAssets(0);
                this._preloadingGeneralAssets = true;
                return;
            }
            if (this._preloadingGeneralAssets && !shouldBeLoadingGeneralAssets)
            {
                this.streamingGroup.ReleaseGeneralAssets();
                this._preloadingGeneralAssets = false;
            }
        }

        public override void OnSectorOccupantsUpdated()
        {
            if (this._sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
            {
                if (StreamingManager.isStreamingEnabled && this.streamingGroup != null)
                {
                    base.enabled = true;
                    return;
                }
            }
            else
            {
                this.UpdatePreloadingState(false, false);
                base.enabled = false;
            }
        }

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody(false);
            if (attachedOWRigidbody != null)
            {
                if (attachedOWRigidbody.CompareTag("Player"))
                {
                    this._playerInVolume = true;
                    return;
                }
                if (attachedOWRigidbody.CompareTag("Probe"))
                {
                    this._probeInVolume = true;
                }
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj)
        {
            OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody(false);
            if (attachedOWRigidbody != null)
            {
                if (attachedOWRigidbody.CompareTag("Player"))
                {
                    this._playerInVolume = false;
                    return;
                }
                if (attachedOWRigidbody.CompareTag("Probe"))
                {
                    this._probeInVolume = false;
                }
            }
        }
    }
}
