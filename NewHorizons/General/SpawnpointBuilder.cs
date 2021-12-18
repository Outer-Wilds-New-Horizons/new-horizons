using NewHorizons.External;
using OWML.Utils;
using System;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class SpawnPointBuilder
    {
        private static bool suitUpQueued = false;
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
                // Move it up a bit more
                ship.transform.position = ship.transform.position + ship.transform.TransformDirection(Vector3.up) * 5f;
                
                ship.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(rb);
            }
            if(module.StartWithSuit && !suitUpQueued)
            {
                suitUpQueued = true;
                Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => SuitUp(), 4);
            }

            Logger.Log("Made spawnpoint on [" + body.name + "]");
            return playerSpawn;
        }

        private static void SuitUp()
        {
            suitUpQueued = false;
            try
            {
                var spv = GameObject.Find("Ship_Body/Module_Supplies/Systems_Supplies/ExpeditionGear").GetComponent<SuitPickupVolume>();
                spv.SetValue("_containsSuit", false);

                if (spv.GetValue<bool>("_allowSuitReturn"))
                    spv.GetValue<MultipleInteractionVolume>("_interactVolume").ChangePrompt(UITextType.ReturnSuitPrompt, spv.GetValue<int>("_pickupSuitCommandIndex"));
                else
                    spv.GetValue<MultipleInteractionVolume>("_interactVolume").EnableSingleInteraction(false, spv.GetValue<int>("_pickupSuitCommandIndex"));

                Locator.GetPlayerTransform().GetComponent<PlayerSpacesuit>().SuitUp(false, true, true);
                
                spv.SetValue("_timer", 0f);
                spv.SetValue("_index", 0);

                GameObject suitGeometry = spv.GetValue<GameObject>("_suitGeometry");
                if (suitGeometry != null) suitGeometry.SetActive(false);
                
                OWCollider suitOWCollider = spv.GetValue<OWCollider>("_suitOWCollider");
                if (suitOWCollider != null) suitOWCollider.SetActivation(false);
                
                spv.enabled = true;
            }
            catch(Exception e)
            {
                Logger.LogWarning($"Was unable to suit up player. {e.Message}, {e.StackTrace}");
            }
        }
    }
}
