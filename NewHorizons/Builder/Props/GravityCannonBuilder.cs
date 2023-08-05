using NewHorizons.Builder.Props.TranslatorText;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.Shuttle;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class GravityCannonBuilder
    {
        private static GameObject _interfacePrefab;
        private static GameObject _detailedPlatformPrefab, _platformPrefab;
        private static GameObject _orbPrefab;

        internal static void InitPrefab()
        {
            if (_interfacePrefab == null)
            {
                _interfacePrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Interactables_GravityCannon/Prefab_NOM_GravityCannonInterface")
                    .InstantiateInactive()
                    .Rename("Prefab_GravityCannon")
                    .DontDestroyOnLoad();
            }
            if (_detailedPlatformPrefab == null)
            {
                // Creating it in the original position so we can instantiate the other parts in the right relative positions
                var original = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Geometry_GravityCannon/ControlledByProxy_OPC/Structure_NOM_GravityCannon_BH");
                _detailedPlatformPrefab = original
                    .InstantiateInactive()
                    .Rename("Prefab_GravityCannonPlatform_Detailed")
                    .DontDestroyOnLoad();
                _detailedPlatformPrefab.transform.position = original.transform.position;
                _detailedPlatformPrefab.transform.rotation = original.transform.rotation;

                GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Interactables_GravityCannon/Prefab_NOM_ShuttleSocket"), _detailedPlatformPrefab.transform, true)
                    .Rename("ShuttleSocket");
                GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Interactables_GravityCannon/CannonForceVolume"), _detailedPlatformPrefab.transform, true)
                    .Rename("ForceVolume");
                GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Volumes_GravityCannon/CannonPlatformTrigger"), _detailedPlatformPrefab.transform, true)
                    .Rename("PlatformTrigger");
            }

            if (_platformPrefab == null)
            {
                _platformPrefab = _detailedPlatformPrefab
                    .InstantiateInactive()
                    .Rename("Prefab_GravityCannonPlatform")
                    .DontDestroyOnLoad();
                GameObject.Destroy(_platformPrefab.FindChild("Structure_NOM_GravityCannon_Collider"));
                GameObject.Destroy(_platformPrefab.FindChild("Structure_NOM_GravityCannon_Crystals"));
                GameObject.Destroy(_platformPrefab.FindChild("Structure_NOM_GravityCannon_Geo"));
            }

            if (_orbPrefab == null)
            {
                _orbPrefab = SearchUtilities.Find("Prefab_NOM_InterfaceOrb")
                    .InstantiateInactive()
                    .Rename("Prefab_NOM_InterfaceOrb")
                    .DontDestroyOnLoad();
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, GravityCannonInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_interfacePrefab == null || planetGO == null || sector == null || _detailedPlatformPrefab == null || _platformPrefab == null || _orbPrefab == null) return null;

            var detailInfo = new DetailInfo(info.controls) { keepLoaded = true };
            var gravityCannonObject = DetailBuilder.Make(planetGO, sector, _interfacePrefab, detailInfo);
            gravityCannonObject.SetActive(false);

            var gravityCannonController = gravityCannonObject.GetComponent<GravityCannonController>();
            gravityCannonController._shuttleID = ShuttleHandler.GetShuttleID(info.shuttleID);

            // Gravity controller checks string length instead of isnullorempty
            gravityCannonController._retrieveShipLogFact = info.retrieveReveal ?? string.Empty;
            gravityCannonController._launchShipLogFact = info.launchReveal ?? string.Empty;

            CreatePlatform(planetGO, sector, gravityCannonController, info);

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
            var orb = _orbPrefab.InstantiateInactive().Rename(_orbPrefab.name);
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

        private static GameObject CreatePlatform(GameObject planetGO, Sector sector, GravityCannonController gravityCannonController, GravityCannonInfo platformInfo)
        {
            var platform = DetailBuilder.Make(planetGO, sector, platformInfo.detailed ? _detailedPlatformPrefab : _platformPrefab, new DetailInfo(platformInfo) { keepLoaded = true });

            gravityCannonController._forceVolume = platform.FindChild("ForceVolume").GetComponent<DirectionalForceVolume>();
            gravityCannonController._platformTrigger = platform.FindChild("PlatformTrigger").GetComponent<OWTriggerVolume>();
            gravityCannonController._shuttleSocket = platform.FindChild("ShuttleSocket").transform;
            gravityCannonController._warpEffect = gravityCannonController._shuttleSocket.GetComponentInChildren<SingularityWarpEffect>();
            gravityCannonController._recallProxyGeometry = gravityCannonController._shuttleSocket.gameObject.FindChild("ShuttleRecallProxy");

            return platform;
        }
    }
}
