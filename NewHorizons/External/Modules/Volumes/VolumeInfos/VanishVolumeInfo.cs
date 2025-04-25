using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    /// <summary>
    /// Note: Only sphere colliders work on vanish volumes!
    /// </summary>
    [JsonObject]
    public class VanishVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// Whether the bodies will shrink when they enter this volume or just disappear instantly.
        /// </summary>
        [DefaultValue(true)] public bool shrinkBodies = true;

        /// <summary>
        /// Whether this volume only affects the player, ship, probe/scout, model rocket ship, and nomai shuttle.
        /// </summary>
        public bool onlyAffectsPlayerRelatedBodies;

        [Obsolete] public bool onlyAffectsPlayerAndShip;
    }

}
