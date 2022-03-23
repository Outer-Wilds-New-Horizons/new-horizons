using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Orbital
{
    public class BinaryFocalPoint : MonoBehaviour
    {
        public string PrimaryName = null;
        public string SecondaryName = null;

        public AstroObject Primary = null;
        public AstroObject Secondary = null;

        public GameObject FakeMassBody = null;

        public List<AstroObject> Planets { get; private set; } = new List<AstroObject>(); 

        void Awake()
        {
            FakeMassBody.SetActive(true);   
        }

        void Update()
        {
            // Secondary and primary must have been engulfed by a star
            if(!Primary.isActiveAndEnabled && !Secondary.isActiveAndEnabled)
            {
                ReferenceFrameTracker component = Locator.GetPlayerBody().GetComponent<ReferenceFrameTracker>();
                if (component.GetReferenceFrame(true) != null && component.GetReferenceFrame(true).GetOWRigidBody() == gameObject)
                {
                    component.UntargetReferenceFrame();
                }
                MapMarker component2 = gameObject.GetComponent<MapMarker>();
                if (component2 != null)
                {
                    component2.DisableMarker();
                }
                gameObject.SetActive(false);
                FakeMassBody.SetActive(false);
            }
        }
    }
}
