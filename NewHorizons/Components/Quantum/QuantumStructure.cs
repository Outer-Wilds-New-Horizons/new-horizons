using NewHorizons.Utility.OWML;
using System;
using UnityEngine;

namespace NewHorizons.Components.Quantum
{
    public class QuantumStructure : MonoBehaviour
    {
        [SerializeField]
        public OWTriggerVolume insideTriggerVolume;

        [SerializeField]
        public Light[] lights = new Light[0];

        [SerializeField]
        public Light ambientLight;

        [SerializeField]
        public FogOverrideVolume fogOverride;

        protected float fadeFraction { get; private set; } = 1f;

        protected bool isPlayerInside { get; private set; }

        protected bool isProbeInside { get; private set; }

        protected bool fading { get; private set; }

        private float _origAmbientIntensity;

        private Color _origFogTint;

        public virtual void Awake()
        {
            if (ambientLight != null)
            {
                _origAmbientIntensity = ambientLight.intensity;
            }
            if (fogOverride != null)
            {
                _origFogTint = fogOverride.tint;
            }
            if (insideTriggerVolume != null)
            {
                insideTriggerVolume.OnEntry += OnEntry;
                insideTriggerVolume.OnExit += OnExit;
            }
            else
            {
                NHLogger.LogWarning("Missing inside trigger volume for " + this.GetType());
            }
        }

        public virtual void Start()
        {
        }

        public virtual void OnDestroy()
        {
            if (insideTriggerVolume != null)
            {
                insideTriggerVolume.OnEntry -= OnEntry;
                insideTriggerVolume.OnExit -= OnExit;
            }
        }

        public virtual bool IsPlayerInside()
        {
            return isPlayerInside;
        }

        public virtual bool IsProbeInside()
        {
            return isProbeInside;
        }

        public virtual bool IsPlayerInDarkness()
        {
            foreach (var light in lights)
            {
                if (light.intensity > 0f)
                {
                    return false;
                }
            }
            if (isPlayerInside && fadeFraction == 0f && !isProbeInside && !PlayerState.IsFlashlightOn())
            {
                return Locator.GetThrusterLightTracker().GetLightRange() <= 0f;
            }
            return false;
        }

        public virtual void OnQuantumPlanetStateChanged(int currentIndex)
        {
        }

        public virtual void OnQuantumPlanetOrbitChanged(int state)
        {
        }

        public virtual void Update()
        {
            if (fading)
            {
                float target = (isPlayerInside ? (GetOpenFraction() * 0.7f) : 1f);
                fadeFraction = Mathf.MoveTowards(fadeFraction, target, Time.deltaTime * 0.5f);
                FadeUpdate();
            }
        }

        public virtual float GetOpenFraction()
        {
            return 1f;
        }

        public virtual void FadeUpdate()
        {
            if (ambientLight != null)
            {
                ambientLight.intensity = _origAmbientIntensity * fadeFraction;
            }
            if (fogOverride != null)
            {
                fogOverride.tint = Color.Lerp(Color.black, _origFogTint, fadeFraction);
            }
        }

        public void OnEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                isPlayerInside = true;
                fading = true;
                OnPlayerEntry(hitObj);
            }
            else if (hitObj.CompareTag("ProbeDetector"))
            {
                isProbeInside = true;
                OnProbeEntry(hitObj);
            }
            else
            {
                OnOtherEntry(hitObj);
            }
        }

        public virtual void OnPlayerEntry(GameObject playerDetector)
        {
        }

        public virtual void OnProbeEntry(GameObject probeDetector)
        {
        }

        public virtual void OnOtherEntry(GameObject otherDetector)
        {
        }

        public void OnExit(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                isPlayerInside = false;
                fading = true;
                OnPlayerExit(hitObj);
            }
            else if (hitObj.CompareTag("ProbeDetector"))
            {
                isProbeInside = false;
                OnProbeExit(hitObj);
            }
            else
            {
                OnOtherExit(hitObj);
            }
        }

        public virtual void OnPlayerExit(GameObject playerDetector)
        {
        }

        public virtual void OnProbeExit(GameObject probeDetector)
        {
        }

        public virtual void OnOtherExit(GameObject otherDetector)
        {
        }
    }
}
