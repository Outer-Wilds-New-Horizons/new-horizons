using NewHorizons.Builder.Props;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using NewHorizons.Utility.OuterWilds;
using System;
using System.Reflection;
using UnityEngine;
using Steamworks;

namespace NewHorizons.Builder.General
{
    public static class SpawnPointBuilder
    {
        private static bool suitUpQueued = false;
        public static SpawnPoint Make(GameObject planetGO, SpawnModule module, OWRigidbody owRigidBody)
        {
            SpawnPoint playerSpawn = null;

            // Make the spawn point even if it won't be used this loop
            if (module.playerSpawn != null)
            {
                GameObject spawnGO = GeneralPropBuilder.MakeNew("PlayerSpawnPoint", planetGO, null, module.playerSpawn);
                spawnGO.SetActive(false);
                spawnGO.layer = Layer.PlayerSafetyCollider;

                playerSpawn = spawnGO.AddComponent<SpawnPoint>();
                playerSpawn._attachedBody = owRigidBody;
                playerSpawn._spawnLocation = SpawnLocation.None;
                // #601 we need to actually set the right trigger volumes here
                playerSpawn._triggerVolumes = new OWTriggerVolume[0];

                spawnGO.SetActive(true);
            }

            if (module.shipSpawn != null)
            {
                GameObject spawnGO = GeneralPropBuilder.MakeNew("ShipSpawnPoint", planetGO, null, module.shipSpawn);
                spawnGO.SetActive(false);
                spawnGO.layer = Layer.PlayerSafetyCollider;

                var shipSpawn = spawnGO.AddComponent<SpawnPoint>();
                shipSpawn._isShipSpawn = true;
                shipSpawn._attachedBody = owRigidBody;
                shipSpawn._spawnLocation = SpawnLocation.None;

                // #601 we need to actually set the right trigger volumes here
                shipSpawn._triggerVolumes = new OWTriggerVolume[0];

                // TODO: this should happen elsewhere
                var ship = SearchUtilities.Find("Ship_Body");
                if (ship != null)
                {
                    ship.transform.position = spawnGO.transform.position;
                    ship.transform.rotation = spawnGO.transform.rotation;

                    // Move it up a bit more when aligning to surface
                    if (module.shipSpawn.alignRadial.GetValueOrDefault())
                    {
                        ship.transform.position += ship.transform.up * 4f;
                    }

                    ship.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(owRigidBody);
                }
                spawnGO.SetActive(true);
            }

            if ((Main.Instance.IsWarpingFromVessel || (!Main.Instance.IsWarpingFromShip && (module.playerSpawn?.startWithSuit ?? false))) && !suitUpQueued)
            {
                suitUpQueued = true;
                Delay.RunWhen(() => Main.IsSystemReady, SuitUp);
            }

            NHLogger.Log($"Made spawnpoint on [{planetGO.name}]");

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
