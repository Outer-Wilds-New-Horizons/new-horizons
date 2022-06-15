using NewHorizons.Components;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
namespace NewHorizons.Builder.Body
{
    public static class CloakBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, OWRigidbody OWRB, CloakModule module, bool keepReferenceFrame, IModBehaviour mod)
        {
            var radius = module.radius;

            AudioClip clip = null;
            if (!string.IsNullOrEmpty(module.audioClip))
            {
                clip = SearchUtilities.FindResourceOfTypeAndName<AudioClip>(module.audioClip);

                if (clip == null)
                {
                    Utility.Logger.LogError($"Couldn't get audio from clip [{module.audioClip}]");
                }
            }
            else if (!string.IsNullOrEmpty(module.audioFilePath))
            {
                try
                {
                    clip = AudioUtilities.LoadAudio(mod.ModHelper.Manifest.ModFolderPath + "/" + module.audioFilePath);
                }
                catch { }

                if (clip == null)
                {
                    Utility.Logger.LogError($"Couldn't get audio from file [{module.audioFilePath}]");
                }
            }

            var cloak = SearchUtilities.Find("RingWorld_Body/CloakingField_IP");

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
            cloakAudioSource._audioLibraryClip = AudioType.None;
            cloakAudioSource._clipArrayIndex = 0;
            cloakAudioSource._clipArrayLength = 0;
            cloakAudioSource._clipSelectionOnPlay = OWAudioSource.ClipSelectionOnPlay.MANUAL;
            cloakAudioSource.clip = clip;

            newCloak.SetActive(true);
            cloakFieldController.enabled = true;

            cloakSectorController.EnableCloak();

            // To cloak from the start
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(cloakSectorController.OnPlayerExit);
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(clip != null ? cloakSectorController.TurnOnMusic : cloakSectorController.TurnOffMusic);
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(keepReferenceFrame ? cloakSectorController.EnableReferenceFrameVolume : cloakSectorController.DisableReferenceFrameVolume);
        }
    }
}
