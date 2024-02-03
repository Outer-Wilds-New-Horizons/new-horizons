using NewHorizons.Builder.General;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System.Collections;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class PlayerSpawnHandler
    {
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

            var cloak = GetDefaultSpawn()?.GetAttachedOWRigidbody()?.GetComponentInChildren<CloakFieldController>();
            if (cloak != null)
            {
                // Ensures it has invoked everything and actually placed the player in the cloaking field #671
                cloak._firstUpdate = true;
            }

            // Spawn ship
            Delay.FireInNUpdates(SpawnShip, 30);
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
                    foreach (var volume in ship.GetComponentInChildren<ShipFluidDetector>()._activeVolumes)
                    {
                        if (volume.gameObject.activeInHierarchy)
                        {
                            volume.gameObject.SetActive(false);
                            volume.gameObject.SetActive(true);
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
            for(int i = 0; i < length; i++) 
            {
                FixPlayerVelocity();
                yield return new WaitForEndOfFrame();
            }

            InvulnerabilityHandler.MakeInvulnerable(false);
        }

        private static void FixPlayerVelocity()
        {
            var playerBody = SearchUtilities.Find("Player_Body").GetAttachedOWRigidbody();
            var resources = playerBody.GetComponent<PlayerResources>();

            SpawnBody(playerBody, GetDefaultSpawn());

            resources._currentHealth = 100f;
        }

        public static void SpawnBody(OWRigidbody body, SpawnPoint spawn, Vector3? positionOverride = null)
        {
            var pos = positionOverride ?? spawn.transform.position;

            body.WarpToPositionRotation(pos, spawn.transform.rotation);

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

        public static bool UsingCustomSpawn() => Main.SystemDict[Main.Instance.CurrentStarSystem].SpawnPoint != null;
        public static PlayerSpawner GetPlayerSpawner() => GameObject.FindObjectOfType<PlayerSpawner>();
        public static SpawnPoint GetDefaultSpawn() => Main.SystemDict[Main.Instance.CurrentStarSystem].SpawnPoint ?? GetPlayerSpawner().GetSpawnPoint(SpawnLocation.TimberHearth);
    }
}
