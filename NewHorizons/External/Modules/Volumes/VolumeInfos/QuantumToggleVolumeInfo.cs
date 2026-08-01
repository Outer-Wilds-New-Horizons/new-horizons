using NewHorizons.External.Modules.Props;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class QuantumToggleVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// Path to the quantum objects to toggle when entering the volume.
        /// </summary>
        public string[] quantumObjects;

        /// <summary>
        /// Invert the toggle so it starts on but turns off when entering volume.
        /// </summary>
        [DefaultValue(false)] public bool invert = false;

        /// <summary>
        /// Whether exiting the volume will undo the toggle that happened when entering it.
        /// </summary>
        [DefaultValue(true)] public bool undoOnExit = true;
    }
}
