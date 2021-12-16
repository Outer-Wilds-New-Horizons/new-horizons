using NewHorizons.External;
using OWML.Utils;
using System;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class SpawnPointBuilder
    {
        public static SpawnPoint Make(GameObject body, SpawnModule module, OWRigidbody rb)
        {
            SpawnPoint playerSpawn = null;
            if(module.PlayerSpawnPoint != null)
            {
                GameObject spawnGO = new GameObject("PlayerSpawnPoint");
                spawnGO.transform.parent = body.transform;
                spawnGO.layer = 8;

                spawnGO.transform.localPosition = module.PlayerSpawnPoint;

                playerSpawn = spawnGO.AddComponent<SpawnPoint>();
                GameObject.FindObjectOfType<PlayerSpawner>().SetInitialSpawnPoint(playerSpawn);
            }
            if(module.ShipSpawnPoint != null)
            {
                GameObject spawnGO = new GameObject("ShipSpawnPoint");
                spawnGO.transform.parent = body.transform;
                spawnGO.layer = 8;

                spawnGO.transform.localPosition = module.ShipSpawnPoint;

                var spawnPoint = spawnGO.AddComponent<SpawnPoint>();
                spawnPoint.SetValue("_isShipSpawn", true);

                var ship = GameObject.Find("Ship_Body");
                ship.transform.position = spawnPoint.transform.position;
                ship.transform.rotation = Quaternion.FromToRotation(Vector3.up, (spawnPoint.transform.position - body.transform.position).normalized);
                ship.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(rb);
            }

            Logger.Log("Made spawnpoint on [" + body.name + "]");
            return playerSpawn;
        }
    }
}
