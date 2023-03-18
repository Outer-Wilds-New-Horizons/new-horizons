using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Volumes
{
    [JsonObject]
    public class VanishVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// Whether the bodies will shrink when they enter this volume or just disappear instantly.
        /// </summary>
        [DefaultValue(true)] public bool shrinkBodies = true;

        /// <summary>
        /// Whether this volume only affects the player and ship.
        /// </summary>
        public bool onlyAffectsPlayerAndShip;
    }
}

