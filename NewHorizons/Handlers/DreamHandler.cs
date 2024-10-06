using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class DreamHandler
    {
        public static DreamArrivalPoint.Location GetDreamArrivalLocation(string id)
        {
            try
            {
                if (EnumUtils.TryParse(id, out DreamArrivalPoint.Location location))
                {
                    return location;
                }
                else
                {
                    NHLogger.LogVerbose($"Registering new dream arrival location [{id}]");
                    return EnumUtilities.Create<DreamArrivalPoint.Location>(id);
                }
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Couldn't load dream arrival location [{id}]:\n{e}");
                return DreamArrivalPoint.Location.Undefined;
            }
        }

        public static void MigrateDreamWorldController()
        {
            // Create new DreamWorldController instance since the existing one is disabled
            var managerObj = new GameObject("DreamWorldManager");
            managerObj.SetActive(false);
            var oldDWC = Locator.GetDreamWorldController();
            var dwc = managerObj.AddComponent<DreamWorldController>();

            var simRootObj = MigrateCopy(oldDWC._primarySimulationRoot.gameObject, managerObj);

            dwc._primarySimulationRoot = simRootObj.transform;
            dwc._simulationRoots = new Transform[] { simRootObj.transform };
            dwc._simulationCamera = simRootObj.FindChild("Camera_Simulation").GetComponent<SimulationCamera>();
            dwc._simulationSphere = simRootObj.FindChild("SimulationSphere").GetComponent<OWRenderer>();

            dwc._tempSkyboxColor = oldDWC._tempSkyboxColor;
            dwc._sarcophagusController = oldDWC._sarcophagusController;
            dwc._prisonerDirector = oldDWC._prisonerDirector;

            // These should correspond to the arrival point's attached body
            dwc._dreamBody = null;
            dwc._dreamWorldSector = null;
            dwc._dreamWorldVolume = null;

            // These should correspond to the campfire's attached body
            dwc._planetBody = null;
            dwc._ringWorldController = null;

            managerObj.SetActive(true);

            // Run after Start() completes
            Delay.FireOnNextUpdate(() =>
            {
                dwc._dreamBody = null;
                dwc._dreamWorldSector = null;
                dwc._dreamWorldVolume = null;
                dwc._planetBody = null;
                dwc._ringWorldController = null;

                // Dreamworld has a giant plane for simulation water, we don't want that in our custom world
                dwc._primarySimulationRoot.Find("water_simulation").gameObject.SetActive(false);
            });

        }

        private static GameObject MigrateCopy(GameObject go, GameObject newParent)
        {
            var clone = GameObject.Instantiate(go);
            clone.transform.SetParent(newParent.transform, false);
            return clone;
        }
    }
}
