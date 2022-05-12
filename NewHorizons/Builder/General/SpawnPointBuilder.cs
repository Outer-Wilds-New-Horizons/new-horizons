using NewHorizons.External;
using OWML.Utils;
using System;
using UnityEngine;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    public static class SpawnPointBuilder
    {
        private static bool suitUpQueued = false;
        public static SpawnPoint Make(GameObject body, SpawnModule module, OWRigidbody rb)
        {
            SpawnPoint playerSpawn = null;
            if(!Main.Instance.IsWarping && module.PlayerSpawnPoint != null)
            {
                GameObject spawnGO = new GameObject("PlayerSpawnPoint");
                spawnGO.transform.parent = body.transform;
                spawnGO.layer = 8;

                spawnGO.transform.localPosition = module.PlayerSpawnPoint;

                playerSpawn = spawnGO.AddComponent<SpawnPoint>();
                spawnGO.transform.rotation = Quaternion.FromToRotation(Vector3.up, (playerSpawn.transform.position - body.transform.position).normalized);
                spawnGO.transform.position = spawnGO.transform.position + spawnGO.transform.TransformDirection(Vector3.up) * 4f;
            }
            if(module.ShipSpawnPoint != null)
            {
                GameObject spawnGO = new GameObject("ShipSpawnPoint");
                spawnGO.transform.parent = body.transform;
                spawnGO.layer = 8;

                spawnGO.transform.localPosition = module.ShipSpawnPoint;

                var spawnPoint = spawnGO.AddComponent<SpawnPoint>();
                spawnPoint._isShipSpawn = true;

                var ship = GameObject.Find("Ship_Body");
                ship.transform.position = spawnPoint.transform.position;
                ship.transform.rotation = Quaternion.FromToRotation(Vector3.up, (spawnPoint.transform.position - body.transform.position).normalized);
                // Move it up a bit more
                ship.transform.position = ship.transform.position + ship.transform.TransformDirection(Vector3.up) * 4f;
                
                ship.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(rb);

                if(Main.Instance.IsWarping)
                {
                    Logger.Log("Overriding player spawn to be inside ship");
                    GameObject playerSpawnGO = new GameObject("PlayerSpawnPoint");
                    playerSpawnGO.transform.parent = ship.transform;
                    playerSpawnGO.layer = 8;

                    playerSpawnGO.transform.localPosition = new Vector3(0, 0, 0);

                    playerSpawn = playerSpawnGO.AddComponent<SpawnPoint>();
                    playerSpawnGO.transform.localRotation = Quaternion.Euler(0,0,0);
                }
            }
            if(!Main.Instance.IsWarping && module.StartWithSuit && !suitUpQueued)
            {
                suitUpQueued = true;
                Main.Instance.ModHelper.Events.Unity.RunWhen(() => Main.IsSystemReady, () => SuitUp());
            }

            Logger.Log("Made spawnpoint on [" + body.name + "]");

            return playerSpawn;
        }

        public static void SuitUp()
        {
            suitUpQueued = false;
            if (Locator.GetPlayerController()._isWearingSuit) return;

            Locator.GetPlayerTransform().GetComponent<PlayerSpacesuit>().SuitUp(false, true, true);

            // Make the ship act as if the player took the suit
            var spv = GameObject.Find("Ship_Body/Module_Supplies/Systems_Supplies/ExpeditionGear")?.GetComponent<SuitPickupVolume>();

            if (spv == null) return;

            spv._containsSuit = false;

            if (spv._allowSuitReturn)
            {
                spv._interactVolume.ChangePrompt(UITextType.ReturnSuitPrompt, spv._pickupSuitCommandIndex);
            }
            else
            {
                spv._interactVolume.EnableSingleInteraction(false, spv._pickupSuitCommandIndex);
            }
                
            spv._timer = 0f;
            spv._index = 0;

            spv.OnSuitUp();
            
            GameObject suitGeometry = spv._suitGeometry;
            if (suitGeometry != null) suitGeometry.SetActive(false);
                
            OWCollider suitOWCollider = spv._suitOWCollider;
            if (suitOWCollider != null) suitOWCollider.SetActivation(false);
                
            spv.enabled = true;
        }
    }
}
