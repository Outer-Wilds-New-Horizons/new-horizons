using NewHorizons.Builder.General;
using NewHorizons.Components.Orbital;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewHorizons.Components.Quantum
{
    public class QuantumPlanet : QuantumObject
    {
        public List<State> states = new List<State>();
        public State groundState;

        private NHAstroObject _astroObject;
        private ConstantForceDetector _detector;
        private AlignWithTargetBody _alignment;
        private OWRigidbody _rb;
        private NHOrbitLine _nhOrbitLine;
        private TrackingOrbitLine _trackingOrbitLine;
        private QuantumStructure[] _structures = new QuantumStructure[0];
        private QuantumDarkTrigger[] _darkTriggers = new QuantumDarkTrigger[0];
        private int _collapseToState = -1;

        public NHAstroObject astroObject
        {
            get
            {
                if (_astroObject == null)
                    _astroObject = GetComponent<NHAstroObject>();
                return _astroObject;
            }
        }

        public OWRigidbody rb
        {
            get
            {
                if (_rb == null)
                    _rb = GetComponent<OWRigidbody>();
                return _rb;
            }
        }

        public int LastIndex { get; private set; }
        public int CurrentIndex { get; private set; }

        public State LastState => states[LastIndex];
        public State CurrentState => states[CurrentIndex];

        public override void Awake()
        {
            base.Awake();

            _astroObject = GetComponent<NHAstroObject>();
            _detector = GetComponentInChildren<ConstantForceDetector>();
            _alignment = GetComponent<AlignWithTargetBody>();
            _rb = GetComponent<OWRigidbody>();

            GlobalMessenger.AddListener("PlayerBlink", OnPlayerBlink);

            _checkIllumination = true;
            _maxSnapshotLockRange = 300000f;
        }

        public override void Start()
        {
            Delay.FireOnNextUpdate(PostStart);

            base.Start();

            ResetStates(true);
        }

        private void PostStart()
        {
            _nhOrbitLine = GetComponentInChildren<NHOrbitLine>();
            _trackingOrbitLine = GetComponentInChildren<TrackingOrbitLine>();
        }

        public void ResetStates(bool changeState)
        {
            _structures = GetComponentsInChildren<QuantumStructure>(true); // finds all quantum structures
            _darkTriggers = GetComponentsInChildren<QuantumDarkTrigger>(true); // finds all quantum dark triggers

            if (changeState)
            {
                ChangeQuantumState(true);
            }
            else
            {
                SetNewSector(states[CurrentIndex]);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            GlobalMessenger.RemoveListener("PlayerBlink", OnPlayerBlink);
        }

        public int GetRandomNewState()
        {
            var range = Enumerable.Range(0, states.Count).Where(i => i != CurrentIndex);
            var index = Random.Range(0, range.Count());
            return range.ElementAt(index);
        }

        public bool ChangeQuantumState(State state, bool skipInstantVisibilityCheck)
            => ChangeQuantumState(states.IndexOf(state), skipInstantVisibilityCheck);

        public bool ChangeQuantumState(int index, bool skipInstantVisibilityCheck)
        {
            _collapseToState = index;
            return ChangeQuantumState(skipInstantVisibilityCheck);
        }

        public override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
        {
            NHLogger.LogVerbose($"QuantumPlanet - Trying to change quantum state");

            if (states.Count <= 1) return false;

            // Don't move if we have a picture
            if (!skipInstantVisibilityCheck && IsLockedByProbeSnapshot()) return false;

            var canChange = false;

            var oldState = states[CurrentIndex];

            // This will all get set in the for loop
            State newState = oldState;
            int newIndex = CurrentIndex;
            int oldIndex = CurrentIndex;
            AstroObject primaryBody = null;
            OrbitalParameters orbitalParams = null;

            // The QM tries to switch 10 times so we'll do that too
            for (int i = 0; i < 10; i++)
            {
                newIndex = ((_collapseToState != -1) ? _collapseToState : GetRandomNewState());

                newState = states[newIndex];

                // Figure out what the new orbit will be if we switch
                var newOrbit = newState.orbit ?? groundState.orbit;
                newOrbit.trueAnomaly = Random.Range(0f, 360f);

                primaryBody = AstroObjectLocator.GetAstroObject(newOrbit.primaryBody);
                var primaryGravity = new Gravity(primaryBody.GetGravityVolume());
                var secondaryGravity = new Gravity(astroObject.GetGravityVolume());
                orbitalParams = newOrbit.GetOrbitalParameters(primaryGravity, secondaryGravity);

                var pos = primaryBody.transform.position + orbitalParams.InitialPosition;

                // See if we can switch, so we move the visibility tracker to the new position
                _visibilityTrackers[0].transform.position = pos;

                // Only if entangled or not seen
                if (skipInstantVisibilityCheck || IsPlayerEntangled() || !CheckVisibilityInstantly())
                {
                    canChange = true;

                    // Since we were able to change states we break
                    break;
                }
            }

            if (canChange)
            {
                var changeSector = newState.sector != null && newState.sector != oldState.sector;
                if (changeSector) SetNewSector(newState);
                var changeOrbit = newState.orbit != null && newState.orbit != oldState.orbit;
                if (changeOrbit) SetNewOrbit(primaryBody, orbitalParams);

                LastIndex = oldIndex;
                CurrentIndex = newIndex;
                _collapseToState = -1;

                if (changeSector) GlobalMessenger<QuantumPlanet>.FireEvent("QuantumPlanetChangeState", this);
                if (changeOrbit) GlobalMessenger<QuantumPlanet>.FireEvent("QuantumPlanetChangeOrbit", this);
            }

            // Be completely sure we move the visibility tracker back to our planet
            _visibilityTrackers[0].transform.localPosition = Vector3.zero;

            return true;
        }

        private void SetNewSector(State newState)
        {
            foreach (var state in states)
            {
                state.sector?.gameObject?.SetActive(false);
            }

            newState.sector.gameObject.SetActive(true);

            foreach (var structure in _structures)
            {
                structure.OnQuantumPlanetStateChanged(CurrentIndex);
            }
        }

        private void SetNewOrbit(AstroObject primaryBody, OrbitalParameters orbitalParameters)
        {
            astroObject._primaryBody = primaryBody;
            DetectorBuilder.SetDetector(primaryBody, astroObject, _detector);
            _detector._activeInheritedDetector = primaryBody.GetComponentInChildren<ForceDetector>();
            _detector._activeVolumes = new List<EffectVolume>() { primaryBody.GetGravityVolume() };
            if (_alignment != null) _alignment.SetTargetBody(primaryBody.GetComponent<OWRigidbody>());

            astroObject.SetOrbitalParametersFromTrueAnomaly(orbitalParameters.eccentricity, orbitalParameters.semiMajorAxis, orbitalParameters.inclination, orbitalParameters.argumentOfPeriapsis, orbitalParameters.longitudeOfAscendingNode, orbitalParameters.trueAnomaly);

            PlanetCreationHandler.UpdatePosition(gameObject, orbitalParameters, primaryBody, astroObject);
            gameObject.transform.parent = null;

            if (!Physics.autoSyncTransforms)
            {
                Physics.SyncTransforms();
            }

            _rb.SetVelocity(orbitalParameters.InitialVelocity + primaryBody.GetAttachedOWRigidbody().GetVelocity());

            if (_nhOrbitLine != null)
            {
                _nhOrbitLine.SetFromParameters(orbitalParameters);
            }

            if (_trackingOrbitLine != null)
            {
                _trackingOrbitLine.Reset();
            }

            foreach (var structure in _structures)
            {
                structure.OnQuantumPlanetOrbitChanged(CurrentIndex);
            }
        }

        private void OnPlayerBlink()
        {
            if (IsVisible())
            {
                Collapse(true);
            }
        }

        public bool IsPlayerInside()
        {
            if (CurrentIndex >= states.Count) return true;

            return states[CurrentIndex].sector.ContainsAnyOccupants(DynamicOccupant.Player);
        }

        public bool IsProbeInside()
        {
            if (CurrentIndex >= states.Count) return true;

            return states[CurrentIndex].sector.ContainsAnyOccupants(DynamicOccupant.Probe);
        }

        public bool IsShipInside()
        {
            if (CurrentIndex >= states.Count) return true;

            return states[CurrentIndex].sector.ContainsAnyOccupants(DynamicOccupant.Ship);
        }

        public bool IsPlayerInsideStructure()
        {
            foreach (var structure in _structures)
            {
                if (structure.IsPlayerInside())
                    return true;
            }
            return false;
        }

        public bool IsPlayerInDarkness()
        {
            foreach (var darkTrigger in _darkTriggers)
            {
                if (darkTrigger.IsPlayerInDarkness())
                    return true;
            }
            foreach (var structure in _structures)
            {
                if (structure.IsPlayerInDarkness())
                    return true;
            }
            return PlayerState.InDarkZone();
        }

        public override bool CheckIllumination() => !IsPlayerInDarkness();

        public override bool IsPlayerEntangled() => IsPlayerInside();

        internal void AddState(State state)
        {
            states.Add(state);
            state.index = states.IndexOf(state);
            ResetStates(false);
        }

        public class State
        {
            public int index { get; set; } = -1;
            public Sector sector { get; set; }
            public OrbitModule orbit { get; set; }

            public State(Sector sector, OrbitModule orbit)
            {
                this.sector = sector;
                this.orbit = orbit;
            }

            public override string ToString()
            {
                return index.ToString();
            }

            public override bool Equals(object obj)
            {
                if (obj is State state)
                {
                    return state == this;
                }
                return base.Equals(obj);
            }

            public static bool operator ==(State left, State right)
            {
                return left.sector == right.sector && left.orbit == right.orbit && left.index == right.index;
            }

            public static bool operator !=(State left, State right)
            {
                return !(left == right);
            }

            public override int GetHashCode() => sector.GetHashCode();
        }
    }
}
