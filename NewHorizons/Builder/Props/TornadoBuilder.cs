using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Random = UnityEngine.Random;

namespace NewHorizons.Builder.Props
{
    public static class TornadoBuilder
    {
        private static GameObject upPrefab;
        private static GameObject downPrefab;

        public static void Make(GameObject go, Sector sector, PropModule.TornadoInfo info, bool hasClouds)
        {
            if (upPrefab == null)
            {
                upPrefab = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_SouthHemisphere/Sector_SouthPole/Sector_Observatory/Interactables_Observatory/MockUpTornado").InstantiateInactive();
                upPrefab.name = "Tornado_Up_Prefab";
            }
            if(downPrefab == null)
            {
                downPrefab = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_SouthHemisphere/Sector_SouthPole/Sector_Observatory/Interactables_Observatory/MockDownTornado").InstantiateInactive();
                downPrefab.name = "Tornado_Down_Prefab";
                downPrefab.name = "Tornado_Down_Prefab";
            }

            float elevation;
            Vector3 position;
            if(info.position != null)
            {
                position = info.position ?? Random.onUnitSphere * info.elevation;
                elevation = position.magnitude;
            }
            else if(info.elevation != 0)
            {
                position = Random.onUnitSphere * info.elevation;
                elevation = info.elevation;
            }
            else
            {
                Logger.LogError($"Need either a position or an elevation for tornados");
                return;
            }

            var tornadoGO = info.downwards ? downPrefab.InstantiateInactive() : upPrefab.InstantiateInactive();
            tornadoGO.transform.parent = sector.transform;
            tornadoGO.transform.localPosition = position;
            tornadoGO.transform.rotation = Quaternion.FromToRotation(Vector3.up, sector.transform.TransformDirection(position.normalized));

            var scale = info.height == 0 ? 1 : info.height / 10f;

            tornadoGO.transform.localScale = Vector3.one * scale;

            var controller = tornadoGO.GetComponent<TornadoController>();
            controller.SetSector(sector);

            controller._bottomStartPos = Vector3.up * -20;
            controller._midStartPos = Vector3.up * 150;
            controller._topStartPos = Vector3.up * 300;

            controller._bottomBone.localPosition = controller._bottomStartPos;
            controller._midBone.localPosition = controller._midStartPos;
            controller._topBone.localPosition = controller._topStartPos;
            
            OWAssetHandler.LoadObject(tornadoGO);
            sector.OnOccupantEnterSector += (sd) => OWAssetHandler.LoadObject(tornadoGO);

            tornadoGO.GetComponentInChildren<CapsuleShape>().enabled = true;

            if(info.tint != null)
            {
                var colour = info.tint.ToColor();
                foreach(var renderer in tornadoGO.GetComponentsInChildren<Renderer>())
                {
                    renderer.material.color = colour;
                    renderer.material.SetColor("_DetailColor", colour);
                    renderer.material.SetColor("_TintColor", colour);
                }
            }

            if(info.wanderRate != 0)
            {
                var wanderer = tornadoGO.AddComponent<NHTornadoWanderController>();
                wanderer.wanderRate = info.wanderRate;
                wanderer.wanderDegreesX = info.wanderDegreesX;
                wanderer.wanderDegreesZ = info.wanderDegreesZ;
                wanderer.sector = sector;
            }

            tornadoGO.SetActive(true);
        }
    }
}
