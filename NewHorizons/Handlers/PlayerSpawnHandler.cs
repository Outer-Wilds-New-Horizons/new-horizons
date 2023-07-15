using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System;
using System.Collections;
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

                Main.Instance.StartCoroutine(SpawnCoroutine(2));
            }
        }

        private static IEnumerator SpawnCoroutine(int length)
        {
            for(int i = 0; i < length; i++) 
            {
                FixVelocity();
                yield return new WaitForEndOfFrame();
            }

            InvulnerabilityHandler.MakeInvulnerable(false);
        }

        private static void FixVelocity()
        {
            var playerBody = SearchUtilities.Find("Player_Body").GetAttachedOWRigidbody();
            var spawn = GetDefaultSpawn();
            var resources = playerBody.GetComponent<PlayerResources>();

            playerBody.WarpToPositionRotation(spawn.transform.position, spawn.transform.rotation);

            var spawnVelocity = spawn._attachedBody.GetVelocity();
            var spawnAngularVelocity = spawn._attachedBody.GetPointTangentialVelocity(playerBody.transform.position);
            var velocity = spawnVelocity + spawnAngularVelocity;

            playerBody.SetVelocity(velocity);
            NHLogger.LogVerbose($"Player spawn velocity {velocity} Player velocity {playerBody.GetVelocity()} spawn body {spawnVelocity} spawn angular vel {spawnAngularVelocity}");

            resources._currentHealth = 100f;
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
