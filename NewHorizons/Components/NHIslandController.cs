using HarmonyLib;
using OWML.Utils;
using System;
using System.Reflection;
using UnityEngine;

namespace NewHorizons.Components
{
    public class NHIslandController : IslandController
    {
        public float maxDistance = 900f;
        public Campfire[] _campfires;
        public OWRigidbody _planetBody;

        private MulticastDelegate IslandSplashDelegate;
        private MulticastDelegate IslandEnteredTornadoDelegate;
        private MulticastDelegate IslandApexDelegate;

        private new void Awake()
        {
            base.Awake();

            IslandSplashDelegate = (MulticastDelegate)AccessTools.Field(typeof(IslandController), "OnIslandSplashEvent").GetValue(this);
            IslandEnteredTornadoDelegate = (MulticastDelegate)AccessTools.Field(typeof(IslandController), "OnIslandEnteredTornadoEvent").GetValue(this);
            IslandApexDelegate = (MulticastDelegate)AccessTools.Field(typeof(IslandController), "OnIslandApexEvent").GetValue(this);

            OnIslandApexEvent += () => Utility.Logger.Log("Apex");
            OnIslandEnteredTornadoEvent += () => Utility.Logger.Log("Entered Tornado");
            OnIslandSplashEvent += () => Utility.Logger.Log("Splash");
        }

        private new void Start()
        {
            if (_planetTransform == null) _planetTransform = this.GetAttachedOWRigidbody().GetOrigParentBody().transform;
            if (_planetBody == null) _planetBody = _planetTransform.GetAttachedOWRigidbody();
            _zeroGVolume.SetVolumeActivation(false);
            _inheritanceFluid.SetVolumeActivation(false);
            SetSafetyBeamActivation(false);
        }

        public void OnIslandSplash()
        {
            if (IslandSplashDelegate != null)
            {
                foreach (var handler in IslandSplashDelegate.GetInvocationList())
                    handler.Method.Invoke(handler.Target, new object[1] {this});
            }
        }

        public void OnIslandEnteredTornado()
        {
            if (IslandEnteredTornadoDelegate != null)
            {
                foreach (var handler in IslandEnteredTornadoDelegate.GetInvocationList())
                    handler.Method.Invoke(handler.Target, new object[1] { this });
            }
        }

        public void OnIslandApex()
        {
            if (IslandApexDelegate != null)
            {
                foreach (var handler in IslandApexDelegate.GetInvocationList())
                    handler.Method.Invoke(handler.Target, new object[1] { this });
            }
        }

        private new void FixedUpdate()
        {
            Vector3 dir = _transform.position - _planetTransform.position;
            float distance = Vector3.Distance(_planetTransform.position, _transform.position);
            if (_safetyTractorBeams != null)
            {
                Vector3 project = Vector3.Project(_islandBody.GetRelativeVelocity(_planetBody), dir);
                if (!_tractorBeamsActive && _fluidDetector.InFluidType(FluidVolume.Type.CLOUD))
                    SetSafetyBeamActivation(true);
                else if (_tractorBeamsActive && !_fluidDetector.InFluidType(FluidVolume.Type.CLOUD) && _fluidDetector.InFluidType(FluidVolume.Type.WATER) && (project.magnitude * -Mathf.Sign(Vector3.Dot(project, dir))) > 0)
                    SetSafetyBeamActivation(false);
            }
            if (_fluidDetector.InFluidType(FluidVolume.Type.CLOUD) && !_inheritanceFluid.IsVolumeActive())
            {
                _inheritanceFluid.SetVolumeActivation(true);
                OnIslandEnteredTornado();
            }
            else if (distance < maxDistance && !_fluidDetector.InFluidType(FluidVolume.Type.CLOUD) && _inheritanceFluid.IsVolumeActive())
                _inheritanceFluid.SetVolumeActivation(false);
            if (distance >= maxDistance && !_zeroGVolume.IsVolumeActive())
            {
                OnIslandApex();
                _zeroGVolume.SetVolumeActivation(true);
                if (_campfire != null)
                {
                    _campfire.StopRoasting();
                    _campfire.StopSleeping(true);
                    _campfire.SetInteractionEnabled(false);
                }
                if (_campfires != null)
                {
                    foreach (var campfire in _campfires)
                    {
                        campfire.StopRoasting();
                        campfire.StopSleeping(true);
                        campfire.SetInteractionEnabled(false);
                    }
                }
            }
            else if (distance < maxDistance && _zeroGVolume.IsVolumeActive())
            {
                _zeroGVolume.SetVolumeActivation(false);

                if (_campfire != null)
                    _campfire.SetInteractionEnabled(true);

                if (_campfires != null)
                {
                    foreach (var campfire in _campfires)
                    {
                        campfire.SetInteractionEnabled(true);
                    }
                }
            }

            if (_repelFluidsActive && _fluidDetector.InFluidType(FluidVolume.Type.CLOUD))
            {
                for (int index = 0; index < _barrierRepelFluids.Length; ++index)
                    _barrierRepelFluids[index].SetVolumeActivation(false);
                _repelFluidsActive = false;
            }
            else
            {
                if (_repelFluidsActive || !_fluidDetector.InFluidType(FluidVolume.Type.WATER) || _fluidDetector.InFluidType(FluidVolume.Type.CLOUD)) return;
                for (int index = 0; index < _barrierRepelFluids.Length; ++index)
                    _barrierRepelFluids[index].SetVolumeActivation(true);
                _repelFluidsActive = true;
            }
        }
    }
}
