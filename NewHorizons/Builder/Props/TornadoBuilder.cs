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

        public static void Make(GameObject go, Sector sector, PropModule.TornadoInfo info)
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

            var scale = Vector3.one;
            var height = 837.0669f;
            if (info.scale != null) 
            {
                scale = new Vector3(info.scale.X / 40f, info.scale.Y / 837.0669f, info.scale.Z / 40f);
                height = info.scale.Y;
            }

            tornado.transform.localScale = scale;

            var tornadoController = tornado.GetComponent<TornadoController>();
            tornadoController.SetSector(sector);
            var n = position.normalized;
            tornadoController._bottomBone.localPosition = n * elevation;
            tornadoController._midBone.localPosition = n * (elevation + height/2f);
            tornadoController._topBone.localPosition = n * (elevation + height);

            tornadoController._snapBonesToSphere = true;
            tornadoController._wander = true;
            tornadoController._wanderRate = 0.02f;
            tornadoController._wanderDegreesX = 360f;
            tornadoController._wanderDegreesZ = 360f;

            /*
            tornadoController._formationDuration = 1f;
            tornadoController._collapseDuration = 1f;
            sector.OnOccupantEnterSector += ((sectorDetector) =>
                {
                    tornadoController.StartFormation();
                });
            sector.OnOccupantExitSector += ((sectorDetector) =>
                {
                    if (!sector.ContainsOccupant(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
                    {
                        tornadoController.StartCollapse();
                    }
                });
            */

            tornado.SetActive(true);
        }
    }
}
