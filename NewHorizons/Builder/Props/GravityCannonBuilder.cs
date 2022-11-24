using NewHorizons.Components;
using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public static class GravityCannonBuilder
    {
        private static GameObject _prefab;

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
        }

        public static GameObject Make(GameObject planetGO, Sector sector, PropModule.GravityCannonInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_prefab == null || planetGO == null || sector == null) return null;

            var detailInfo = new PropModule.DetailInfo()
            {
                position = info.position,
                rotation = info.rotation,
                parentPath = info.parentPath,
                isRelativeToParent = info.isRelativeToParent,
                rename = info.rename
            };
            var gravityCannonObject = DetailBuilder.Make(planetGO, sector, _prefab, detailInfo);
            gravityCannonObject.SetActive(false);

            StreamingHandler.SetUpStreaming(gravityCannonObject, sector);

            var gravityCannonController = gravityCannonObject.GetComponent<GravityCannonController>();
            gravityCannonController._shuttleID = ShuttleHandler.GetShuttleID(info.shuttleID);
            gravityCannonController._retrieveShipLogFact = info.retrieveReveal;
            gravityCannonController._launchShipLogFact = info.launchReveal;
            if (info.computer != null)
            {
                gravityCannonController._nomaiComputer = NomaiTextBuilder.Make(planetGO, sector, new PropModule.NomaiTextInfo
                {
                    type = PropModule.NomaiTextInfo.NomaiTextType.Computer,
                    position = info.computer.position,
                    rotation = info.computer.rotation,
                    normal = info.computer.normal,
                    isRelativeToParent = info.computer.isRelativeToParent,
                    rename = info.computer.rename,
                    location = info.computer.location,
                    xmlFile = info.computer.xmlFile,
                    parentPath = info.computer.parentPath
                }, mod).GetComponent<NomaiComputer>();
            }
            else
            {
                gravityCannonController._nomaiComputer = NomaiTextBuilder.Make(planetGO, sector, new PropModule.NomaiTextInfo
                {
                    type = PropModule.NomaiTextInfo.NomaiTextType.Computer,
                    position = new MVector3(-2.556838f, -0.8018004f, 10.01348f),
                    rotation = new MVector3(8.293f, 2.403f, 0.9f),
                    isRelativeToParent = true,
                    rename = "Computer",
                    xmlFile = "Assets/GravityCannonComputer.xml",
                    parentPath = gravityCannonObject.transform.GetPath().Remove(0, planetGO.name.Length + 1)
                }, Main.Instance).GetComponent<NomaiComputer>();
            }

            gravityCannonObject.SetActive(true);

            return gravityCannonObject;
        }
    }
}
