using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public static class TornadoBuilder
    {
        public static string tornadoParentName = "Tornados";

        public static void Make(GameObject go, Sector sector, PropModule.TornadoInfo info, bool hasClouds)
        {
            // If we are given elevation choose a random position
            Vector3 position;
            float elevation = 0f;

            if (info.position != null)
            {
                position = info.position;
                elevation = position.magnitude;
            }
            else if (info.elevation != 0f)
            {
                Logger.Log("Giving tornado random pos");
                position = UnityEngine.Random.insideUnitSphere * info.elevation;
                elevation = info.elevation;
            }
            else
            {
                Logger.LogError($"Couldn't make tornado for {go.name}: No elevation or position was given");
                return;
            }

            var upPrefab = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_SouthHemisphere/Sector_SouthPole/Sector_Observatory/Interactables_Observatory/MockUpTornado");
            var downPrefab = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_SouthHemisphere/Sector_SouthPole/Sector_Observatory/Interactables_Observatory/MockDownTornado");
            // Default radius is 40, height is 837.0669

            var tornadoGO = upPrefab.InstantiateInactive();
            tornadoGO.transform.parent = sector?.transform ?? go.transform;
            tornadoGO.transform.localPosition = Vector3.zero;
            var tornado = tornadoGO.GetComponent<TornadoController>();
            tornado.SetSector(sector);

            var height = 837.0669f;
            if (info.height != 0f) height = info.height;

            var width = 40f;
            if (info.width != 0f) width = info.width;

            tornado._bottomBone.localPosition = new Vector3(0, elevation, 0);
            tornado._midBone.localPosition = new Vector3(0, elevation + height / 2f, 0);
            tornado._topBone.localPosition = new Vector3(0, elevation + height, 0);

            tornado._startActive = false;

            // TODO make these settings
            tornado._wanderRate = 0.5f;
            tornado._wanderDegreesX = 45f;
            tornado._wanderDegreesZ = 45f;

            /*
            if (!hasClouds)
            {
                var fix = tornadoGO.AddComponent<TornadoFix>();
                fix.SetSector(sector);

                var top = tornadoGO.transform.Find("UpTornado/Effects_GD_TornadoCyclone/Tornado_Top");

                // Get rid of the bit that appears above the clouds
                GameObject.Destroy(top.transform.Find("Effects_GD_TornadoCloudCap_Large")?.gameObject);
                GameObject.Destroy(top.transform.Find("Effects_GD_TornadoCloudCap_Medium")?.gameObject);
                GameObject.Destroy(top.transform.Find("Effects_GD_TornadoCloudCap_Small")?.gameObject);

                var top_objects = new GameObject[3];
                top_objects[0] = GameObject.Instantiate(top.transform.Find("Effects_GD_TornadoCloudBlend_Large").gameObject, top.transform);
                top_objects[1] = GameObject.Instantiate(top.transform.Find("Effects_GD_TornadoCloudBlend_Medium").gameObject, top.transform);
                top_objects[2] = GameObject.Instantiate(top.transform.Find("Effects_GD_TornadoCloudBlend_Small").gameObject, top.transform);

                foreach(var obj in top_objects)
                {
                    obj.transform.localPosition = new Vector3(0, -20, 0);
                    obj.transform.localRotation = Quaternion.Euler(180, 0, 0);
                }
            }
            */

            tornadoGO.SetActive(true);
        }
    }
}
