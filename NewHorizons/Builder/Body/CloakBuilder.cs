using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    static class CloakBuilder
    {
        public static void Make(GameObject body, OWRigidbody rigidbody, float radius)
        {
            var cloak = SearchUtilities.Find("RingWorld_Body/CloakingField_IP");

            var newCloak = GameObject.Instantiate(cloak, body.transform);
            newCloak.transform.localPosition = Vector3.zero;

            // Get all the mesh renders
            var renderers = new List<Renderer>();

            foreach(var renderer in body.GetComponentsInChildren<Renderer>())
            {
                renderers.SafeAdd(renderer);
                renderer.enabled = false;
            }

        }
    }
}
