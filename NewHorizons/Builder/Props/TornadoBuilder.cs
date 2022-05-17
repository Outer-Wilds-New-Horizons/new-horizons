using NewHorizons.Components;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Random = UnityEngine.Random;
namespace NewHorizons.Builder.Props
{
    public static class TornadoBuilder
    {
        private static GameObject upPrefab;
        private static GameObject downPrefab;
        private static GameObject soundPrefab;

        public static void Make(GameObject planetGO, Sector sector, PropModule.TornadoInfo info, bool hasClouds)
        {
            if (upPrefab == null)
            {
                upPrefab = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_SouthHemisphere/Sector_SouthPole/Sector_Observatory/Interactables_Observatory/MockUpTornado").InstantiateInactive();
                upPrefab.name = "Tornado_Up_Prefab";
            }
            if (downPrefab == null)
            {
                downPrefab = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_SouthHemisphere/Sector_SouthPole/Sector_Observatory/Interactables_Observatory/MockDownTornado").InstantiateInactive();
                downPrefab.name = "Tornado_Down_Prefab";
            }
            if (soundPrefab == null)
            {
                soundPrefab = GameObject.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Tornadoes_GDInterior/SouthernTornadoes/DownTornado_Pivot/DownTornado/AudioRail").InstantiateInactive();
                soundPrefab.name = "AudioRail_Prefab";
            }

            float elevation;
            Vector3 position;
            if (info.position != null)
            {
                position = info.position ?? Random.onUnitSphere * info.elevation;
                elevation = position.magnitude;
            }
            else if (info.elevation != 0)
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
            tornadoGO.transform.position = planetGO.transform.TransformPoint(position);
            tornadoGO.transform.rotation = Quaternion.FromToRotation(Vector3.up, sector.transform.TransformDirection(position.normalized));

            // Add the sound thing before changing the scale
            var soundGO = soundPrefab.InstantiateInactive();
            soundGO.name = "AudioRail";
            soundGO.transform.parent = tornadoGO.transform;
            soundGO.transform.localPosition = Vector3.zero;
            soundGO.transform.localRotation = Quaternion.identity;

            // Height of the tornado is 10 by default
            var audioRail = soundGO.GetComponent<AudioRail>();
            audioRail.SetSector(sector);
            audioRail._railPointsRoot.GetChild(0).transform.localPosition = Vector3.zero;
            audioRail._railPointsRoot.GetChild(1).transform.localPosition = Vector3.up * 10;
            audioRail._railPoints = new Vector3[]
            {
                Vector3.zero,
                Vector3.up * 10
            };

            var audioSpreadController = soundGO.GetComponentInChildren<AudioSpreadController>();
            audioSpreadController.SetSector(sector);

            var audioSource = audioRail._audioTransform.GetComponent<AudioSource>();
            audioSource.playOnAwake = true;

            var scale = info.height == 0 ? 1 : info.height / 10f;

            tornadoGO.transform.localScale = Vector3.one * scale;

            // Resize the distance it can be heard from to match roughly with the size
            audioSource.maxDistance = 100 * scale;

            var controller = tornadoGO.GetComponent<TornadoController>();
            controller.SetSector(sector);

            // Found these values by messing around in unity explorer until it looked right
            controller._bottomStartPos = Vector3.up * -20;
            controller._midStartPos = Vector3.up * 150;
            controller._topStartPos = Vector3.up * 300;

            controller._bottomBone.localPosition = controller._bottomStartPos;
            controller._midBone.localPosition = controller._midStartPos;
            controller._topBone.localPosition = controller._topStartPos;

            OWAssetHandler.LoadObject(tornadoGO);
            sector.OnOccupantEnterSector += (sd) => OWAssetHandler.LoadObject(tornadoGO);

            tornadoGO.GetComponentInChildren<CapsuleShape>().enabled = true;

            if (info.tint != null)
            {
                var colour = info.tint.ToColor();
                foreach (var renderer in tornadoGO.GetComponentsInChildren<Renderer>())
                {
                    renderer.material.color = colour;
                    renderer.material.SetColor("_DetailColor", colour);
                    renderer.material.SetColor("_TintColor", colour);
                }
            }

            if (info.wanderRate != 0)
            {
                var wanderer = tornadoGO.AddComponent<NHTornadoWanderController>();
                wanderer.wanderRate = info.wanderRate;
                wanderer.wanderDegreesX = info.wanderDegreesX;
                wanderer.wanderDegreesZ = info.wanderDegreesZ;
                wanderer.sector = sector;
            }

            soundGO.SetActive(true);
            tornadoGO.SetActive(true);
        }
    }
}
