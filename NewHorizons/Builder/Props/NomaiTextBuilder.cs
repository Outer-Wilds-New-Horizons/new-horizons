using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.TranslatorText;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

using Random = UnityEngine.Random;

namespace NewHorizons.Builder.Props
{
    /// <summary>
    /// Legacy - this class is used with the deprecated "nomaiText" module (deprecated on release of autospirals)
    /// </summary>
    [Obsolete]
    public static class NomaiTextBuilder
    {
        private static List<GameObject> _arcPrefabs;
        private static List<GameObject> _childArcPrefabs;
        private static GameObject _teenagerArcPrefab;
        private static List<GameObject> _ghostArcPrefabs;
        private static GameObject _scrollPrefab;
        private static GameObject _computerPrefab;
        private static GameObject _preCrashComputerPrefab;
        private static GameObject _cairnBHPrefab;
        private static GameObject _cairnTHPrefab;
        private static GameObject _cairnCTPrefab;
        private static GameObject _recorderPrefab;
        private static GameObject _preCrashRecorderPrefab;
        private static GameObject _trailmarkerPrefab;

        private static Dictionary<NomaiTextArcInfo, GameObject> arcInfoToCorrespondingSpawnedGameObject = new();
        public static GameObject GetSpawnedGameObjectByNomaiTextArcInfo(NomaiTextArcInfo arc)
        {
            if (!arcInfoToCorrespondingSpawnedGameObject.ContainsKey(arc)) return null;
            return arcInfoToCorrespondingSpawnedGameObject[arc];
        }

        private static Dictionary<NomaiTextInfo, GameObject> conversationInfoToCorrespondingSpawnedGameObject = new();
        
        public static GameObject GetSpawnedGameObjectByNomaiTextInfo(NomaiTextInfo convo)
        {
            NHLogger.LogVerbose("Retrieving wall text obj for " + convo);
            if (!conversationInfoToCorrespondingSpawnedGameObject.ContainsKey(convo)) return null;
            return conversationInfoToCorrespondingSpawnedGameObject[convo];
        }

        public static List<GameObject> GetArcPrefabs() { return _arcPrefabs; }
        public static List<GameObject> GetChildArcPrefabs() { return _childArcPrefabs; }
        public static GameObject GetTeenagerArcPrefab() { return _teenagerArcPrefab; }
        public static List<GameObject> GetGhostArcPrefabs() { return _ghostArcPrefabs; }

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_arcPrefabs == null || _childArcPrefabs == null)
            {
                // Just take every scroll and get the first arc
                var existingArcs = UnityEngine.Object.FindObjectsOfType<ScrollItem>()
                    .Select(x => x?._nomaiWallText?.gameObject?.transform?.Find("Arc 1")?.gameObject)
                    .Where(x => x != null)
                    .OrderBy(x => x.transform.GetPath()) // order by path so game updates dont break things
                    .ToArray();
                _arcPrefabs = new List<GameObject>();
                _childArcPrefabs = new List<GameObject>();
                foreach (var existingArc in existingArcs)
                {
                    if (existingArc.GetComponent<MeshRenderer>().material.name.Contains("Child"))
                    {
                        _childArcPrefabs.Add(existingArc.InstantiateInactive().Rename("Arc (Child)").DontDestroyOnLoad());
                    }
                    else
                    {
                        _arcPrefabs.Add(existingArc.InstantiateInactive().Rename("Arc").DontDestroyOnLoad());
                    }
                }
            }

            if (_teenagerArcPrefab == null)
            {
                _teenagerArcPrefab = GameObject.FindObjectsOfType<NomaiWallText>().FirstOrDefault(x => x.name.Equals("Arc_GD_StatueIsland_WindowNote"))
                    ?.gameObject?.transform?.Find("Arc 1")?.gameObject.InstantiateInactive().Rename("Arc (Teenager)").DontDestroyOnLoad();
            }

            if (_ghostArcPrefabs == null)
            {
                var existingGhostArcs = UnityEngine.Object.FindObjectsOfType<GhostWallText>()
                    .Select(x => x?._textLine?.gameObject)
                    .Where(x => x != null)
                    .OrderBy(x => x.transform.GetPath()) // order by path so game updates dont break things
                    .ToArray();
                _ghostArcPrefabs = new List<GameObject>();
                foreach (var existingArc in existingGhostArcs)
                {
                    _ghostArcPrefabs.Add(existingArc.InstantiateInactive().Rename("Arc (Ghost)").DontDestroyOnLoad());
                }
            }

            if (_scrollPrefab == null) _scrollPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District2/Interactables_HangingCity_District2/Prefab_NOM_Scroll").InstantiateInactive().Rename("Prefab_NOM_Scroll").DontDestroyOnLoad();

            if (_computerPrefab == null)
            {
                _computerPrefab = SearchUtilities.Find("VolcanicMoon_Body/Sector_VM/Interactables_VM/Prefab_NOM_Computer").InstantiateInactive().Rename("Prefab_NOM_Computer").DontDestroyOnLoad();
                _computerPrefab.transform.rotation = Quaternion.identity;
            }

            if (_preCrashComputerPrefab == null)
            {
                _preCrashComputerPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_EscapePodCrashSite/Sector_CrashFragment/EscapePod_Socket/Interactibles_EscapePod/Prefab_NOM_Vessel_Computer").InstantiateInactive().Rename("Prefab_NOM_Vessel_Computer").DontDestroyOnLoad();
                _preCrashComputerPrefab.transform.rotation = Quaternion.identity;
            }

            if (_cairnBHPrefab == null)
            {
                _cairnBHPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_Crossroads/Interactables_Crossroads/Trailmarkers/Prefab_NOM_BH_Cairn_Arc (1)").InstantiateInactive().Rename("Prefab_NOM_BH_Cairn").DontDestroyOnLoad();
                _cairnBHPrefab.transform.rotation = Quaternion.identity;
            }

            if (_cairnTHPrefab == null)
            {
                _cairnTHPrefab = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Sector_NomaiMines/Interactables_NomaiMines/Prefab_NOM_TH_Cairn_Arc").InstantiateInactive().Rename("Prefab_NOM_TH_Cairn").DontDestroyOnLoad();
                _cairnTHPrefab.transform.rotation = Quaternion.identity;
            }

            if (_cairnCTPrefab == null)
            {
                _cairnCTPrefab = SearchUtilities.Find("CaveTwin_Body/Sector_CaveTwin/Sector_NorthHemisphere/Sector_NorthSurface/Sector_TimeLoopExperiment/Interactables_TimeLoopExperiment/Prefab_NOM_CT_Cairn_Arc").InstantiateInactive().Rename("Prefab_NOM_CT_Cairn").DontDestroyOnLoad();
                _cairnCTPrefab.transform.rotation = Quaternion.identity;
            }

            if (_recorderPrefab == null)
            {
                _recorderPrefab = SearchUtilities.Find("Comet_Body/Prefab_NOM_Shuttle/Sector_NomaiShuttleInterior/Interactibles_NomaiShuttleInterior/Prefab_NOM_Recorder").InstantiateInactive().Rename("Prefab_NOM_Recorder").DontDestroyOnLoad();
                _recorderPrefab.transform.rotation = Quaternion.identity;
            }

            if (_preCrashRecorderPrefab == null)
            {
                _preCrashRecorderPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_EscapePodCrashSite/Sector_CrashFragment/Interactables_CrashFragment/Prefab_NOM_Recorder").InstantiateInactive().Rename("Prefab_NOM_Recorder_Vessel").DontDestroyOnLoad();
                _preCrashRecorderPrefab.transform.rotation = Quaternion.identity;
            }

            if (_trailmarkerPrefab == null)
            {
                _trailmarkerPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District2/Interactables_HangingCity_District2/Prefab_NOM_Sign").InstantiateInactive().Rename("Prefab_NOM_Trailmarker").DontDestroyOnLoad();
                _trailmarkerPrefab.transform.rotation = Quaternion.identity;
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, NomaiTextInfo info, IModBehaviour mod)
        {
            InitPrefabs();

            var xmlPath = File.ReadAllText(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, info.xmlFile));

            switch (info.type)
            {
                case NomaiTextType.Wall:
                    {
                        var nomaiWallTextObj = MakeWallText(planetGO, sector, info, xmlPath).gameObject;

                        if (!string.IsNullOrEmpty(info.rename))
                        {
                            nomaiWallTextObj.name = info.rename;
                        }

                        nomaiWallTextObj.transform.parent = sector?.transform ?? planetGO.transform;

                        if (!string.IsNullOrEmpty(info.parentPath))
                        {
                            var newParent = planetGO.transform.Find(info.parentPath);
                            if (newParent != null)
                            {
                                nomaiWallTextObj.transform.parent = newParent;
                            }
                            else
                            {
                                NHLogger.LogError($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                            }
                        }

                        var pos = (Vector3)(info.position ?? Vector3.zero);
                        if (info.isRelativeToParent)
                        {
                            nomaiWallTextObj.transform.localPosition = pos;
                            if (info.normal != null)
                            {
                                // In global coordinates (normal was in local coordinates)
                                var up = (nomaiWallTextObj.transform.position - planetGO.transform.position).normalized;
                                var forward = planetGO.transform.TransformDirection(info.normal).normalized;

                                nomaiWallTextObj.transform.up = up;
                                nomaiWallTextObj.transform.forward = forward;
                            }
                            if (info.rotation != null)
                            {
                                nomaiWallTextObj.transform.localRotation = Quaternion.Euler(info.rotation);
                            }
                        }
                        else
                        {
                            nomaiWallTextObj.transform.position = planetGO.transform.TransformPoint(pos);
                            if (info.normal != null)
                            {
                                // In global coordinates (normal was in local coordinates)
                                var up = (nomaiWallTextObj.transform.position - planetGO.transform.position).normalized;
                                var forward = planetGO.transform.TransformDirection(info.normal).normalized;

                                nomaiWallTextObj.transform.up = up;
                                nomaiWallTextObj.transform.forward = forward;
                            }
                            if (info.rotation != null)
                            {
                                nomaiWallTextObj.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(info.rotation));
                            }
                        }

                        nomaiWallTextObj.SetActive(true);
                        conversationInfoToCorrespondingSpawnedGameObject[info] = nomaiWallTextObj;
                        
                        return nomaiWallTextObj;
                    }
                case NomaiTextType.Scroll:
                    {
                        var customScroll = _scrollPrefab.InstantiateInactive();

                        if (!string.IsNullOrEmpty(info.rename))
                        {
                            customScroll.name = info.rename;
                        }
                        else
                        {
                            customScroll.name = _scrollPrefab.name;
                        }

                        var nomaiWallText = MakeWallText(planetGO, sector, info, xmlPath);
                        nomaiWallText.transform.parent = customScroll.transform;
                        nomaiWallText.transform.localPosition = Vector3.zero;
                        nomaiWallText.transform.localRotation = Quaternion.identity;

                        nomaiWallText._showTextOnStart = false;

                        // Don't want to be able to translate until its in a socket
                        nomaiWallText.GetComponent<Collider>().enabled = false;

                        nomaiWallText.gameObject.SetActive(true);

                        var scrollItem = customScroll.GetComponent<ScrollItem>();

                        // Idk why this thing is always around
                        UnityEngine.Object.Destroy(customScroll.transform.Find("Arc_BH_City_Forum_2").gameObject);

                        // This variable is the bane of my existence i dont get it
                        scrollItem._nomaiWallText = nomaiWallText;

                        // Because the scroll was already awake it does weird shit in Awake and makes some of the entries in this array be null
                        scrollItem._colliders = new OWCollider[] { scrollItem.GetComponent<OWCollider>() };

                        // Else when you put them down you can't pick them back up
                        customScroll.GetComponent<OWCollider>()._physicsRemoved = false;

                        // Place scroll
                        customScroll.transform.parent = sector?.transform ?? planetGO.transform;

                        if (!string.IsNullOrEmpty(info.parentPath))
                        {
                            var newParent = planetGO.transform.Find(info.parentPath);
                            if (newParent != null)
                            {
                                customScroll.transform.parent = newParent;
                            }
                            else
                            {
                                NHLogger.LogError($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                            }
                        }

                        var pos = (Vector3)(info.position ?? Vector3.zero);
                        if (info.isRelativeToParent) customScroll.transform.localPosition = pos;
                        else customScroll.transform.position = planetGO.transform.TransformPoint(pos);

                        var up = planetGO.transform.InverseTransformPoint(customScroll.transform.position).normalized;
                        if (info.rotation != null)
                        {
                            customScroll.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(info.rotation));
                        }
                        else
                        {
                            customScroll.transform.rotation = Quaternion.FromToRotation(customScroll.transform.up, up) * customScroll.transform.rotation;
                        }

                        customScroll.SetActive(true);

                        // Enable the collider and renderer
                        Delay.RunWhen(
                            () => Main.IsSystemReady,
                            () =>
                            {
                                NHLogger.LogVerbose("Fixing scroll!");
                                scrollItem._nomaiWallText = nomaiWallText;
                                scrollItem.SetSector(sector);
                                customScroll.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Geo").GetComponent<MeshRenderer>().enabled = true;
                                customScroll.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Collider").gameObject.SetActive(true);
                                nomaiWallText.gameObject.GetComponent<Collider>().enabled = false;
                                customScroll.GetComponent<CapsuleCollider>().enabled = true;
                            }
                        );
                        conversationInfoToCorrespondingSpawnedGameObject[info] = customScroll;
                        
                        return customScroll;
                    }
                case NomaiTextType.Computer:
                    {
                        var computerObject = _computerPrefab.InstantiateInactive();

                        if (!string.IsNullOrEmpty(info.rename))
                        {
                            computerObject.name = info.rename;
                        }
                        else
                        {
                            computerObject.name = _computerPrefab.name;
                        }

                        computerObject.transform.parent = sector?.transform ?? planetGO.transform;

                        if (!string.IsNullOrEmpty(info.parentPath))
                        {
                            var newParent = planetGO.transform.Find(info.parentPath);
                            if (newParent != null)
                            {
                                computerObject.transform.parent = newParent;
                            }
                            else
                            {
                                NHLogger.LogError($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                            }
                        }

                        var pos = (Vector3)(info.position ?? Vector3.zero);
                        if (info.isRelativeToParent) computerObject.transform.localPosition = pos;
                        else computerObject.transform.position = planetGO.transform.TransformPoint(pos);

                        var up = computerObject.transform.position - planetGO.transform.position;
                        if (info.normal != null) up = planetGO.transform.TransformDirection(info.normal);
                        computerObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * computerObject.transform.rotation;

                        var computer = computerObject.GetComponent<NomaiComputer>();
                        computer.SetSector(sector);

                        computer._location = EnumUtils.Parse<NomaiText.Location>(info.location.ToString());
                        computer._dictNomaiTextData = MakeNomaiTextDict(xmlPath);
                        computer._nomaiTextAsset = new TextAsset(xmlPath);
                        computer._nomaiTextAsset.name = Path.GetFileNameWithoutExtension(info.xmlFile);
                        AddTranslation(xmlPath);

                        // Make sure the computer model is loaded
                        StreamingHandler.SetUpStreaming(computerObject, sector);

                        computerObject.SetActive(true);
                        conversationInfoToCorrespondingSpawnedGameObject[info] = computerObject;
                        
                        return computerObject;
                    }
                case NomaiTextType.PreCrashComputer:
                    {
                        var computerObject = DetailBuilder.Make(planetGO, sector, mod, _preCrashComputerPrefab, new DetailInfo(info));
                        computerObject.SetActive(false);

                        var up = computerObject.transform.position - planetGO.transform.position;
                        if (info.normal != null) up = planetGO.transform.TransformDirection(info.normal);
                        computerObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * computerObject.transform.rotation;

                        var computer = computerObject.GetComponent<NomaiVesselComputer>();
                        computer.SetSector(sector);

                        computer._location = EnumUtils.Parse<NomaiText.Location>(info.location.ToString());
                        computer._dictNomaiTextData = MakeNomaiTextDict(xmlPath);
                        computer._nomaiTextAsset = new TextAsset(xmlPath);
                        computer._nomaiTextAsset.name = Path.GetFileNameWithoutExtension(info.xmlFile);
                        AddTranslation(xmlPath);

                        // Make fifth ring work
                        var fifthRingObject = computerObject.FindChild("Props_NOM_Vessel_Computer 1/Props_NOM_Vessel_Computer_Effects (4)");
                        fifthRingObject.SetActive(true);
                        var fifthRing = fifthRingObject.GetComponent<NomaiVesselComputerRing>();
                        //fifthRing._baseProjectorColor = new Color(1.4118, 1.5367, 4, 1);
                        //fifthRing._baseTextColor = new Color(0.8824, 0.9604, 2.5, 1);
                        //fifthRing._baseTextShadowColor = new Color(0.3529, 0.3843, 1, 0.25);
                        fifthRing._computer = computer;

                        computerObject.SetActive(true);

                        // All rings are rendered by detail builder so dont do that (have to wait for entries to be set)
                        Delay.FireOnNextUpdate(() =>
                        {
                            for (var i = computer.GetNumTextBlocks(); i < 5; i++)
                            {
                                var ring = computer._computerRings[i];
                                ring.gameObject.SetActive(false);
                            }
                        });

                        conversationInfoToCorrespondingSpawnedGameObject[info] = computerObject;
                        
                        return computerObject;
                    }
                case NomaiTextType.CairnBrittleHollow:
                case NomaiTextType.CairnTimberHearth:
                case NomaiTextType.CairnEmberTwin:
                    {
                        var cairnObject = (info.type == NomaiTextType.CairnTimberHearth ? _cairnTHPrefab : (info.type == NomaiTextType.CairnEmberTwin ? _cairnCTPrefab : _cairnBHPrefab)).InstantiateInactive();

                        if (!string.IsNullOrEmpty(info.rename))
                        {
                            cairnObject.name = info.rename;
                        }

                        cairnObject.transform.parent = sector?.transform ?? planetGO.transform;

                        if (!string.IsNullOrEmpty(info.parentPath))
                        {
                            var newParent = planetGO.transform.Find(info.parentPath);
                            if (newParent != null)
                            {
                                cairnObject.transform.parent = newParent;
                            }
                            else
                            {
                                NHLogger.LogError($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                            }
                        }

                        var pos = (Vector3)(info.position ?? Vector3.zero);
                        if (info.isRelativeToParent) cairnObject.transform.localPosition = pos;
                        else cairnObject.transform.position = planetGO.transform.TransformPoint(pos);

                        if (info.rotation != null)
                        {
                            var rot = Quaternion.Euler(info.rotation);
                            if (info.isRelativeToParent) cairnObject.transform.localRotation = rot;
                            else cairnObject.transform.rotation = planetGO.transform.TransformRotation(rot);
                        }
                        else
                        {
                            // By default align it to normal
                            var up = (cairnObject.transform.position - planetGO.transform.position).normalized;
                            cairnObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * cairnObject.transform.rotation;
                        }

                        // Idk do we have to set it active before finding things?
                        cairnObject.SetActive(true);

                        // Make it do the thing when it finishes being knocked over
                        foreach (var rock in cairnObject.GetComponent<NomaiCairn>()._rocks)
                        {
                            rock._returning = false;
                            rock._owCollider.SetActivation(true);
                            rock.enabled = false;
                        }

                        // So we can actually knock it over
                        cairnObject.GetComponent<CapsuleCollider>().enabled = true;

                        var nomaiWallText = cairnObject.transform.Find("Props_TH_ClutterSmall/Arc_Short").GetComponent<NomaiWallText>();
                        nomaiWallText.SetSector(sector);

                        nomaiWallText._location = EnumUtils.Parse<NomaiText.Location>(info.location.ToString());
                        nomaiWallText._dictNomaiTextData = MakeNomaiTextDict(xmlPath);
                        nomaiWallText._nomaiTextAsset = new TextAsset(xmlPath);
                        nomaiWallText._nomaiTextAsset.name = Path.GetFileNameWithoutExtension(info.xmlFile);
                        AddTranslation(xmlPath);

                        // Make sure the computer model is loaded
                        StreamingHandler.SetUpStreaming(cairnObject, sector);
                        conversationInfoToCorrespondingSpawnedGameObject[info] = cairnObject;

                        return cairnObject;
                    }
                case NomaiTextType.PreCrashRecorder:
                case NomaiTextType.Recorder:
                    {
                        var prefab = (info.type == NomaiTextType.PreCrashRecorder ? _preCrashRecorderPrefab : _recorderPrefab);
                        var recorderObject = DetailBuilder.Make(planetGO, sector, mod, prefab, new DetailInfo(info));
                        recorderObject.SetActive(false);

                        if (info.rotation == null)
                        {
                            var up = recorderObject.transform.position - planetGO.transform.position;
                            recorderObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * recorderObject.transform.rotation;
                        }

                        var nomaiText = recorderObject.GetComponentInChildren<NomaiText>();
                        nomaiText.SetSector(sector);

                        nomaiText._location = EnumUtils.Parse<NomaiText.Location>(info.location.ToString());
                        nomaiText._dictNomaiTextData = MakeNomaiTextDict(xmlPath);
                        nomaiText._nomaiTextAsset = new TextAsset(xmlPath);
                        nomaiText._nomaiTextAsset.name = Path.GetFileNameWithoutExtension(info.xmlFile);
                        AddTranslation(xmlPath);

                        recorderObject.SetActive(true);

                        recorderObject.transform.Find("InteractSphere").gameObject.GetComponent<SphereShape>().enabled = true;
                        conversationInfoToCorrespondingSpawnedGameObject[info] = recorderObject;
                        return recorderObject;
                    }
                case NomaiTextType.Trailmarker:
                    {
                        var trailmarkerObject = _trailmarkerPrefab.InstantiateInactive();

                        if (!string.IsNullOrEmpty(info.rename))
                        {
                            trailmarkerObject.name = info.rename;
                        }
                        else
                        {
                            trailmarkerObject.name = _trailmarkerPrefab.name;
                        }

                        trailmarkerObject.transform.parent = sector?.transform ?? planetGO.transform;

                        if (!string.IsNullOrEmpty(info.parentPath))
                        {
                            var newParent = planetGO.transform.Find(info.parentPath);
                            if (newParent != null)
                            {
                                trailmarkerObject.transform.parent = newParent;
                            }
                            else
                            {
                                NHLogger.LogError($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                            }
                        }

                        var pos = (Vector3)(info.position ?? Vector3.zero);
                        if (info.isRelativeToParent) trailmarkerObject.transform.localPosition = pos;
                        else trailmarkerObject.transform.position = planetGO.transform.TransformPoint(pos);

                        // shrink because that is what mobius does on all trailmarkers or else they are the size of the player
                        trailmarkerObject.transform.localScale = Vector3.one * 0.75f;

                        if (info.rotation != null)
                        {
                            var rot = Quaternion.Euler(info.rotation);
                            if (info.isRelativeToParent) trailmarkerObject.transform.localRotation = rot;
                            else trailmarkerObject.transform.rotation = planetGO.transform.TransformRotation(rot);
                        }
                        else
                        {
                            // By default align it to normal
                            var up = (trailmarkerObject.transform.position - planetGO.transform.position).normalized;
                            trailmarkerObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * trailmarkerObject.transform.rotation;
                        }

                        // Idk do we have to set it active before finding things?
                        trailmarkerObject.SetActive(true);

                        var nomaiWallText = trailmarkerObject.transform.Find("Arc_Short").GetComponent<NomaiWallText>();
                        nomaiWallText.SetSector(sector);

                        nomaiWallText._location = EnumUtils.Parse<NomaiText.Location>(info.location.ToString());
                        nomaiWallText._dictNomaiTextData = MakeNomaiTextDict(xmlPath);
                        nomaiWallText._nomaiTextAsset = new TextAsset(xmlPath);
                        nomaiWallText._nomaiTextAsset.name = Path.GetFileNameWithoutExtension(info.xmlFile);
                        AddTranslation(xmlPath);

                        // Make sure the model is loaded
                        StreamingHandler.SetUpStreaming(trailmarkerObject, sector);
                        conversationInfoToCorrespondingSpawnedGameObject[info] = trailmarkerObject;

                        return trailmarkerObject;
                    }
                default:
                    NHLogger.LogError($"Unsupported NomaiText type {info.type}");
                    return null;
            }
        }

        private static NomaiWallText MakeWallText(GameObject go, Sector sector, NomaiTextInfo info, string xmlPath)
        {
            GameObject nomaiWallTextObj = new GameObject("NomaiWallText");
            nomaiWallTextObj.SetActive(false);

            var box = nomaiWallTextObj.AddComponent<BoxCollider>();
            box.center = new Vector3(-0.0643f, 1.1254f, 0f);
            box.size = new Vector3(6.1424f, 5.2508f, 0.5f);

            box.isTrigger = true;

            nomaiWallTextObj.AddComponent<OWCollider>();

            var nomaiWallText = nomaiWallTextObj.AddComponent<NomaiWallText>();

            nomaiWallText._location = EnumUtils.Parse<NomaiText.Location>(info.location.ToString());

            var text = new TextAsset(xmlPath);

            // Text assets need a name to be used with VoiceMod
            text.name = Path.GetFileNameWithoutExtension(info.xmlFile);

            BuildArcs(xmlPath, nomaiWallText, nomaiWallTextObj, info);
            AddTranslation(xmlPath);
            nomaiWallText._nomaiTextAsset = text;

            nomaiWallText.SetTextAsset(text);

            // #433 fuzzy stranger text
            if (info.arcInfo != null && info.arcInfo.Any(x => x.type == NomaiTextArcInfo.NomaiTextArcType.Stranger))
            {
                StreamingHandler.SetUpStreaming(AstroObject.Name.RingWorld, sector);
            }

            return nomaiWallText;
        }

        internal static void BuildArcs(string xml, NomaiWallText nomaiWallText, GameObject conversationZone, NomaiTextInfo info)
        {
            var dict = MakeNomaiTextDict(xml);

            nomaiWallText._dictNomaiTextData = dict;

            RefreshArcs(nomaiWallText, conversationZone, info);
        }

        internal static void RefreshArcs(NomaiWallText nomaiWallText, GameObject conversationZone, NomaiTextInfo info)
        {
            var dict = nomaiWallText._dictNomaiTextData;
            Random.InitState(info.seed);

            var arcsByID = new Dictionary<int, GameObject>();

            if (info.arcInfo != null && info.arcInfo.Count() != dict.Values.Count())
            {
                NHLogger.LogError($"Can't make NomaiWallText, arcInfo length [{info.arcInfo.Count()}] doesn't equal text entries [{dict.Values.Count()}]");
                return;
            }

            var i = 0;
            foreach (var textData in dict.Values)
            {
                var arcInfo = info.arcInfo?.Length > i ? info.arcInfo[i] : null;
                var textEntryID = textData.ID;
                var parentID = textData.ParentID;

                var parent = parentID == -1 ? null : arcsByID[parentID];

                GameObject arc = MakeArc(arcInfo, conversationZone, parent, textEntryID);
                arc.name = $"Arc {i} - Child of {parentID}";

                arcsByID.Add(textEntryID, arc);

                i++;
            }
        }

        internal static GameObject MakeArc(NomaiTextArcInfo arcInfo, GameObject conversationZone, GameObject parent, int textEntryID)
        {
            GameObject arc;
            var type = arcInfo != null ? arcInfo.type : NomaiTextArcInfo.NomaiTextArcType.Adult;
            var variation = arcInfo != null ? arcInfo.variation : -1;
            switch (type)
            {
                case NomaiTextArcInfo.NomaiTextArcType.Child:
                    variation = variation < 0
                        ? Random.Range(0, _childArcPrefabs.Count())
                        : (variation % _childArcPrefabs.Count());
                    arc = _childArcPrefabs[variation].InstantiateInactive();
                    break;
                case NomaiTextArcInfo.NomaiTextArcType.Teenager:
                    arc = _teenagerArcPrefab.InstantiateInactive();
                    break;
                case NomaiTextArcInfo.NomaiTextArcType.Stranger when _ghostArcPrefabs.Any():
                    variation = variation < 0
                        ? Random.Range(0, _ghostArcPrefabs.Count())
                        : (variation % _ghostArcPrefabs.Count());
                    arc = _ghostArcPrefabs[variation].InstantiateInactive();
                    break;
                case NomaiTextArcInfo.NomaiTextArcType.Adult:
                default:
                    variation = variation < 0
                        ? Random.Range(0, _arcPrefabs.Count())
                        : (variation % _arcPrefabs.Count());
                    arc = _arcPrefabs[variation].InstantiateInactive();
                    break;
            }

            arc.transform.parent = conversationZone.transform;
            arc.GetComponent<NomaiTextLine>()._prebuilt = false;

            if (arcInfo != null)
            {
                arcInfo.variation = variation;
                if (arcInfo.position == null) arc.transform.localPosition = Vector3.zero;
                else arc.transform.localPosition = new Vector3(arcInfo.position.x, arcInfo.position.y, 0);

                arc.transform.localRotation = Quaternion.Euler(0, 0, arcInfo.zRotation.GetValueOrDefault());

                if (arcInfo.mirror.GetValueOrDefault()) arc.transform.localScale = new Vector3(-1, 1, 1);
            }
            // Try auto I guess
            else
            {
                if (parent == null)
                {
                    arc.transform.localPosition = Vector3.zero;
                }
                else
                {
                    var points = parent.GetComponent<NomaiTextLine>().GetPoints();
                    var point = points[points.Count() / 2];

                    arc.transform.localPosition = point;
                    arc.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
                }
            }

            arc.GetComponent<NomaiTextLine>().SetEntryID(textEntryID);
            arc.GetComponent<MeshRenderer>().enabled = false;

            arc.SetActive(true);

            if (arcInfo != null) arcInfoToCorrespondingSpawnedGameObject[arcInfo] = arc;

            return arc;
        }

        private static Dictionary<int, NomaiText.NomaiTextData> MakeNomaiTextDict(string xmlPath)
        {
            var dict = new Dictionary<int, NomaiText.NomaiTextData>();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlPath);
            XmlNode rootNode = xmlDocument.SelectSingleNode("NomaiObject");
            
            if (rootNode == null)
            {
                NHLogger.LogError($"Couldn't find NomaiObject in [{xmlPath}]");
                return dict;
            }

            foreach (object obj in rootNode.SelectNodes("TextBlock"))
            {
                XmlNode xmlNode = (XmlNode)obj;

                int textEntryID = -1;
                int parentID = -1;

                XmlNode textNode = xmlNode.SelectSingleNode("Text");
                XmlNode entryIDNode = xmlNode.SelectSingleNode("ID");
                XmlNode parentIDNode = xmlNode.SelectSingleNode("ParentID");

                if (entryIDNode != null && !int.TryParse(entryIDNode.InnerText, out textEntryID))
                {
                    NHLogger.LogError($"Couldn't parse int ID in [{entryIDNode?.InnerText}] for [{xmlPath}]");
                    textEntryID = -1;
                }

                if (parentIDNode != null && !int.TryParse(parentIDNode.InnerText, out parentID))
                {
                    NHLogger.LogError($"Couldn't parse int ParentID in [{parentIDNode?.InnerText}] for [{xmlPath}]");
                    parentID = -1;
                }

                NomaiText.NomaiTextData value = new NomaiText.NomaiTextData(textEntryID, parentID, textNode, false, NomaiText.Location.UNSPECIFIED);
                dict.Add(textEntryID, value);
            }
            return dict;
        }

        private static void AddTranslation(string xmlPath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlPath);

            XmlNode xmlNode = xmlDocument.SelectSingleNode("NomaiObject");
            XmlNodeList xmlNodeList = xmlNode.SelectNodes("TextBlock");

            foreach (object obj in xmlNodeList)
            {
                XmlNode xmlNode2 = (XmlNode)obj;
                var text = xmlNode2.SelectSingleNode("Text").InnerText;
                TranslationHandler.AddDialogue(text);
            }
        }
    }
}
