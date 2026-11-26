using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    public class NHInteractionVolume : MonoBehaviour
    {
        public bool Reusable { get; set; }
        public string Condition { get; set; }
        public bool Persistent { get; set; }
        public Animator TargetAnimator { get; set; }
        public string AnimationTrigger { get; set; }

        InteractReceiver _interactReceiver;
        OWAudioSource _audioSource;

        protected void Awake()
        {
            _interactReceiver = GetComponent<InteractReceiver>();
            _audioSource = GetComponent<OWAudioSource>();

            _interactReceiver.OnPressInteract += OnInteract;
        }

        protected void OnDestroy()
        {
            _interactReceiver.OnPressInteract -= OnInteract;
        }

        protected void OnInteract()
        {
            if (!string.IsNullOrEmpty(Condition))
            {
                if (Persistent)
                {
                    PlayerData.SetPersistentCondition(Condition, true);
                }
                else
                {
                    DialogueConditionManager.SharedInstance.SetConditionState(Condition, true);
                }
            }

            if (_audioSource != null)
            {
                _audioSource.Play();
            }

            if (TargetAnimator)
            {
                TargetAnimator.SetTrigger(AnimationTrigger);
            }

            if (Reusable)
            {
                _interactReceiver.ResetInteraction();
                _interactReceiver.EnableInteraction();
            }
            else
            {
                _interactReceiver.DisableInteraction();
            }
        }
    }
}
