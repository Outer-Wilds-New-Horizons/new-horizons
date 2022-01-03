using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Components
{
	public class BlackHoleDestructionVolume : DestructionVolume
	{
        public override void Awake()
        {
			base.Awake();
			_deathType = DeathType.BlackHole;
        }

        public override void VanishProbe(OWRigidbody probeBody, RelativeLocationData entryLocation)
		{
			Logger.Log($"Uh oh you shot your probe into a black hole");
			SurveyorProbe requiredComponent = probeBody.GetRequiredComponent<SurveyorProbe>();
			if (requiredComponent.IsLaunched())
			{
				UnityEngine.Object.Destroy(requiredComponent.gameObject);
			}
		}
	}
}

