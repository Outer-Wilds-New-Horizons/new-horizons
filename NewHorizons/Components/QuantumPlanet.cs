using NewHorizons.Builder.General;
using NewHorizons.Builder.Orbital;
using NewHorizons.Components.Orbital;
using NewHorizons.External;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Components
{
    public class QuantumPlanet : QuantumObject
    {
        public List<State> states = new List<State>();

        private int _currentIndex;
        private NHAstroObject _astroObject;
        private ConstantForceDetector _detector;
        private AlignWithTargetBody _alignment;
        private OWRigidbody _rb;

        public override void Awake()
        {
            base.Awake();

            _astroObject = GetComponent<NHAstroObject>();
            _detector = GetComponentInChildren<ConstantForceDetector>();
            _alignment = GetComponent<AlignWithTargetBody>();
            _rb = GetComponent<OWRigidbody>();
        }

        public override void Start()
        {
            base.Start();

            foreach(var state in states)
            {
                state.sector?.gameObject?.SetActive(false);
            }

            ChangeQuantumState(true);
        }

        public override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
        {
            Logger.Log($"Changing quantum state");

            if (states.Count <= 1) return false;

            var newIndex = _currentIndex;
            while(newIndex == _currentIndex)
            {
                newIndex = Random.Range(0, states.Count);
            }

            var oldState = states[_currentIndex];
            var newState = states[newIndex];

            if (newState.sector != null) SetNewSector(oldState, newState);
            if(newState.orbit != null) SetNewOrbit(newState);

            _currentIndex = newIndex;

            return true;
        }

        private void SetNewSector(State oldState, State newState)
        {
            oldState.sector.gameObject.SetActive(false);
            newState.sector.gameObject.SetActive(true);
        }

        private void SetNewOrbit(State state)
        {
            var currentOrbit = state.orbit;

            var primaryBody = AstroObjectLocator.GetAstroObject(currentOrbit.PrimaryBody);

            _astroObject._primaryBody = primaryBody;
            DetectorBuilder.SetDetector(primaryBody, _astroObject, _detector);
            if (_alignment != null) _alignment.SetTargetBody(primaryBody.GetComponent<OWRigidbody>());

            PlanetCreationHandler.UpdatePosition(gameObject, currentOrbit, primaryBody, _astroObject);

            var primaryGravity = new Gravity(primaryBody.GetGravityVolume());
            var secondaryGravity = new Gravity(_astroObject.GetGravityVolume());

            _rb.SetVelocity(currentOrbit.GetOrbitalParameters(primaryGravity, secondaryGravity).InitialVelocity);
        }

        public class State
        {
            public Sector sector { get; set; }
            public OrbitModule orbit { get; set; }

            public State(Sector sector, OrbitModule orbit)
            {
                this.sector = sector;
                this.orbit = orbit;
            }
        }
    }
}
