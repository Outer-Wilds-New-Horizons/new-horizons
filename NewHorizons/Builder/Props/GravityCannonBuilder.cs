using NewHorizons.Builder.Props.TranslatorText;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.Shuttle;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace NewHorizons.Builder.Props
{
    public static class GravityCannonBuilder
    {
        private static GameObject _prefab;
        private static GameObject _orb;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                var original = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Interactables_GravityCannon/Prefab_NOM_GravityCannonInterface");
                _prefab = original?.InstantiateInactive()?.Rename("Prefab_GravityCannon")?.DontDestroyOnLoad();
                _prefab.transform.position = original.transform.position;
                _prefab.transform.rotation = original.transform.rotation;
                var gravityCannonController = _prefab.GetComponent<GravityCannonController>();
                gravityCannonController._shuttleID = NomaiShuttleController.ShuttleID.HourglassShuttle;
                gravityCannonController._shuttleSocket = GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Interactables_GravityCannon/Prefab_NOM_ShuttleSocket"), _prefab.transform, true).Rename("ShuttleSocket").transform;
                gravityCannonController._warpEffect = gravityCannonController._shuttleSocket.GetComponentInChildren<SingularityWarpEffect>();
                gravityCannonController._recallProxyGeometry = gravityCannonController._shuttleSocket.gameObject.FindChild("ShuttleRecallProxy");
                gravityCannonController._forceVolume = GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Interactables_GravityCannon/CannonForceVolume"), _prefab.transform, true).Rename("ForceVolume").GetComponent<DirectionalForceVolume>();
                gravityCannonController._platformTrigger = GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Volumes_GravityCannon/CannonPlatformTrigger"), _prefab.transform, true).Rename("PlatformTrigger").GetComponent<OWTriggerVolume>();
                gravityCannonController._nomaiComputer = null;
            }
            if (_orb == null)
            {
                _orb = SearchUtilities.Find("Prefab_NOM_InterfaceOrb")
                    .InstantiateInactive()
                    .Rename("Prefab_NOM_InterfaceOrb")
                    .DontDestroyOnLoad();
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, GravityCannonInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_prefab == null || planetGO == null || sector == null) return null;

            var detailInfo = new DetailInfo(info) { keepLoaded = true };
            var gravityCannonObject = DetailBuilder.Make(planetGO, sector, _prefab, detailInfo);
            gravityCannonObject.SetActive(false);

            var gravityCannonController = gravityCannonObject.GetComponent<GravityCannonController>();
            gravityCannonController._shuttleID = ShuttleHandler.GetShuttleID(info.shuttleID);

            // Gravity controller checks string length instead of isnullorempty
            gravityCannonController._retrieveShipLogFact = info.retrieveReveal ?? string.Empty;
            gravityCannonController._launchShipLogFact = info.launchReveal ?? string.Empty;

            if (info.computer != null)
            {
                gravityCannonController._nomaiComputer = CreateComputer(planetGO, sector, info.computer);
            }
            else
            {
                gravityCannonController._nomaiComputer = null;
            }

            CreateOrb(planetGO, gravityCannonController);

            gravityCannonObject.SetActive(true);

            return gravityCannonObject;
        }

        private static void CreateOrb(GameObject planetGO, GravityCannonController gravityCannonController)
        {
            var orb = _orb.InstantiateInactive().Rename(_orb.name);
            orb.transform.parent = gravityCannonController.transform;
            orb.transform.localPosition = new Vector3(0f, 0.9673f, 0f);
            orb.transform.localScale = Vector3.one;
            orb.SetActive(true);

            var planetBody = planetGO.GetComponent<OWRigidbody>();
            var orbBody = orb.GetComponent<OWRigidbody>();

            var nomaiInterfaceOrb = orb.GetComponent<NomaiInterfaceOrb>();
            nomaiInterfaceOrb._orbBody = orbBody;
            nomaiInterfaceOrb._slotRoot = gravityCannonController.gameObject;
            orbBody._origParent = planetGO.transform;
            orbBody._origParentBody = planetBody;
            nomaiInterfaceOrb._slots = nomaiInterfaceOrb._slotRoot.GetComponentsInChildren<NomaiInterfaceSlot>();
            nomaiInterfaceOrb.SetParentBody(planetBody);
            nomaiInterfaceOrb._safetyRails = new OWRail[0];
            nomaiInterfaceOrb.RemoveAllLocks();

            var spawnVelocity = planetBody.GetVelocity();
            var spawnAngularVelocity = planetBody.GetPointTangentialVelocity(orbBody.transform.position);
            var velocity = spawnVelocity + spawnAngularVelocity;

            orbBody._lastVelocity = velocity;
            orbBody._currentVelocity = velocity;

            orb.GetComponent<ConstantForceDetector>()._detectableFields = new ForceVolume[] { planetGO.GetComponentInChildren<GravityVolume>() };

            Delay.RunWhenAndInNUpdates(() =>
            {
                foreach (var component in orb.GetComponents<MonoBehaviour>())
                {
                    component.enabled = true;
                }
                nomaiInterfaceOrb.RemoveAllLocks();
            }, () => Main.IsSystemReady, 10);
        }

        private static NomaiComputer CreateComputer(GameObject planetGO, Sector sector, GeneralPropInfo computerInfo)
        {
            var computerObject = DetailBuilder.Make(planetGO, sector, TranslatorTextBuilder.ComputerPrefab, new DetailInfo(computerInfo));

            var computer = computerObject.GetComponentInChildren<NomaiComputer>();
            computer.SetSector(sector);

            Delay.FireOnNextUpdate(computer.ClearAllEntries);

            computerObject.SetActive(true);

            return computer;
        }
    }
}
