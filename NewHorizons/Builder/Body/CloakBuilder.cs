using NewHorizons.Components;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    public static class CloakBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, OWRigidbody OWRB, CloakModule module, bool keepReferenceFrame, IModBehaviour mod)
        {
            var radius = module.radius;

            var cloak = SearchUtilities.Find("RingWorld_Body/CloakingField_IP");
            if (cloak == null)
            {
                Logger.LogWarning($"Tried to make a cloak but couldn't. Do you have the DLC installed?");
                return;
            }

            var newCloak = GameObject.Instantiate(cloak, sector?.transform ?? planetGO.transform);
            newCloak.transform.position = planetGO.transform.position;
            newCloak.transform.name = "CloakingField";
            newCloak.transform.localScale = Vector3.one * radius;

            GameObject.Destroy(newCloak.GetComponent<PlayerCloakEntryRedirector>());

            var cloakFieldController = newCloak.GetComponent<CloakFieldController>();
            cloakFieldController._cloakScaleDist = radius * 2000 / 3000f;
            cloakFieldController._farCloakRadius = radius * 500 / 3000f;
            cloakFieldController._innerCloakRadius = radius * 900 / 3000f;
            cloakFieldController._nearCloakRadius = radius * 800 / 3000f;

            cloakFieldController._referenceFrameVolume = OWRB._attachedRFVolume;
            cloakFieldController._exclusionSector = null;
            cloakFieldController._cloakSphereVolume = (sector?.transform ?? planetGO.transform).GetComponentInChildren<OWTriggerVolume>();

            var cloakSectorController = newCloak.AddComponent<CloakSectorController>();
            cloakSectorController.Init(newCloak.GetComponent<CloakFieldController>(), planetGO);

            var cloakAudioSource = newCloak.GetComponentInChildren<OWAudioSource>();
            cloakAudioSource._audioSource = cloakAudioSource.GetComponent<AudioSource>();
            bool hasCustomAudio = !string.IsNullOrEmpty(module.audio);
            if (hasCustomAudio) AudioUtilities.SetAudioClip(cloakAudioSource, module.audio, mod);
            
            newCloak.SetActive(true);
            cloakFieldController.enabled = true;

            cloakSectorController.EnableCloak();

            // To cloak from the start
            Delay.FireOnNextUpdate(cloakSectorController.OnPlayerExit);
            Delay.FireOnNextUpdate(hasCustomAudio ? cloakSectorController.TurnOnMusic : cloakSectorController.TurnOffMusic);
            Delay.FireOnNextUpdate(keepReferenceFrame ? cloakSectorController.EnableReferenceFrameVolume : cloakSectorController.DisableReferenceFrameVolume);
        }
    }
}
