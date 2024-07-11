using NewHorizons.Builder.Props.TranslatorText;
using NewHorizons.Components;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.WarpPad;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Utils;
using UnityEngine;


namespace NewHorizons.Builder.Props
{
    public static class WarpPadBuilder
    {
        private static GameObject _detailedReceiverPrefab;
        private static GameObject _receiverPrefab;
        private static GameObject _transmitterPrefab;
        private static GameObject _platformContainerPrefab;

        public static void InitPrefabs()
        {
            if (_platformContainerPrefab == null)
            {
                // Put this around the platforms without details 
                // Trifid is a Nomai ruins genius
                _platformContainerPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_SouthHemisphere/Sector_SouthPole/Sector_Observatory/Interactables_Observatory/Prefab_NOM_RemoteViewer/Structure_NOM_RemoteViewer")
                    .InstantiateInactive()
                    .Rename("Prefab_NOM_PlatformContainer")
                    .DontDestroyOnLoad();
                _platformContainerPrefab.transform.localScale = new Vector3(0.85f, 3f, 0.85f);
            }

            if (_detailedReceiverPrefab == null)
            {
                var thReceiverLamp = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Sector_NomaiCrater/Geometry_NomaiCrater/OtherComponentsGroup/Structure_NOM_WarpReceiver_TimberHearth_Lamp");
                var thReceiver = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Sector_NomaiCrater/Interactables_NomaiCrater/Prefab_NOM_WarpReceiver");

                _detailedReceiverPrefab = new GameObject("NomaiWarpReceiver");

                var detailedReceiver = thReceiver.InstantiateInactive().Rename("Prefab_NOM_WarpReceiver");
                detailedReceiver.transform.parent = _detailedReceiverPrefab.transform;
                detailedReceiver.transform.localPosition = Vector3.zero;
                detailedReceiver.transform.localRotation = Quaternion.identity;

                var lamp = thReceiverLamp.InstantiateInactive().Rename("Structure_NOM_WarpReceiver_Lamp");
                lamp.transform.parent = _detailedReceiverPrefab.transform;
                lamp.transform.localPosition = thReceiver.transform.InverseTransformPoint(thReceiverLamp.transform.position);
                lamp.transform.localRotation = thReceiver.transform.InverseTransformRotation(thReceiverLamp.transform.rotation);

                _detailedReceiverPrefab.SetActive(false);
                lamp.SetActive(true);
                detailedReceiver.SetActive(true);

                _detailedReceiverPrefab.DontDestroyOnLoad();

                Object.Destroy(_detailedReceiverPrefab.GetComponentInChildren<NomaiWarpStreaming>().gameObject);
            }

            if (_receiverPrefab == null)
            {
                _receiverPrefab = SearchUtilities.Find("SunStation_Body/Sector_SunStation/Sector_WarpModule/Interactables_WarpModule/Prefab_NOM_WarpReceiver")
                    .InstantiateInactive()
                    .Rename("Prefab_NOM_WarpReceiver")
                    .DontDestroyOnLoad();
                Object.Destroy(_receiverPrefab.GetComponentInChildren<NomaiWarpStreaming>().gameObject);

                var structure = _platformContainerPrefab.Instantiate().Rename("Structure_NOM_PlatformContainer");
                structure.transform.parent = _receiverPrefab.transform;
                structure.transform.localPosition = new Vector3(0, 0.8945f, 0);
                structure.transform.localRotation = Quaternion.identity;
                structure.SetActive(true);
            }

            if (_transmitterPrefab == null)
            {
                _transmitterPrefab = SearchUtilities.Find("TowerTwin_Body/Sector_TowerTwin/Sector_Tower_SS/Interactables_Tower_SS/Tower_SS_VisibleFrom_TowerTwin/Prefab_NOM_WarpTransmitter")
                    .InstantiateInactive()
                    .Rename("Prefab_NOM_WarpTransmitter")
                    .DontDestroyOnLoad();
                Object.Destroy(_transmitterPrefab.GetComponentInChildren<NomaiWarpStreaming>().gameObject);

                var structure = _platformContainerPrefab.Instantiate().Rename("Structure_NOM_PlatformContainer");
                structure.transform.parent = _transmitterPrefab.transform;
                structure.transform.localPosition = new Vector3(0, 0.8945f, 0);
                structure.transform.localRotation = Quaternion.identity;
                structure.SetActive(true);
            }
        }

        public static void Make(GameObject planetGO, Sector sector, IModBehaviour mod, NomaiWarpReceiverInfo info)
        {
            var detailInfo = new DetailInfo(info);
            var receiverObject = DetailBuilder.Make(planetGO, sector, mod, info.detailed ? _detailedReceiverPrefab : _receiverPrefab, detailInfo);

            NHLogger.Log($"Position is {detailInfo.position} was {info.position}");

            var receiver = receiverObject.GetComponentInChildren<NomaiWarpReceiver>();

            receiver._frequency = GetFrequency(info.frequency);

            if (string.IsNullOrEmpty(info.alignmentTargetBody))
            {
                receiver._alignmentTarget = planetGO?.transform;
            }
            else
            {
                Delay.FireOnNextUpdate(() =>
                {
                    var targetAO = AstroObjectLocator.GetAstroObject(info.alignmentTargetBody);
                    if (targetAO != null)
                    {
                        receiver._alignmentTarget = targetAO.transform;
                    }
                    else
                    {
                        NHLogger.LogError($"Could not find target body [{info.alignmentTargetBody}] for warp receiver.");
                        receiver._alignmentTarget = null;
                    }
                });
            }

            receiverObject.SetActive(true);

            if (info.computer != null)
            {
                CreateComputer(planetGO, sector, mod, info.computer, receiver);
            }
        }

        public static void Make(GameObject planetGO, Sector sector, IModBehaviour mod, NomaiWarpTransmitterInfo info)
        {
            var transmitterObject = DetailBuilder.Make(planetGO, sector, mod, _transmitterPrefab, new DetailInfo(info));

            var transmitter = transmitterObject.GetComponentInChildren<NomaiWarpTransmitter>();
            transmitter._frequency = GetFrequency(info.frequency);

            transmitter._alignmentWindow = info.alignmentWindow;

            transmitter._upsideDown = info.flipAlignment;

            transmitter.GetComponent<BoxShape>().enabled = true;

            // Prevents the transmitter from sending you straight back if you use the return function of the receiver #563
            transmitterObject.AddComponent<NomaiWarpTransmitterCooldown>();

            transmitterObject.SetActive(true);
        }

        private static void CreateComputer(GameObject planetGO, Sector sector, IModBehaviour mod, GeneralPropInfo computerInfo, NomaiWarpReceiver receiver)
        {
            var computerObject = DetailBuilder.Make(planetGO, sector, mod, TranslatorTextBuilder.ComputerPrefab, new DetailInfo(computerInfo));

            var computer = computerObject.GetComponentInChildren<NomaiComputer>();
            computer.SetSector(sector);

            Delay.FireOnNextUpdate(computer.ClearAllEntries);

            var computerLogger = computerObject.AddComponent<NomaiWarpComputerLogger>();
            computerLogger._warpReceiver = receiver;
            computerLogger.Awake(); // Redo awake because OnReceiveWarpedBody doesn't get added to otherwise

            computerObject.SetActive(true);
        }

        private static NomaiWarpPlatform.Frequency GetFrequency(string frequency)
        {
            if (!EnumUtils.TryParse<NomaiWarpPlatform.Frequency>(frequency, out var frequencyEnum))
            {
                frequencyEnum = EnumUtilities.Create<NomaiWarpPlatform.Frequency>(frequency);
            }
            return frequencyEnum;
        }
    }
}
