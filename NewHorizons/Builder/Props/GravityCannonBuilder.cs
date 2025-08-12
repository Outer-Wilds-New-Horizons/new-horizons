using NewHorizons.Builder.Props.TranslatorText;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.Shuttle;
using NewHorizons.External.Modules.TranslatorText;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using Newtonsoft.Json;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class GravityCannonBuilder
    {
        private static GameObject _interfacePrefab;
        private static GameObject _detailedPlatformPrefab, _platformPrefab;

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
                GameObject.DestroyImmediate(_platformPrefab.FindChild("Structure_NOM_GravityCannon_Collider"));
                GameObject.DestroyImmediate(_platformPrefab.FindChild("Structure_NOM_GravityCannon_Crystals"));
                GameObject.DestroyImmediate(_platformPrefab.FindChild("Structure_NOM_GravityCannon_Geo"));
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, GravityCannonInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_interfacePrefab == null || planetGO == null || sector == null || _detailedPlatformPrefab == null || _platformPrefab == null) return null;

            var planetSector = sector;

            var detailInfo = new DetailInfo(info.controls) { keepLoaded = true };
            var gravityCannonObject = DetailBuilder.Make(planetGO, ref sector, mod, _interfacePrefab, detailInfo);
            gravityCannonObject.SetActive(false);

            var gravityCannonController = gravityCannonObject.GetComponent<GravityCannonController>();
            var id = ShuttleHandler.GetShuttleID(info.shuttleID);
            gravityCannonController._shuttleID = id;

            // Gravity controller checks string length instead of isnullorempty
            gravityCannonController._retrieveShipLogFact = info.retrieveReveal ?? string.Empty;
            gravityCannonController._launchShipLogFact = info.launchReveal ?? string.Empty;

            CreatePlatform(planetGO, planetSector, mod, gravityCannonController, info);

            if (info.computer != null)
            {
                // Do it next update so that the shuttle has been made
                Delay.FireOnNextUpdate(() =>
                {
                    gravityCannonController._nomaiComputer = CreateComputer(planetGO, planetSector, info.computer, id);
                });
            }
            else
            {
                gravityCannonController._nomaiComputer = null;
            }

            gravityCannonObject.SetActive(true);

            return gravityCannonObject;
        }

        private static NomaiComputer CreateComputer(GameObject planetGO, Sector sector, GeneralPropInfo computerInfo, NomaiShuttleController.ShuttleID id)
        {
            // Load the position info from the GeneralPropInfo
            var translatorTextInfo = new TranslatorTextInfo();
            JsonConvert.PopulateObject(JsonConvert.SerializeObject(computerInfo), translatorTextInfo);
            translatorTextInfo.type = NomaiTextType.Computer;

            var shuttle = ShuttleBuilder.Shuttles[id];
            var planet = AstroObjectLocator.GetPlanetName(shuttle.GetComponentInParent<AstroObject>());

            var displayText = TranslationHandler.GetTranslation("NOMAI_SHUTTLE_COMPUTER", TranslationHandler.TextType.OTHER).Replace("{0}", planet);
            NHLogger.Log(displayText);
            var xmlContent = $"<NomaiObject>\r\n    <TextBlock>\r\n        <ID>1</ID>\r\n        <Text>{displayText}</Text>\r\n    </TextBlock>\r\n</NomaiObject>";

            var computerObject = TranslatorTextBuilder.Make(planetGO, sector, translatorTextInfo, null, xmlContent);

            var computer = computerObject.GetComponentInChildren<NomaiComputer>();

            computerObject.SetActive(true);

            return computer;
        }

        private static GameObject CreatePlatform(GameObject planetGO, Sector sector, IModBehaviour mod, GravityCannonController gravityCannonController, GravityCannonInfo platformInfo)
        {
            var platform = DetailBuilder.Make(planetGO, ref sector, mod, platformInfo.detailed ? _detailedPlatformPrefab : _platformPrefab, new DetailInfo(platformInfo) { keepLoaded = true });

            gravityCannonController._forceVolume = platform.FindChild("ForceVolume").GetComponent<DirectionalForceVolume>();
            gravityCannonController._platformTrigger = platform.FindChild("PlatformTrigger").GetComponent<OWTriggerVolume>();
            gravityCannonController._shuttleSocket = platform.FindChild("ShuttleSocket").transform;
            gravityCannonController._warpEffect = gravityCannonController._shuttleSocket.GetComponentInChildren<SingularityWarpEffect>();
            gravityCannonController._recallProxyGeometry = gravityCannonController._shuttleSocket.gameObject.FindChild("ShuttleRecallProxy");

            return platform;
        }
    }
}
