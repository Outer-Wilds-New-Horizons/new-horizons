using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public class PlanetaryRaftController : MonoBehaviour
    {
		private LightSensor[] _lightSensors;
		private float _acceleration = 5f;
		private Vector3 _localAcceleration = Vector3.zero;
		private OWRigidbody _raftBody;
		private Sector _sector;
		private GameObject parentBody;
		//private GravityVolume gravityVolume;

		public float BuoyancyModifier = 5f;

		private Vector3 initialPosition;
		private float targetRadius;

        private void Awake()
        {
            this._raftBody = base.GetComponent<OWRigidbody>();
            _lightSensors = GetComponentsInChildren<LightSensor>();

			//gravity = (GetComponentInChildren<ConstantForceDetector>()._activeVolumes[0] as GravityVolume);
			//gravityVolume = _sector.transform.parent.GetComponentInChildren<GravityVolume>();

            initialPosition = transform.position - _sector.transform.position;
            targetRadius = initialPosition.magnitude;
			parentBody = _sector.transform.parent.gameObject;
			Logger.Log($"{targetRadius}");
        }

        public void OnDestroy()
        {
			if(_sector != null)
            {
				_sector.OnOccupantEnterSector += OnOccupantEnterSector;
				_sector.OnOccupantExitSector += OnOccupantExitSector;
			}
        }

		private void FixedUpdate()
        {
			if (this._lightSensors[0].IsIlluminated())
			{
				this._localAcceleration += Vector3.forward * this._acceleration;
			}
			if (this._lightSensors[1].IsIlluminated())
			{
				this._localAcceleration += Vector3.right * this._acceleration;
			}
			if (this._lightSensors[2].IsIlluminated())
			{
				this._localAcceleration -= Vector3.forward * this._acceleration;
			}
			if (this._lightSensors[3].IsIlluminated())
			{
				this._localAcceleration -= Vector3.right * this._acceleration;
			}

			/*
			var currentRadius = GetRelativePosition().magnitude;
			if(currentRadius != targetRadius)
            {
				float g = 1f;
				var r = (currentRadius / targetRadius);
				//if (gravityVolume != null) g = gravityVolume.CalculateGravityMagnitude(currentRadius);
				_raftBody.AddAcceleration((1f - Mathf.Sqrt(r)) * GetRelativePosition().normalized * g * BuoyancyModifier);
            }
			*/

			/*
			Vector3 vector2 = _raftBody.GetVelocity();
			var normalVector = GetRelativePosition().normalized;
			float normalComponent2 = Vector3.Dot(vector2, normalVector);
			vector2 -= normalComponent2 * normalVector;
			_raftBody.SetVelocity(vector2);
			_raftBody.SetPosition(_sector.transform.position + normalVector * targetRadius);
			*/

			if (this._localAcceleration.sqrMagnitude > 0.001f)
			{
				this._raftBody.AddLocalAcceleration(this._localAcceleration);
			}
			this._localAcceleration = Vector3.zero;
			
		}

		private Vector3 GetRelativePosition()
        {
			return transform.position - parentBody.transform.position;
		}

		public void SetSector(Sector sector)
        {
			_sector = sector;
			_sector.OnOccupantEnterSector += OnOccupantEnterSector;
			_sector.OnOccupantExitSector += OnOccupantExitSector;
        }

		private void OnOccupantEnterSector(SectorDetector sectorDetector)
		{
			if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
			{
				_raftBody.Unsuspend();
			}
		}

		private void OnOccupantExitSector(SectorDetector sectorDetector)
		{
			if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
			{
				_raftBody.Suspend();
			}
		}
	}
}
