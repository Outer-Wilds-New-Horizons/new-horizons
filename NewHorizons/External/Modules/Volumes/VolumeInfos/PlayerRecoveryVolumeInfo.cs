using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class PlayerRecoveryVolumeInfo : BaseInteractionVolumeInfo
    {
        /// <summary>
        /// Whether interacting with this volume refuels the player's jetpack.
        /// </summary>
        [DefaultValue(true)] public bool refuels = true;

        /// <summary>
        /// Whether interacting with this volume heals the player.
        /// </summary>
        public bool heals;

        /// <summary>
        /// Whether interacting with this volume cleans dirt off the helmet visor.
        /// </summary>
        public bool cleansVisor;

        /// <summary>
        /// Whether the fuel fire color would be green.
        /// </summary>
        public bool dlcFuel;
    }
}
