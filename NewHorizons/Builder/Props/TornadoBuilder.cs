using NewHorizons.Components;
using NewHorizons.External;
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
                position = UnityEngine.Random.insideUnitSphere * info.elevation;
                elevation = info.elevation;
            }
            else
            {
                Logger.LogError($"Couldn't make tornado for {go.name}: No elevation or position was given");
                return;
            }

            var prefab = GameObject.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Tornadoes_GDInterior/MovingTornadoes/Root/UpTornado_Pivot (2)");

            // Default radius is 40, height is 837.0669

            var tornado = GameObject.Instantiate(prefab, sector.transform);
            tornado.SetActive(false);

            tornado.transform.localPosition = Vector3.zero;

            var height = 837.0669f;
            if (info.height != 0f) height = info.height;

            var width = 40f;
            if (info.width != 0f) width = info.width;

            var scale = new Vector3(width / 40f, height / 837.0669f, width / 40f);

            tornado.transform.localScale = scale;

            var tornadoController = tornado.GetComponent<TornadoController>();
            tornadoController.SetSector(sector);
            var n = position.normalized;
            var up = new Vector3(0, 1, 0);

            var h1 = elevation;
            var h2 = (elevation + height / 2f);
            var h3 = (elevation + height);

            tornadoController._bottomElevation = h1;
            tornadoController._bottomStartElevation = h1;
            tornadoController._bottomStartPos = n * h1;
            tornadoController._bottomBasePos = up * h1;
            tornadoController._bottomBone.localPosition = n * h1;

            tornadoController._midElevation = h2;
            tornadoController._midStartElevation = h2;
            tornadoController._midStartPos = n * h2;
            tornadoController._midBasePos = up * h2;
            tornadoController._midBone.localPosition = n * h2;

            tornadoController._topElevation = h3;
            tornadoController._topStartPos = n * h3;
            tornadoController._topBasePos = up * h3;
            tornadoController._topBone.localPosition = n * h3;

            tornadoController._snapBonesToSphere = true;
            tornadoController._wander = true;
            tornadoController._wanderRate = 0.02f;
            tornadoController._wanderDegreesX = 45f;
            tornadoController._wanderDegreesZ = 45f;

            if(!hasClouds)
            {
                var fix = tornado.AddComponent<TornadoFix>();
                fix.SetSector(sector);

                var top = tornado.transform.Find("UpTornado/Effects_GD_TornadoCyclone/Tornado_Top");

                Logger.Log($"{top.name}");

                // Get rid of the bit that appears above the clouds
                GameObject.Destroy(top.transform.Find("Effects_GD_TornadoCloudCap_Large")?.gameObject);
                GameObject.Destroy(top.transform.Find("Effects_GD_TornadoCloudCap_Medium")?.gameObject);
                GameObject.Destroy(top.transform.Find("Effects_GD_TornadoCloudCap_Small")?.gameObject);

                /*
                var top_objects = new GameObject[3];
                top_objects[0] = GameObject.Instantiate(top.transform.Find("Effects_GD_TornadoCloudBlend_Large").gameObject, top.transform);
                top_objects[1] = GameObject.Instantiate(top.transform.Find("Effects_GD_TornadoCloudBlend_Medium").gameObject, top.transform);
                top_objects[2] = GameObject.Instantiate(top.transform.Find("Effects_GD_TornadoCloudBlend_Small").gameObject, top.transform);

                foreach(var obj in top_objects)
                {
                    obj.transform.localPosition = new Vector3(0, -20, 0);
                    obj.transform.localRotation = Quaternion.Euler(180, 0, 0);
                }
                */
            }

            tornadoController._startActive = false;
            tornado.SetActive(true);
        }
    }
}
