using NewHorizons.Builder.Props;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using OWML.Utils;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NewHorizons.Builder.General
{
    public static class SpawnPointBuilder
    {
        private static bool suitUpQueued = false;

        // Ship
        public static SpawnModule.ShipSpawnPoint ShipSpawnInfo { get; private set; }
        public static SpawnPoint ShipSpawn { get; private set; }

        // Marked as obsolete instead of removing to avoid breaking Ship Enhancements mod.
        [Obsolete]
        public static Vector3 ShipSpawnOffset { get; private set; }

        // Player
        public static SpawnModule.PlayerSpawnPoint PlayerSpawnInfo { get; private set; }
        public static SpawnPoint PlayerSpawn { get; private set; }

        /// <summary>
        /// Makes a ship spawn at the place it spawns in vanilla.
        /// </summary>
        internal static void MakeVanillaShipSpawn()
        {
            // Mark mobius's test spawn as the alt enum value (that one just spawns the ship high above the village for some reason)
            var altShipSpawn = SearchUtilities.Find("TimberHearth_Body/SPAWN_TH/ShipSpawn_TH");
            altShipSpawn.GetComponent<SpawnPoint>().SetSpawnLocation(SpawnLocation.TimberHearth_Alt);
            altShipSpawn.name = "ShipSpawn_TH_Alt";

            var timberHearth = altShipSpawn.GetAttachedOWRigidbody();

            Sector spawnSector = null;
            var spawnGO = GeneralPropBuilder.MakeNew("ShipSpawn_TH", timberHearth.gameObject, ref spawnSector, new GeneralPropInfo
            {
                position = new Vector3(-16.45f, -52.67f, 227.39f),
                rotation = new Vector3(283.0626f, 1.0617f, 174.9344f),
                isRelativeToParent = true
            });
            spawnGO.transform.SetParent(altShipSpawn.transform.parent, true);
            spawnGO.layer = Layer.PlayerSafetyCollider;

            var shipSpawn = spawnGO.AddComponent<SpawnPoint>();
            shipSpawn._isShipSpawn = true;
            shipSpawn._attachedBody = timberHearth;
            shipSpawn._spawnLocation = SpawnLocation.TimberHearth;
            shipSpawn._triggerVolumes = new OWTriggerVolume[0];

            spawnGO.SetActive(true);
        }

        public static void OverridePlayerSpawn(SpawnPoint newSpawn)
        {
            PlayerSpawn = newSpawn;
            PlayerSpawnInfo = null;
        }

        public static SpawnPoint Make(GameObject planetGO, SpawnModule module, OWRigidbody owRigidBody)
        {
            SpawnPoint playerSpawn = null;

            // Make the spawn point even if it won't be used this loop
            if (module.playerSpawnPoints != null)
            {
                foreach (var point in module.playerSpawnPoints)
                {
                    Sector spawnSector = null;
                    GameObject spawnGO = GeneralPropBuilder.MakeNew("PlayerSpawnPoint", planetGO, ref spawnSector, point);
                    spawnGO.layer = Layer.PlayerSafetyCollider;

                    playerSpawn = spawnGO.AddComponent<SpawnPoint>();
                    playerSpawn._attachedBody = owRigidBody;
                    playerSpawn._spawnLocation = SpawnLocation.None;
                    // #601 we need to actually set the right trigger volumes here
                    playerSpawn._triggerVolumes = new OWTriggerVolume[0];

                    // This was a stupid hack to stop players getting stuck in the ground and now we have to keep it forever
                    var playerSpawnOffset = point.offset ?? Vector3.up * 4f;
                    spawnGO.transform.position += spawnGO.transform.TransformDirection(playerSpawnOffset);

                    var flagUseTHSpawn = false;
                    if (Main.Instance.CurrentStarSystem == "SolarSystem")
                    {
                        // When in the base solar system, treat the TH spawn point as being isDefault
                        // If the priority of any new spawn point is less than that, ignore it
                        // Do take them if they're equal tho
                        var minPriority = new SpawnModule.PlayerSpawnPoint() { isDefault = true }.GetPriority();
                        if (point.GetPriority() < minPriority)
                        {
                            flagUseTHSpawn = true;
                        }
                    }

                    if (!flagUseTHSpawn && (PlayerSpawn == null || point.GetPriority() > PlayerSpawnInfo.GetPriority()))
                    {
                        PlayerSpawn = playerSpawn;
                        PlayerSpawnInfo = point;
                    }

                    spawnGO.SetActive(true);
                }
            }

            if (module.shipSpawnPoints != null)
            {
                foreach (var point in module.shipSpawnPoints)
                {
                    Sector spawnSector = null;
                    var spawnGO = GeneralPropBuilder.MakeNew("ShipSpawnPoint", planetGO, ref spawnSector, point);
                    spawnGO.layer = Layer.PlayerSafetyCollider;

                    var shipSpawn = spawnGO.AddComponent<SpawnPoint>();
                    shipSpawn._isShipSpawn = true;
                    shipSpawn._attachedBody = owRigidBody;
                    shipSpawn._spawnLocation = SpawnLocation.None;

                    // #601 we need to actually set the right trigger volumes here
                    shipSpawn._triggerVolumes = new OWTriggerVolume[0];

                    // Move it up a bit more when aligning to surface
                    var shipSpawnOffset = point.offset ?? (point.alignRadial.GetValueOrDefault() ? Vector3.up * 4 : Vector3.zero);
                    spawnGO.transform.position += spawnGO.transform.TransformDirection(shipSpawnOffset);

                    if (ShipSpawn == null || point.GetPriority() > ShipSpawnInfo.GetPriority())
                    {
                        ShipSpawn = shipSpawn;
                        ShipSpawnInfo = point;
                    }

                    spawnGO.SetActive(true);
                }
            }

            // Make sure to queue this up if any spawn point building is happening
            if (!suitUpQueued)
            {
                suitUpQueued = true;
                Delay.RunWhen(() => Main.IsSystemReady, () =>
                {
                    suitUpQueued = false;
                    if (Main.Instance.IsWarpingFromVessel || (!Main.Instance.IsWarpingFromShip && (PlayerSpawnInfo?.startWithSuit ?? false)))
                    {
                        SuitUp();
                    }
                });
            }

            NHLogger.Log($"Made spawnpoint on [{planetGO.name}]");

            return playerSpawn;
        }

        public static void SuitUp()
        {
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

                    // Fix Disappearing Signalscope UI #934 after warping to new system wearing suit
                    Delay.StartCoroutine(SignalScopeZoomCoroutine());
                }
            }
        }

        private static IEnumerator SignalScopeZoomCoroutine()
        {
            while (!Locator.GetToolModeSwapper().GetSignalScope().InZoomMode())
            {
                yield return new WaitForFixedUpdate();
            }
            yield return null;
            Locator.GetToolModeSwapper().GetSignalScope().ExitSignalscopeZoom();
            Locator.GetToolModeSwapper().GetSignalScope().EnterSignalscopeZoom();
        }
    }
}
