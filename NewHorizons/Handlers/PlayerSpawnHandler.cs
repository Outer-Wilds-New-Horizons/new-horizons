using NewHorizons.Builder.General;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class PlayerSpawnHandler
    {
        /// <summary>
        /// Set during the previous loop, force the player to spawn here
        /// </summary>
        public static string TargetSpawnID { get; set; }

        public static void SetUpPlayerSpawn()
        {
            if (UsingCustomSpawn())
            {
                var spawn = GetDefaultSpawn();
                SearchUtilities.Find("Player_Body").GetComponent<MatchInitialMotion>().SetBodyToMatch(spawn.GetAttachedOWRigidbody());
                GetPlayerSpawner().SetInitialSpawnPoint(spawn);
            }
            else
            {
                NHLogger.Log($"No NH spawn point for {Main.Instance.CurrentStarSystem}");
            }
        }

        public static void OnSystemReady(bool shouldWarpInFromShip, bool shouldWarpInFromVessel)
        {
            NHLogger.Log($"OnSystemReady {shouldWarpInFromVessel}, {shouldWarpInFromShip}, {UsingCustomSpawn()}");
            if (shouldWarpInFromShip)
            {
                Main.Instance.ShipWarpController.WarpIn(Main.Instance.WearingSuit);
            }
            else if (shouldWarpInFromVessel)
            {
                VesselWarpHandler.TeleportToVessel();
            }
            else if (UsingCustomSpawn())
            {
                InvulnerabilityHandler.MakeInvulnerable(true);

                // Idk why but these just don't work?
                var matchInitialMotion = SearchUtilities.Find("Player_Body").GetComponent<MatchInitialMotion>();
                if (matchInitialMotion != null) UnityEngine.Object.Destroy(matchInitialMotion);

                // Arbitrary number, depending on the machine some people die, some people fall through the floor, its very inconsistent
                Delay.StartCoroutine(SpawnCoroutine(30));
            }


            // It was NREing in here when it was all ?. so explicit null checks
            var spawn = GetDefaultSpawn();
            if (spawn != null)
            {
                var attachedOWRigidBody = spawn.GetAttachedOWRigidbody();
                if (attachedOWRigidBody != null)
                {
                    var cloak = attachedOWRigidBody.GetComponentInChildren<CloakFieldController>();
                    if (cloak != null)
                    {
                        // Ensures it has invoked everything and actually placed the player in the cloaking field #671
                        cloak._firstUpdate = true;
                    }
                }
            }

            // Spawn ship
            Delay.FireInNUpdates(SpawnShip, 30);

            if (UsingCustomSpawn() || shouldWarpInFromShip || shouldWarpInFromVessel)
            {
                // Have had bug reports (#1034, #975) where sometimes after spawning via vessel warp or ship warp you die from impact velocity after being flung
                // Something weird must be happening with velocity.
                // Try to correct it here after the ship is done spawning
                Delay.FireInNUpdates(() => FixVelocity(shouldWarpInFromVessel, shouldWarpInFromShip), 31);
            }
        }

        private static void FixVelocity(bool shouldWarpInFromVessel, bool shouldWarpInFromShip)
        {
            var spawnOWRigidBody = GetDefaultSpawn().GetAttachedOWRigidbody();
            if (shouldWarpInFromVessel) spawnOWRigidBody = VesselWarpHandler.VesselSpawnPoint.GetAttachedOWRigidbody();
            if (shouldWarpInFromShip) spawnOWRigidBody = Locator.GetShipBody();

            var spawnVelocity = spawnOWRigidBody.GetVelocity();
            var spawnAngularVelocity = spawnOWRigidBody.GetPointTangentialVelocity(Locator.GetPlayerBody().GetPosition());
            var velocity = spawnVelocity + spawnAngularVelocity;

            Locator.GetPlayerBody().SetVelocity(velocity);
        }

        public static void SpawnShip()
        {
            var ship = SearchUtilities.Find("Ship_Body");

            if (SpawnPointBuilder.ShipSpawn != null)
            {
                NHLogger.Log("Spawning player ship");

                if (ship != null)
                {
                    ship.SetActive(true);

                    var pos = SpawnPointBuilder.ShipSpawn.transform.position;

                    // Move it up a bit more when aligning to surface
                    if (SpawnPointBuilder.ShipSpawnOffset != null)
                    {
                        pos += SpawnPointBuilder.ShipSpawn.transform.TransformDirection(SpawnPointBuilder.ShipSpawnOffset);
                    }

                    // #748 Before moving the ship, reset all its landing pad sensors
                    // Else they might think its still touching TH
                    // Doing this before moving the ship so that if they start contacting in the new spawn point then that gets preserved
                    foreach (var landingPadSensor in ship.GetComponentsInChildren<LandingPadSensor>())
                    {
                        landingPadSensor._contactBody = null;
                    }

                    SpawnBody(ship.GetAttachedOWRigidbody(), SpawnPointBuilder.ShipSpawn, pos);

                    // Bug affecting mods with massive stars (8600m+ radius)
                    // TH has an orbital radius of 8600m, meaning the base game ship spawn ends up inside the star
                    // This places the ship into the star's fluid volumes (destruction volume and atmosphere)
                    // When the ship is teleported out, it doesn't update it's detected fluid volumes and gets affected by drag forever
                    // Can fix by turning the volumes off and on again
                    // Done after re-positioning else it'd just get re-added to the old volumes
                    
                    // .ToList is because it makes a copy of the array, else it errors:
                    // "InvalidOperationException: Collection was modified; enumeration operation may not execute."
                    foreach (var volume in ship.GetComponentInChildren<ShipFluidDetector>()._activeVolumes.ToList())
                    {
                        if (volume.gameObject.activeInHierarchy)
                        {
                            volume.gameObject.SetActive(false);
                            volume.gameObject.SetActive(true);
                        }
                    }
                    // Also applies to force volumes
                    foreach (var volume in ship.GetComponentInChildren<AlignmentForceDetector>()._activeVolumes.ToList())
                    {
                        if (volume.gameObject.activeInHierarchy)
                        {
                            volume.gameObject.SetActive(false);
                            volume.gameObject.SetActive(true);
                        }
                    }
                    // For some reason none of this seems to apply to the Player.
                    // If somebody ever makes a sound volume thats somehow always applying to the player tho then itd probably be this

                    // Sometimes the ship isn't added to the volumes it's meant to now be in
                    foreach (var volume in SpawnPointBuilder.ShipSpawn.GetAttachedOWRigidbody().GetComponentsInChildren<EffectVolume>())
                    {
                        if (volume.GetOWTriggerVolume().GetPenetrationDistance(ship.transform.position) > 0)
                        {
                            // Add ship to volume
                            // If it's already tracking it it will complain here but thats fine
                            volume.GetOWTriggerVolume().AddObjectToVolume(Locator.GetShipDetector());
                        }
                    }
                }
            }
            else if (Main.Instance.CurrentStarSystem != "SolarSystem" && !Main.Instance.IsWarpingFromShip)
            {
                NHLogger.Log("System has no ship spawn. Deactivating it.");
                ship?.SetActive(false);
            }
        }

        private static IEnumerator SpawnCoroutine(int length)
        {
            FixPlayerVelocity();
            for(int i = 0; i < length; i++) 
            {
                FixPlayerVelocity(false); // dont recenter universe here or else it spams and lags game
                yield return new WaitForFixedUpdate();
            }
            FixPlayerVelocity();

            InvulnerabilityHandler.MakeInvulnerable(false);

            // Done spawning
            TargetSpawnID = null;
        }

        private static void FixPlayerVelocity(bool recenter = true)
        {
            var playerBody = SearchUtilities.Find("Player_Body").GetAttachedOWRigidbody();
            var resources = playerBody.GetComponent<PlayerResources>();

            SpawnBody(playerBody, GetDefaultSpawn(), recenter: recenter);

            resources._currentHealth = 100f;
        }

        public static void SpawnBody(OWRigidbody body, SpawnPoint spawn, Vector3? positionOverride = null, bool recenter = true)
        {
            var pos = positionOverride ?? spawn.transform.position;

            if (recenter)
            {
                body.WarpToPositionRotation(pos, spawn.transform.rotation);
            }
            else
            {
                body.transform.SetPositionAndRotation(pos, spawn.transform.rotation);
            }

            var spawnVelocity = spawn._attachedBody.GetVelocity();
            var spawnAngularVelocity = spawn._attachedBody.GetPointTangentialVelocity(pos);
            var velocity = spawnVelocity + spawnAngularVelocity;

            body.SetVelocity(velocity);
        }

        private static Vector3 CalculateMatchVelocity(OWRigidbody owRigidbody, OWRigidbody bodyToMatch, bool ignoreAngularVelocity)
        {
            var vector = Vector3.zero;
            owRigidbody.UpdateCenterOfMass();
            vector += bodyToMatch.GetVelocity();
            if (!ignoreAngularVelocity)
            {
                var worldCenterOfMass = owRigidbody.GetWorldCenterOfMass();
                var worldCenterOfMass2 = bodyToMatch.GetWorldCenterOfMass();
                var initAngularVelocity = bodyToMatch.GetAngularVelocity();
                vector += OWPhysics.PointTangentialVelocity(worldCenterOfMass, worldCenterOfMass2, initAngularVelocity);
            }

            var aoPrimary = bodyToMatch.GetComponent<AstroObject>()?._primaryBody?.GetAttachedOWRigidbody();
            // Stock sun has its primary as itself for some reason
            if (aoPrimary != null && aoPrimary != bodyToMatch)
            {
                vector += CalculateMatchVelocity(bodyToMatch, aoPrimary, true);
            }
            return vector;
        }

        public static bool UsingCustomSpawn() => SpawnPointBuilder.PlayerSpawn != null;
        public static PlayerSpawner GetPlayerSpawner() => GameObject.FindObjectOfType<PlayerSpawner>();
        public static SpawnPoint GetDefaultSpawn() => SpawnPointBuilder.PlayerSpawn ?? GetPlayerSpawner().GetSpawnPoint(SpawnLocation.TimberHearth);
    }
}
