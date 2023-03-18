using NewHorizons.Builder.Props;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using System;
using System.Reflection;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.General
{
    public static class SpawnPointBuilder
    {
        private static bool suitUpQueued = false;
        public static SpawnPoint Make(GameObject planetGO, SpawnModule module, OWRigidbody owRigidBody)
        {
            SpawnPoint playerSpawn = null;

            var playerSpawnInfo = module.playerSpawn;
            var shipSpawnInfo = module.shipSpawn;

            // Backwards compatibility
#pragma warning disable 612, 618
            if (playerSpawnInfo == null && module.playerSpawnPoint != null)
            {
                playerSpawnInfo = new SpawnModule.PlayerSpawnPoint()
                {
                    position = module.playerSpawnPoint,
                    rotation = module.playerSpawnRotation,
                    startWithSuit = module.startWithSuit,
                };
            }
            if (shipSpawnInfo == null && module.shipSpawnPoint != null)
            {
                shipSpawnInfo = new SpawnModule.ShipSpawnPoint()
                {
                    position = module.shipSpawnPoint,
                    rotation = module.shipSpawnRotation,
                };
            }
#pragma warning restore 612, 618

            if (!Main.Instance.IsWarpingFromVessel && !Main.Instance.IsWarpingFromShip && playerSpawnInfo != null)
            {
                bool alignToBody = playerSpawnInfo.rotation == null;
                GameObject spawnGO = GeneralPropBuilder.MakeNew("PlayerSpawnPoint", planetGO.transform, playerSpawnInfo, alignToBody);
                spawnGO.layer = 8;

                playerSpawn = spawnGO.AddComponent<SpawnPoint>();
                playerSpawn._triggerVolumes = new OWTriggerVolume[0];

                spawnGO.transform.position += spawnGO.transform.up * 4f;
            }
            if (shipSpawnInfo != null)
            {
                bool alignToBody = shipSpawnInfo.rotation == null;
                GameObject spawnGO = GeneralPropBuilder.MakeNew("ShipSpawnPoint", planetGO.transform, shipSpawnInfo, alignToBody);
                spawnGO.layer = 8;

                var spawnPoint = spawnGO.AddComponent<SpawnPoint>();
                spawnPoint._isShipSpawn = true;
                spawnPoint._triggerVolumes = new OWTriggerVolume[0];

                var ship = SearchUtilities.Find("Ship_Body");
                if (ship != null)
                {
                    ship.transform.position = spawnGO.transform.position;
                    ship.transform.rotation = spawnGO.transform.rotation;

                    // Move it up a bit more when aligning to surface
                    if (alignToBody)
                    {
                        ship.transform.position += ship.transform.up * 4f;
                    }

                    ship.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(owRigidBody);

                    if (Main.Instance.IsWarpingFromShip)
                    {
                        Logger.LogVerbose("Overriding player spawn to be inside ship");
                        GameObject playerSpawnGO = new GameObject("PlayerSpawnPoint");
                        playerSpawnGO.transform.parent = ship.transform;
                        playerSpawnGO.layer = 8;

                        playerSpawnGO.transform.localPosition = new Vector3(0, 0, 0);

                        playerSpawn = playerSpawnGO.AddComponent<SpawnPoint>();
                        playerSpawn._triggerVolumes = new OWTriggerVolume[0];
                        playerSpawnGO.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                }
            }

            if ((Main.Instance.IsWarpingFromVessel || (!Main.Instance.IsWarpingFromShip && (playerSpawnInfo?.startWithSuit ?? false))) && !suitUpQueued)
            {
                suitUpQueued = true;
                Delay.RunWhen(() => Main.IsSystemReady, () => SuitUp());
            }

            Logger.Log($"Made spawnpoint on [{planetGO.name}]");

            return playerSpawn;
        }

        public static void SuitUp()
        {
            suitUpQueued = false;
            if (!Locator.GetPlayerController()._isWearingSuit)
            {
                Locator.GetPlayerSuit().SuitUp(false, true, true);
                var spv = SearchUtilities.Find("Ship_Body/Module_Supplies/Systems_Supplies/ExpeditionGear")?.GetComponent<SuitPickupVolume>();
                if (spv != null)
                {
                    var command = spv._interactVolume.GetInteractionAt(spv._pickupSuitCommandIndex).inputCommand;

                    // Make the ship act as if the player took the suit
                    var eventDelegate = (MulticastDelegate)typeof(MultipleInteractionVolume).GetField(
                        nameof(MultipleInteractionVolume.OnPressInteract),
                        BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(spv._interactVolume);
                    foreach (var handler in eventDelegate.GetInvocationList())
                    {
                        handler.Method.Invoke(handler.Target, new object[] { command });
                    }
                }
            }
        }
    }
}
