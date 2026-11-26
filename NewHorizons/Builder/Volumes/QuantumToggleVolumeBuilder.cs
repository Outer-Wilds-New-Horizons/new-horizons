using NewHorizons.Builder.Props;
using NewHorizons.Components.Props;
using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class QuantumToggleVolumeBuilder
    {
        public static QuantumToggleVolume Make(GameObject planetGO, Sector sector, QuantumToggleVolumeInfo info, IModBehaviour mod)
        {
            var quantumVolume = VolumeBuilder.Make<QuantumToggleVolume>(planetGO, ref sector, info);

            // Preserving name for backwards compatibility
            quantumVolume.gameObject.name = string.IsNullOrEmpty(info.rename) ? "QuantumToggleVolume" : info.rename;

            if (planetGO != null && info.quantumObjects != null)
            {
                var quantumObjects = new List<QuantumObject>();

                foreach (var path in info.quantumObjects)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        var transform = planetGO.transform.Find(path);
                        if (transform != null && transform.TryGetComponent(out QuantumObject quantumObject))
                        {
                            quantumObjects.Add(quantumObject);
                        }
                        else
                        {
                            NHLogger.LogError($"Cannot find quantum object at path: {planetGO.name}/{path}");
                        }
                    }
                }

                quantumVolume.quantumObjects = quantumObjects.ToArray();
            }

            quantumVolume.invert = info.invert;
            quantumVolume.undoOnExit = info.undoOnExit;

            quantumVolume.gameObject.SetActive(true);

            return quantumVolume;
        }
    }
}
