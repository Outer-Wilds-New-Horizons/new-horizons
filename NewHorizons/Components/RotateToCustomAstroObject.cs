using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class RotateToCustomAstroObject : RotateToPoint
    {
		private void FixedUpdate()
		{
			if (this._quaternionTargetMode)
			{
				this._hasTargetLock = base.CheckLockedOn();
				this.IncrementalRotate(Time.fixedDeltaTime);
				return;
			}
			if (this._astroObjectLock == AstroObject.Name.None)
			{
				this._hasTargetLock = false;
				return;
			}
			AstroObject astroObject = Locator.GetAstroObject(this._astroObjectLock);
			if (astroObject == null)
			{
				this._hasTargetLock = false;
				return;
			}
			this._target = astroObject.transform.position;
			this._hasTargetLock = base.CheckLockedOn();
			this.IncrementalRotate(Time.fixedDeltaTime);
		}

		public void SetNewAstroTarget(AstroObject.Name name, string customName, bool resetRampUp)
		{
			if (resetRampUp)
			{
				base.ResetRotationSpeed(resetRampUp);
			}
			_astroObjectLock = name;
			_astroObjectCustomName = customName;
		}

		private AstroObject.Name _astroObjectLock;
		private string _astroObjectCustomName;
	}
}
