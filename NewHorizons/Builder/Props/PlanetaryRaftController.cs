using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public class PlanetaryRaftController : MonoBehaviour
    {
		private LightSensor[] _lightSensors;
		private float _acceleration = 5f;
		private Vector3 _localAcceleration = Vector3.zero;
		private OWRigidbody _raftBody;

		private void Awake()
        {
			this._raftBody = base.GetComponent<OWRigidbody>();
			_lightSensors = GetComponentsInChildren<LightSensor>();
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
			if (this._localAcceleration.sqrMagnitude > 0.001f)
			{
				this._raftBody.AddLocalAcceleration(this._localAcceleration);
			}
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
