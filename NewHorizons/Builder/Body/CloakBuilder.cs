using NewHorizons.Components;
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
        public static void Make(GameObject body, Sector sector, float radius)
        {
            var cloak = SearchUtilities.Find("RingWorld_Body/CloakingField_IP");

            var newCloak = GameObject.Instantiate(cloak, body.transform);
            newCloak.transform.localPosition = Vector3.zero;
            newCloak.SetActive(true);

            var cloakSectorController = newCloak.AddComponent<CloakSectorController>();
            cloakSectorController.Init(newCloak.GetComponent<CloakFieldController>(), sector);

            // To cloak from the start
            cloakSectorController.OnPlayerExit();
        }
    }
}
