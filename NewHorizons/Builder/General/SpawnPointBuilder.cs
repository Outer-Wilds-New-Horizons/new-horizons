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

            if (!Main.Instance.IsWarpingFromVessel && !Main.Instance.IsWarpingFromShip && module.playerSpawn != null)
            {
                bool alignToBody = module.playerSpawn.rotation == null;
                GameObject spawnGO = GeneralPropBuilder.MakeNew("PlayerSpawnPoint", planetGO.transform, module.playerSpawn, alignToBody);
                spawnGO.layer = 8;

                playerSpawn = spawnGO.AddComponent<SpawnPoint>();
                playerSpawn._triggerVolumes = new OWTriggerVolume[0];

                spawnGO.transform.position += spawnGO.transform.up * 4f;
            }
            if (module.shipSpawn != null)
            {
                bool alignToBody = module.shipSpawn.rotation == null;
                GameObject spawnGO = GeneralPropBuilder.MakeNew("ShipSpawnPoint", planetGO.transform, module.shipSpawn, alignToBody);
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

            if ((Main.Instance.IsWarpingFromVessel || (!Main.Instance.IsWarpingFromShip && (module.playerSpawn?.startWithSuit ?? false))) && !suitUpQueued)
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
