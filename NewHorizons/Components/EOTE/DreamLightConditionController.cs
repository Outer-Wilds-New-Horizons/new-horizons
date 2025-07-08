using NewHorizons.External.Modules.Props.EchoesOfTheEye;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.EOTE
{
    public class DreamLightConditionController : MonoBehaviour
    {
        public string Condition { get; set; }
        public bool Persistent { get; set; }
        public bool Reversible { get; set; }
        public bool OnExtinguish { get; set; }

        DreamObjectProjector _projector;
        DreamCandle _dreamCandle;

        public void SetFromInfo(DreamLightConditionInfo info)
        {
            Condition = info.condition;
            Persistent = info.persistent;
            Reversible = info.reversible;
            OnExtinguish = info.onExtinguish;
        }

        protected void Awake()
        {
            _projector = GetComponent<DreamObjectProjector>();
            _projector.OnProjectorLit.AddListener(OnProjectorLit);
            _projector.OnProjectorExtinguished.AddListener(OnProjectorExtinguished);

            _dreamCandle = GetComponent<DreamCandle>();
            if (_dreamCandle != null)
            {
                _dreamCandle.OnLitStateChanged.AddListener(OnCandleLitStateChanged);
            }
        }

        protected void OnDestroy()
        {
            if (_projector != null)
            {
                _projector.OnProjectorLit.RemoveListener(OnProjectorLit);
                _projector.OnProjectorExtinguished.RemoveListener(OnProjectorExtinguished);
            }
            if (_dreamCandle != null)
            {
                _dreamCandle.OnLitStateChanged.RemoveListener(OnCandleLitStateChanged);
            }
        }

        private void OnProjectorLit()
        {
            HandleCondition(!OnExtinguish);
        }

        private void OnProjectorExtinguished()
        {
            HandleCondition(OnExtinguish);
        }

        private void OnCandleLitStateChanged()
        {
            HandleCondition(OnExtinguish ? !_dreamCandle._lit : _dreamCandle._lit);
        }

        private void HandleCondition(bool shouldSet)
        {
            if (shouldSet || Reversible)
            {
                if (Persistent)
                {
                    PlayerData.SetPersistentCondition(Condition, shouldSet);
                }
                else
                {
                    DialogueConditionManager.SharedInstance.SetConditionState(Condition, shouldSet);
                }
            }
        }
    }
}
