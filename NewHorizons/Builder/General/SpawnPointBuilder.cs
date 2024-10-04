using NewHorizons.Builder.Props;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NewHorizons.Builder.General
{
    public static class SpawnPointBuilder
    {
        private static bool suitUpQueued = false;
        public static SpawnPoint ShipSpawn { get; private set; }
        public static Vector3 ShipSpawnOffset { get; private set; }

        public static SpawnPoint Make(GameObject planetGO, SpawnModule module, OWRigidbody owRigidBody)
        {
            SpawnPoint playerSpawn = null;

            // Make the spawn point even if it won't be used this loop
            if (module.playerSpawn != null)
            {
                GameObject spawnGO = GeneralPropBuilder.MakeNew("PlayerSpawnPoint", planetGO, null, module.playerSpawn);
                spawnGO.layer = Layer.PlayerSafetyCollider;

                playerSpawn = spawnGO.AddComponent<SpawnPoint>();
                playerSpawn._attachedBody = owRigidBody;
                playerSpawn._spawnLocation = SpawnLocation.None;
                // #601 we need to actually set the right trigger volumes here
                playerSpawn._triggerVolumes = new OWTriggerVolume[0];

                // This was a stupid hack to stop players getting stuck in the ground and now we have to keep it forever
                spawnGO.transform.position += spawnGO.transform.TransformDirection(module.playerSpawn.offset ?? Vector3.up * 4f);
            }

            if (module.shipSpawn != null)
            {
                var spawnGO = GeneralPropBuilder.MakeNew("ShipSpawnPoint", planetGO, null, module.shipSpawn);
                spawnGO.SetActive(false);
                spawnGO.layer = Layer.PlayerSafetyCollider;

                var shipSpawn = spawnGO.AddComponent<SpawnPoint>();
                shipSpawn._isShipSpawn = true;
                shipSpawn._attachedBody = owRigidBody;
                shipSpawn._spawnLocation = SpawnLocation.None;

                // #601 we need to actually set the right trigger volumes here
                shipSpawn._triggerVolumes = new OWTriggerVolume[0];

                var shipSpawnOffset = module.shipSpawn.offset ?? (module.shipSpawn.alignRadial.GetValueOrDefault() ? Vector3.up * 4 : Vector3.zero);

                if (ShipSpawn == null || module.shipSpawn.IsDefault())
                {
                    ShipSpawn = shipSpawn;
                    ShipSpawnOffset = shipSpawnOffset;
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
                    spv._interactVolume._listInteractions.First(x => x.promptText == UITextType.SuitUpPrompt).interactionEnabled = true;
                }
            }
        }
    }
}
