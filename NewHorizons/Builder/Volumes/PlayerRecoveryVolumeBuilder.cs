using NewHorizons.External.Modules.Volumes.VolumeInfos;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    internal static class PlayerRecoveryVolumeBuilder
    {
        public static PlayerRecoveryPoint Make(GameObject planetGO, Sector sector, PlayerRecoveryVolumeInfo info, IModBehaviour mod)
        {
            var receiver = InteractionVolumeBuilder.MakeReceiver(planetGO, sector, info, mod);

            var volume = receiver.gameObject.AddComponent<PlayerRecoveryPoint>();

            volume._refuelsPlayer = info.refuels;
            volume._healsPlayer = info.heals;
            volume._cleansVisor = info.cleansVisor;
            volume._DLCFuelTank = info.dlcFuel;

            receiver.gameObject.SetActive(true);

            return volume;
        }
    }
}
