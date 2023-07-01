using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class PlayerSpawnHandler
    {
        private static bool _wasInvincible;
        private static bool _wasDeathManagerInvincible;
        private static float _impactDeathSpeed;

        public static void SetUpPlayerSpawn()
        {
            var spawnPoint = Main.SystemDict[Main.Instance.CurrentStarSystem].SpawnPoint;
            if (spawnPoint != null)
            {
                SearchUtilities.Find("Player_Body").GetComponent<MatchInitialMotion>().SetBodyToMatch(spawnPoint.GetAttachedOWRigidbody());
                GetPlayerSpawner().SetInitialSpawnPoint(spawnPoint);
            }
            else
            {
                NHLogger.Log($"No NH spawn point for {Main.Instance.CurrentStarSystem}");
            }
        }

        public static void OnSystemReady(bool shouldWarpInFromShip, bool shouldWarpInFromVessel)
        {
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
                var player = SearchUtilities.Find("Player_Body");
                var playerBody = player.GetAttachedOWRigidbody();
                var spawn = GetDefaultSpawn();

                // Player dies during the teleport sometimes so we prevent that
                var resources = player.GetComponent<PlayerResources>();
                var deathManager = Locator.GetDeathManager();

                _wasInvincible = resources._invincible;
                _wasDeathManagerInvincible = deathManager._invincible;
                _impactDeathSpeed = deathManager._impactDeathSpeed;

                resources._invincible = true;
                deathManager._invincible = true;
                deathManager._impactDeathSpeed = float.MaxValue;

                Delay.FireOnNextUpdate(() =>
                {
                    var matchInitialMotion = playerBody.GetComponent<MatchInitialMotion>();

                    playerBody.WarpToPositionRotation(spawn.transform.position, spawn.transform.rotation);

                    if (matchInitialMotion != null)
                    {
                        // Idk why but these just don't work?
                        UnityEngine.Object.Destroy(matchInitialMotion);
                        Delay.FireOnNextUpdate(FixVelocity);
                    }
                    else
                    {
                        FixVelocity();
                    }
                });
            }
        }

        private static void FixVelocity()
        {
            var player = SearchUtilities.Find("Player_Body");
            var playerBody = player.GetAttachedOWRigidbody();
            var spawn = GetDefaultSpawn();

            playerBody.WarpToPositionRotation(spawn.transform.position, spawn.transform.rotation);

            // Player dies during the teleport sometimes so we prevent that
            var resources = player.GetComponent<PlayerResources>();
            var deathManager = Locator.GetDeathManager();

            var spawnVelocity = spawn._attachedBody.GetVelocity();
            var spawnAngularVelocity = spawn._attachedBody.GetPointTangentialVelocity(playerBody.transform.position);
            var velocity = spawnVelocity + spawnAngularVelocity;

            playerBody.SetVelocity(velocity);
            NHLogger.LogVerbose($"Player spawn velocity {velocity} Player velocity {playerBody.GetVelocity()} spawn body {spawnVelocity} spawn angular vel {spawnAngularVelocity}");

            resources._currentHealth = 100f;

            resources._invincible = _wasInvincible;
            deathManager._invincible = _wasDeathManagerInvincible;
            deathManager._impactDeathSpeed = _impactDeathSpeed;
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
