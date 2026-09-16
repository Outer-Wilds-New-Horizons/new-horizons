using NewHorizons.Components;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Handlers
{
    internal static class EntrywayHandler
    {
        internal static EntrywayVolumeHelper AttachVolumeList(GameObject planetGO, GameObject go, string[] triggerVolumePaths)
        {
            var entrywayVolumes = ResolveTriggerVolumes(planetGO, triggerVolumePaths);
            if (entrywayVolumes.Length == 0)
            {
                return null;
            }
            var helper = go.AddComponent<EntrywayVolumeHelper>();
            helper.entrywayVolumes = entrywayVolumes;
            return helper;
        }

        internal static OWTriggerVolume[] ResolveTriggerVolumes(GameObject planetGO, string[] triggerVolumePaths)
        {
            if (triggerVolumePaths == null || triggerVolumePaths.Length == 0)
            {
                return new OWTriggerVolume[0];
            }
            var triggerVolumes = new OWTriggerVolume[triggerVolumePaths.Length];
            for (int i = 0; i < triggerVolumePaths.Length; i++)
            {
                var volumeTransform = planetGO.transform.Find(triggerVolumePaths[i]);
                if (volumeTransform == null)
                {
                    NHLogger.LogError($"Failed to find trigger volume object at path: {planetGO.name}/{triggerVolumePaths[i]}");
                    continue;
                }
                var triggerVolume = volumeTransform.GetComponent<OWTriggerVolume>();
                if (triggerVolume == null)
                {
                    NHLogger.LogError($"Failed to find {nameof(OWTriggerVolume)} component on object at path: {planetGO.name}/{triggerVolumePaths[i]}");
                    continue;
                }
                triggerVolumes[i] = triggerVolume;
            }
            return triggerVolumes;
        }

        internal static void AddBodyToTriggerVolumes(OWRigidbody body, EntrywayVolumeHelper helper)
        {
            if (helper == null) return;
            AddBodyToTriggerVolumes(body, helper.entrywayVolumes);
        }

        internal static void AddBodyToTriggerVolumes(OWRigidbody body, OWTriggerVolume[] triggerVolumes)
        {
            if (triggerVolumes == null || triggerVolumes.Length == 0) return;
            if (body.CompareTag("Player"))
            {
                AddPlayerToTriggerVolumes(triggerVolumes);
            }
            else if (body.CompareTag("Ship"))
            {
                foreach (var triggerVolume in triggerVolumes)
                {
                    triggerVolume.AddObjectToVolume(Locator.GetShipDetector());
                    if (PlayerState.IsInsideShip())
                    {
                        AddPlayerToTriggerVolumes(triggerVolumes);
                    }
                }
            }
            else if (body.CompareTag("Probe"))
            {
                foreach (var triggerVolume in triggerVolumes)
                {
                    triggerVolume.AddObjectToVolume(Locator.GetProbe().GetDetectorObject());
                }
            }
            else
            {
                // "ShipCockpit", "NomaiShuttleBody", etc. might need special handling too but the above should cover most cases
                foreach (var triggerVolume in triggerVolumes)
                {
                    triggerVolume.AddObjectToVolume(body.gameObject);
                }
            }
        }

        internal static void RemoveBodyFromTriggerVolumes(OWRigidbody body, EntrywayVolumeHelper helper)
        {
            if (helper == null) return;
            RemoveBodyFromTriggerVolumes(body, helper.entrywayVolumes);
        }

        internal static void RemoveBodyFromTriggerVolumes(OWRigidbody body, OWTriggerVolume[] triggerVolumes)
        {
            if (triggerVolumes == null || triggerVolumes.Length == 0) return;
            if (body.CompareTag("Player"))
            {
                RemovePlayerFromTriggerVolumes(triggerVolumes);
            }
            else if (body.CompareTag("Ship"))
            {
                foreach (var triggerVolume in triggerVolumes)
                {
                    triggerVolume.RemoveObjectFromVolume(Locator.GetShipDetector());
                    if (PlayerState.IsInsideShip())
                    {
                        RemovePlayerFromTriggerVolumes(triggerVolumes);
                    }
                }
            }
            else if (body.CompareTag("Probe"))
            {
                foreach (var triggerVolume in triggerVolumes)
                {
                    triggerVolume.RemoveObjectFromVolume(Locator.GetProbe().GetDetectorObject());
                }
            }
            else
            {
                foreach (var triggerVolume in triggerVolumes)
                {
                    triggerVolume.RemoveObjectFromVolume(body.gameObject);
                }
            }
        }

        internal static void AddPlayerToTriggerVolumes(EntrywayVolumeHelper helper, bool includeCamera = true)
        {
            if (helper == null) return;
            AddPlayerToTriggerVolumes(helper.entrywayVolumes, includeCamera);
        }

        internal static void AddPlayerToTriggerVolumes(OWTriggerVolume[] triggerVolumes, bool includeCamera = true)
        {
            if (triggerVolumes == null || triggerVolumes.Length == 0) return;
            foreach (var triggerVolume in triggerVolumes)
            {
                triggerVolume.AddObjectToVolume(Locator.GetPlayerDetector());
                if (includeCamera)
                {
                    triggerVolume.AddObjectToVolume(Locator.GetPlayerCameraDetector());
                }
            }
        }

        internal static void RemovePlayerFromTriggerVolumes(EntrywayVolumeHelper helper, bool includeCamera = true)
        {
            if (helper == null) return;
            RemovePlayerFromTriggerVolumes(helper.entrywayVolumes, includeCamera);
        }

        internal static void RemovePlayerFromTriggerVolumes(OWTriggerVolume[] triggerVolumes, bool includeCamera = true)
        {
            if (triggerVolumes == null || triggerVolumes.Length == 0) return;
            foreach (var triggerVolume in triggerVolumes)
            {
                triggerVolume.RemoveObjectFromVolume(Locator.GetPlayerDetector());
                if (includeCamera)
                {
                    triggerVolume.RemoveObjectFromVolume(Locator.GetPlayerCameraDetector());
                }
            }
        }
    }
}
