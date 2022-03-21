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
            newCloak.transform.name = "CloakingField";
            newCloak.transform.localScale = Vector3.one * radius;

            GameObject.Destroy(newCloak.GetComponent<PlayerCloakEntryRedirector>());

            var cloakFieldController = newCloak.GetComponent<CloakFieldController>();
            cloakFieldController._cloakScaleDist = radius * 2000 / 3000f;
            cloakFieldController._farCloakRadius = radius * 500 / 3000f;
            cloakFieldController._innerCloakRadius = radius * 900 / 3000f;
            cloakFieldController._nearCloakRadius = radius * 800 / 3000f;

            cloakFieldController._referenceFrameVolume = null;
            cloakFieldController._exclusionSector = null;

            var cloakSectorController = newCloak.AddComponent<CloakSectorController>();
            cloakSectorController.Init(newCloak.GetComponent<CloakFieldController>(), body);

            newCloak.SetActive(true);
            cloakFieldController.enabled = true;

            // To cloak from the start
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(cloakSectorController.OnPlayerExit);
        }
    }
}
