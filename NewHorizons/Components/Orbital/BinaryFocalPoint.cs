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
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }

        public AstroObject Primary { get; set; }
        public AstroObject Secondary { get; set; }

        public GameObject FakeMassBody { get; set; }

        void Start()
        {
            // Make sure its active but maybe it hasn't been set yet
            if(FakeMassBody) FakeMassBody.SetActive(true);   
        }

        void Update()
        {
            if (Primary == null || Secondary == null) return;

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
