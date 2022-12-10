using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using Enum = System.Enum;
using Logger = NewHorizons.Utility.Logger;
using Random = UnityEngine.Random;
using OWML.Utils;

namespace NewHorizons.Builder.Props
{
    public static class NomaiTextBuilder
    {
        private static List<GameObject> _arcPrefabs;
        private static List<GameObject> _childArcPrefabs;
        private static List<GameObject> _ghostArcPrefabs;
        private static GameObject _scrollPrefab;
        private static GameObject _computerPrefab;
        private static GameObject _preCrashComputerPrefab;
        private static GameObject _cairnPrefab;
        private static GameObject _cairnVariantPrefab;
        private static GameObject _recorderPrefab;
        private static GameObject _preCrashRecorderPrefab;
        private static GameObject _trailmarkerPrefab;

        private static Dictionary<PropModule.NomaiTextArcInfo, GameObject> arcInfoToCorrespondingSpawnedGameObject = new Dictionary<PropModule.NomaiTextArcInfo, GameObject>();
        public static GameObject GetSpawnedGameObjectByNomaiTextArcInfo(PropModule.NomaiTextArcInfo arc)
        {
            if (!arcInfoToCorrespondingSpawnedGameObject.ContainsKey(arc)) return null;
            return arcInfoToCorrespondingSpawnedGameObject[arc];
        }

        private static Dictionary<PropModule.NomaiTextInfo, GameObject> conversationInfoToCorrespondingSpawnedGameObject = new Dictionary<PropModule.NomaiTextInfo, GameObject>();
        
        public static GameObject GetSpawnedGameObjectByNomaiTextInfo(PropModule.NomaiTextInfo convo)
        {
            Logger.LogVerbose("Retrieving wall text obj for " + convo);
            if (!conversationInfoToCorrespondingSpawnedGameObject.ContainsKey(convo)) return null;
            return conversationInfoToCorrespondingSpawnedGameObject[convo];
        }

        public static List<GameObject> GetArcPrefabs() { return _arcPrefabs; }
        public static List<GameObject> GetChildArcPrefabs() { return _childArcPrefabs; }
        public static List<GameObject> GetGhostArcPrefabs() { return _ghostArcPrefabs; }

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_arcPrefabs == null || _childArcPrefabs == null)
            {
                // Just take every scroll and get the first arc
                var existingArcs = GameObject.FindObjectsOfType<ScrollItem>()
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

            if (_ghostArcPrefabs == null)
            {
                var existingGhostArcs = GameObject.FindObjectsOfType<GhostWallText>()
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

            if (_cairnPrefab == null)
            {
                _cairnPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_Crossroads/Interactables_Crossroads/Trailmarkers/Prefab_NOM_BH_Cairn_Arc (1)").InstantiateInactive().Rename("Prefab_NOM_Cairn").DontDestroyOnLoad();
                _cairnPrefab.transform.rotation = Quaternion.identity;
            }

            if (_cairnVariantPrefab == null)
            {
                _cairnVariantPrefab = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Sector_NomaiMines/Interactables_NomaiMines/Prefab_NOM_TH_Cairn_Arc").InstantiateInactive().Rename("Prefab_NOM_Cairn").DontDestroyOnLoad();
                _cairnVariantPrefab.transform.rotation = Quaternion.identity;
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

        public static GameObject Make(GameObject planetGO, Sector sector, PropModule.NomaiTextInfo info, IModBehaviour mod)
        {
            InitPrefabs();

            var xmlPath = File.ReadAllText(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, info.xmlFile));

            switch (info.type)
            {
                case PropModule.NomaiTextInfo.NomaiTextType.Wall:
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
                                Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
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
                case PropModule.NomaiTextInfo.NomaiTextType.Scroll:
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
                        GameObject.Destroy(customScroll.transform.Find("Arc_BH_City_Forum_2").gameObject);

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
                                Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                            }
                        }

                        var pos = (Vector3)(info.position ?? Vector3.zero);
                        if (info.isRelativeToParent) customScroll.transform.localPosition = pos;
                        else customScroll.transform.position = planetGO.transform.TransformPoint(pos);

                        var up = planetGO.transform.InverseTransformPoint(customScroll.transform.position).normalized;
                        customScroll.transform.rotation = Quaternion.FromToRotation(customScroll.transform.up, up) * customScroll.transform.rotation;

                        customScroll.SetActive(true);

                        // Enable the collider and renderer
                        Delay.RunWhen(
                            () => Main.IsSystemReady,
                            () =>
                            {
                                Logger.LogVerbose("Fixing scroll!");
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
                case PropModule.NomaiTextInfo.NomaiTextType.Computer:
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
                                Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
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
                case PropModule.NomaiTextInfo.NomaiTextType.PreCrashComputer:
                    {
                        var detailInfo = new PropModule.DetailInfo()
                        {
                            position = info.position,
                            parentPath = info.parentPath,
                            isRelativeToParent = info.isRelativeToParent,
                            rename = info.rename
                        };
                        var computerObject = DetailBuilder.Make(planetGO, sector, _preCrashComputerPrefab, detailInfo);
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
                case PropModule.NomaiTextInfo.NomaiTextType.Cairn:
                case PropModule.NomaiTextInfo.NomaiTextType.CairnVariant:
                    {
                        var cairnObject = (info.type == PropModule.NomaiTextInfo.NomaiTextType.CairnVariant ? _cairnVariantPrefab : _cairnPrefab).InstantiateInactive();

                        if (!string.IsNullOrEmpty(info.rename))
                        {
                            cairnObject.name = info.rename;
                        }
                        else
                        {
                            cairnObject.name = _cairnPrefab.name;
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
                                Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
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
                case PropModule.NomaiTextInfo.NomaiTextType.PreCrashRecorder:
                case PropModule.NomaiTextInfo.NomaiTextType.Recorder:
                    {
                        var prefab = (info.type == PropModule.NomaiTextInfo.NomaiTextType.PreCrashRecorder ? _preCrashRecorderPrefab : _recorderPrefab);
                        var detailInfo = new PropModule.DetailInfo {
                            parentPath = info.parentPath,
                            rotation = info.rotation,
                            position = info.position,
                            isRelativeToParent = info.isRelativeToParent,
                            rename = info.rename
                        };
                        var recorderObject = DetailBuilder.Make(planetGO, sector, prefab, detailInfo);
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
                case PropModule.NomaiTextInfo.NomaiTextType.Trailmarker:
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
                                Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
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
                    Logger.LogError($"Unsupported NomaiText type {info.type}");
                    return null;
            }
        }

        private static NomaiWallText MakeWallText(GameObject go, Sector sector, PropModule.NomaiTextInfo info, string xmlPath)
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

            return nomaiWallText;
        }

        internal static void BuildArcs(string xml, NomaiWallText nomaiWallText, GameObject conversationZone, PropModule.NomaiTextInfo info)
        {
            var dict = MakeNomaiTextDict(xml);

            nomaiWallText._dictNomaiTextData = dict;

            RefreshArcs(nomaiWallText, conversationZone, info);
        }

        internal static void RefreshArcs(NomaiWallText nomaiWallText, GameObject conversationZone, PropModule.NomaiTextInfo info)
        {
            var dict = nomaiWallText._dictNomaiTextData;
            Random.InitState(info.seed);

            var arcsByID = new Dictionary<int, GameObject>();

            if (info.arcInfo != null && info.arcInfo.Count() != dict.Values.Count())
            {
                Logger.LogError($"Can't make NomaiWallText, arcInfo length [{info.arcInfo.Count()}] doesn't equal text entries [{dict.Values.Count()}]");
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

        internal static GameObject MakeArc(PropModule.NomaiTextArcInfo arcInfo, GameObject conversationZone, GameObject parent, int textEntryID)
        {
            GameObject arc;
            var type = arcInfo != null ? arcInfo.type : PropModule.NomaiTextArcInfo.NomaiTextArcType.Adult;
            var variation = arcInfo != null ? arcInfo.variation : -1;
            switch (type)
            {
                case PropModule.NomaiTextArcInfo.NomaiTextArcType.Child:
                    variation = variation < 0
                        ? Random.Range(0, _childArcPrefabs.Count())
                        : (variation % _childArcPrefabs.Count());
                    arc = _childArcPrefabs[variation].InstantiateInactive();
                    break;
                case PropModule.NomaiTextArcInfo.NomaiTextArcType.Stranger when _ghostArcPrefabs.Any():
                    variation = variation < 0
                        ? Random.Range(0, _ghostArcPrefabs.Count())
                        : (variation % _ghostArcPrefabs.Count());
                    arc = _ghostArcPrefabs[variation].InstantiateInactive();
                    break;
                case PropModule.NomaiTextArcInfo.NomaiTextArcType.Adult:
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

                arc.transform.localRotation = Quaternion.Euler(0, 0, arcInfo.zRotation);

                if (arcInfo.mirror) arc.transform.localScale = new Vector3(-1, 1, 1);
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
                    Logger.LogError($"Couldn't parse int ID in [{entryIDNode?.InnerText}] for [{xmlPath}]");
                    textEntryID = -1;
                }

                if (parentIDNode != null && !int.TryParse(parentIDNode.InnerText, out parentID))
                {
                    Logger.LogError($"Couldn't parse int ParentID in [{parentIDNode?.InnerText}] for [{xmlPath}]");
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
        //
        //
        // Handle the connection between game objects and spiral meshes
        //
        //

        // TODO: spiral profiles, pass as a value to constructor, use value in Randomize()
        // use current defaults to make an AdultSpiralProfile, then make ChildSpiralProfile and StrangerSpiralProfile
        public struct SpiralProfile
        {
            // all of the Vector2 params here refer to a range of valid values
            public bool canMirror;
            public Vector2 a;
            public Vector2 b;
            public Vector2 endS;
            public Vector2 skeletonScale;
            public int numSkeletonPoints;
            public float uvScale;
            public float innerWidth;
            public float outerWidth;
            public Material material;
        }

        public static SpiralProfile adultSpiralProfile = new SpiralProfile()
        {
            canMirror = true,
            a =      new Vector2(0.5f, 0.5f),
            b =      new Vector2(0.3f, 0.6f),
            endS =   new Vector2(0, 50f),
            skeletonScale = new Vector2(0.01f, 0.01f),
            numSkeletonPoints = 51,

            innerWidth = 0.001f, // width at the tip
            outerWidth = 0.05f, //0.107f; // width at the base
            uvScale = 4.9f, //2.9f;
        };

        public class SpiralGameObjectManager
        {
            public SpiralMesh spiralMesh;
            public GameObject gameObject;
            public PropModule.NomaiTextArcInfo config;
            public List<SpiralGameObjectManager> children = new List<SpiralGameObjectManager>();

            public SpiralGameObjectManager(SpiralProfile profile, GameObject parent)
            {
                gameObject = GameObject.Instantiate(_arcPrefabs[0]);
                gameObject.transform.parent = parent.transform;
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localEulerAngles = Vector3.zero;
                
                spiralMesh = new SpiralMesh(profile);
                spiralMesh.Randomize();
                spiralMesh.updateMesh();

                gameObject.GetComponent<MeshFilter>().sharedMesh = spiralMesh.mesh;
                gameObject.GetComponent<MeshRenderer>().enabled = false;

                gameObject.SetActive(true);
                
                //gameObject.AddComponent<MeshFilter>().sharedMesh = spiralMesh.mesh;
                //gameObject.AddComponent<MeshRenderer>().sharedMaterial = profile.material; //_arcPrefabs[0].GetComponent<MeshRenderer>().sharedMaterial;
                //var text = gameObject.AddComponent<NomaiTextLine>();
                //text._points = spiralMesh.skeleton.ToArray();
            }

            public void addChild(SpiralGameObjectManager child)
            {
                this.children.Add(child);
                this.spiralMesh.addChild(child.spiralMesh);
            }

            public void UpdateMesh()
            {
                this.spiralMesh.updateMesh();
                var text = gameObject.GetComponent<NomaiTextLine>();
                text._points = spiralMesh.skeleton.ToArray();
            }
            
            public void updateChildren() 
            {
                this.UpdateMesh();
                this.children.ForEach(child => {
                    this.spiralMesh.updateChild(child.spiralMesh, false);
                    child.updateChildren();
                });
            }
        }


        //
        //
        // Construct spiral meshes from the mathematical spirals generated below
        //
        //

        public class SpiralMesh : Spiral
        {
            public new List<SpiralMesh> children = new List<SpiralMesh>();

            public List<Vector3> skeleton;

            public int numSkeletonPoints = 51; // seems to be Mobius' default

            public float innerWidth = 0.001f; // width at the tip
            public float outerWidth = 0.05f;//0.107f; // width at the base
            public float uvScale = 4.9f; //2.9f;
            private float baseUVScale = 1f/300f; 
            public float uvOffset = 0;

            public Mesh mesh;

            
            public SpiralMesh(SpiralProfile profile) : base(profile)
            {
                this.numSkeletonPoints = profile.numSkeletonPoints;
                this.innerWidth        = profile.innerWidth;
                this.outerWidth        = profile.outerWidth;
                this.uvScale           = profile.uvScale;

                this.uvOffset = UnityEngine.Random.value;
            }

            public override void Randomize()
            {
                base.Randomize();
                uvOffset = UnityEngine.Random.value;
                updateChildren();
            }

            internal void updateMesh()
            {
                skeleton = this.getSkeleton(numSkeletonPoints);
                List<Vector3> vertsSide1 = skeleton.Select((skeletonPoint, index) => {
                    Vector3 normal = new Vector3(cos(skeletonPoint.z), 0, sin(skeletonPoint.z));
                    float width = lerp(((float)index) / ((float)skeleton.Count()), outerWidth, innerWidth);
                    
                    return new Vector3(skeletonPoint.x, 0, skeletonPoint.y) + width*normal;
                }).ToList();
                
                List<Vector3> vertsSide2 = skeleton.Select((skeletonPoint, index) => {
                    Vector3 normal = new Vector3(cos(skeletonPoint.z), 0, sin(skeletonPoint.z));
                    float width = lerp(((float)index) / ((float)skeleton.Count()), outerWidth, innerWidth);
                    
                    return new Vector3(skeletonPoint.x, 0, skeletonPoint.y) - width*normal;
                }).ToList();
                
                Vector3[] newVerts = vertsSide1.Zip(vertsSide2, (f, s) => new[] { f, s }).SelectMany(f => f).ToArray(); // interleave vertsSide1 and vertsSide2
                
                if (mesh != null && false)  // TODO: remove the && false
                {
                    mesh.vertices = newVerts;
                    mesh.RecalculateBounds();
                }
                else
                {   
                    List<int> triangles = new List<int>();
                    for (int i = 0; i < newVerts.Length-2; i+= 2)
                    {
                        /*  |  ⟍  |
                            |    ⟍|
                          2 *-----* 3                  
                            |⟍    |                   
                            |  ⟍  |        
                            |    ⟍|                   
                          0 *-----* 1       
                            |⟍    | 
                         */
                        triangles.Add(i+2);
                        triangles.Add(i+1);
                        triangles.Add(i);
            
                        triangles.Add(i+2);
                        triangles.Add(i+3);
                        triangles.Add(i+1);
                    }


                    var startT = tFromArcLen(startS);
                    var endT = tFromArcLen(endS);

                    var rangeT = endT-startT;    
                    var rangeS = endS-startS;
            
                    Vector2[] uvs = new Vector2[newVerts.Length];
                    Vector2[] uv2s = new Vector2[newVerts.Length];
                    for (int i = 0; i < skeleton.Count(); i++)
                    {
                        float fraction = 1- ((float)i)/((float)skeleton.Count()); // casting is so uuuuuuuugly

                        // note: cutting the sprial into numPoints equal slices of arclen would
                        // provide evenly spaced skeleton points
                        // on the other hand, cutting the spiral into numPoints equal slices of t
                        // will cluster points in areas of higher detail. this is the way Mobius does it, so it is the way we also will do it
                        float inputT = startT + rangeT*fraction;
                        float inputS = tToArcLen(inputT);
                        float sFraction = (inputS-startS)/rangeS;
                        float absoluteS = (inputS-startS);

                        float u = absoluteS * uvScale * baseUVScale + uvOffset;
                        uvs[i*2]   = new Vector2(u, 0);
                        uvs[i*2+1] = new Vector2(u, 1);

                        uv2s[i*2]   = new Vector2(1-sFraction, 0);
                        uv2s[i*2+1] = new Vector2(1-sFraction, 1);
                    }
                    
                    Vector3[] normals = new Vector3[newVerts.Length];
                    for (int i = 0; i < newVerts.Length; i++) normals[i] = new Vector3(0, 1, 0);
                    
                    
                    if (mesh == null) mesh = new Mesh(); // TODO: remove the if statement
                    mesh.vertices = newVerts.ToArray();
                    mesh.triangles = triangles.ToArray();
                    mesh.uv = uvs;
                    mesh.uv2 = uv2s;
                    mesh.normals = normals;
                    mesh.RecalculateBounds();
                }
            }
            
            internal void updateChild(SpiralMesh child, bool updateMesh = true)
            {
                Vector3 pointAndNormal = getDrawnSpiralPointAndNormal(child.startSOnParent); 
                var cx = pointAndNormal.x;
                var cy = pointAndNormal.y;
                var cang = pointAndNormal.z  + (this.mirror? -1:1) * Mathf.PI/2f; // if this spiral is mirrored, the child needs to be rotated by -90deg. if it's not, +90deg
                child.x = cx;
                child.y = cy;
                child.ang = cang+(child.mirror?Mathf.PI:0);
            
                if (updateMesh) child.updateMesh();
            }

            public void addChild(SpiralMesh child)
            {
                updateChild(child);
                this.children.Add(child);
            }

            
            public override void updateChildren() 
            {
                this.updateMesh();
                this.children.ForEach(child => {
                    updateChild(child, false);
                    child.updateChildren();
                });
            }
        }


        //
        //
        // Construct the mathematical spirals that Nomai arcs are built from
        //
        //
        
        public class Spiral {
            public bool mirror;
            public float a;
            public float b; // 0.3-0.6
            public float startSOnParent;
            public float scale;
            public List<Spiral> children;
            
            public float x;
            public float y;
            public float ang;


            // public float startIndex = 2.5f;

            public float startS = 42.87957f; // go all the way down to 0, all the way up to 50
            public float endS = 342.8796f;

            SpiralProfile profile;

            public Spiral(SpiralProfile profile)
            {
                this.profile = profile;
                
                this.Randomize();
            }

            public Spiral(float startSOnParent=0, bool mirror=false, float len=300, float a=0.5f, float b=0.43f, float scale=0.01f) 
            {
                this.mirror = mirror;
                this.a = a;
                this.b = b;
                this.startSOnParent = startSOnParent;
                this.scale = scale;

                this.children = new List<Spiral>();

                this.x = 0;
                this.y = 0;
                this.ang = 0;
            }

            public virtual void Randomize()
            {
                this.a = UnityEngine.Random.Range(profile.a.x, profile.a.y); //0.5f;
                this.b = UnityEngine.Random.Range(profile.b.x, profile.b.y);
                this.startS = UnityEngine.Random.Range(profile.endS.x, profile.endS.y);
                this.scale = UnityEngine.Random.Range(profile.skeletonScale.x, profile.skeletonScale.y);
                if (profile.canMirror) this.mirror = UnityEngine.Random.value < 0.5f;
            }

            internal virtual void updateChild(Spiral child)
            {
                Vector3 pointAndNormal = getDrawnSpiralPointAndNormal(child.startSOnParent);
                var cx = pointAndNormal.x;
                var cy = pointAndNormal.y;
                var cang = pointAndNormal.z;
                child.x = cx;
                child.y = cy;
                child.ang = cang+(child.mirror?Mathf.PI:0);
            }

            public virtual void addChild(Spiral child)
            {
                updateChild(child);
                this.children.Add(child);
            }

            public virtual void updateChildren() 
            {
                this.children.ForEach(child => {
                    updateChild(child);
                    child.updateChildren();
                });
            }

            // note: each Vector3 in this list is of form <x, y, angle in radians of the normal at this point>
            public List<Vector3> getSkeleton(int numPoints)
            {
                var endT = tFromArcLen(endS);
                var startT = tFromArcLen(startS);
                var rangeT = endT-startT;    

                List<Vector3> skeleton = new List<Vector3>();
                for (int i = 0; i < numPoints; i++)
                {
                    float fraction = ((float)i)/((float)numPoints-1f); // casting is so uuuuuuuugly

                    // note: cutting the sprial into numPoints equal slices of arclen would
                    // provide evenly spaced skeleton points
                    // on the other hand, cutting the spiral into numPoints equal slices of t
                    // will cluster points in areas of higher detail. this is the way Mobius does it, so it is the way we also will do it
                    float inputT = startT + rangeT*fraction;
                    float inputS = tToArcLen(inputT);

                    skeleton.Add(getDrawnSpiralPointAndNormal(inputS));
                }

                skeleton.Reverse();
                return skeleton;
            }

            

            // all of this math is based off of this:
            // https://www.desmos.com/calculator/9gdfgyuzf6
            //
            // note: t refers to theta, and s refers to arc length
            //
            

            // get the (x, y) coordinates and the normal angle at the given location (measured in arcLen) of a spiral with the given parameters 
            // note: arcLen is inverted so that 0 refers to what we consider the start of the Nomai spiral
            public Vector3 getDrawnSpiralPointAndNormal(float arcLen) {
                float offsetX = this.x; 
                float offsetY = this.y;
                float offsetAngle = this.ang;
                var startS = this.endS; // I know this is funky, but just go with it for now. 

                var startT = tFromArcLen(startS); // this is the `t` value for the root of the spiral (the end of the non-curled side)


                var startPoint = spiralPoint(startT); // and this is the (x,y) location of the non-curled side, relative to the rest of the spiral. we'll offset everything so this is at (0,0) later
                var startX = startPoint.x;
                var startY = startPoint.y;

                var t = tFromArcLen(arcLen);
                var point = spiralPoint(t);  // the absolute (x,y) location that corresponds to `arcLen`, before accounting for things like putting the start point at (0,0), or dealing with offsetX/offsetY/offsetAngle
                var x = point.x; 
                var y = point.y;
                var ang = normalAngle(t); 

                if (mirror) { 
                    x = x + 2*(startX-x);
                    ang = -ang+Mathf.PI;
                }     
    
                // translate so that startPoint is at (0,0)
                // (also scale the spiral)
                var retX = scale*(x-startX); 
                var retY = scale*(y-startY);

                // rotate offsetAngle rads 
                var retX2=retX*cos(offsetAngle)
                         -retY*sin(offsetAngle);
                var retY2=retX*sin(offsetAngle)        
                         +retY*cos(offsetAngle);

                retX = retX2;
                retY = retY2;

                // translate for offsetX, offsetY
                retX += offsetX;
                retY += offsetY;

                return new Vector3(retX, retY, ang+offsetAngle+Mathf.PI/2f);
            } 

            // the base formula for the spiral
            protected Vector2 spiralPoint(float t) {
                var r = a*exp(b*t);
                var retval = new Vector2(r*cos(t), r*sin(t));
                return retval;
            }

            // the spiral's got two functions: x(t) and y(t)
            // so it's got two derrivatives (with respect to t) x'(t) and y'(t)
            protected Vector2 spiralDerivative(float t) { // derrivative with respect to t
                var r = a*exp(b*t);
                return new Vector2(
                    -r*(sin(t)-b*cos(t)),
                     r*(b*sin(t)+cos(t))
                );
            }

            // returns the length of the spiral between t0 and t1
            protected float spiralArcLength(float t0, float t1) {
                return (a/b)*sqrt(b*b+1)*(exp(b*t1)-exp(b*t0));
            }

            // converts from a value of t to the equivalent value of s (the value of s that corresponds to the same point on the spiral as t)
            protected float tToArcLen(float t) {
                return spiralArcLength(0, t);
            }

            // reverse of above
            protected float tFromArcLen(float s) {
                return ln(
                        1+s/(
                            (a/b)*
                            sqrt(b*b+1)
                        )
                    )/b;
            }

            // returns the angle of the spiral's normal at a given point
            protected float normalAngle(float t) {
                var d = spiralDerivative(t);
                var n = new Vector2(d.y, -d.x);
                var angle = Mathf.Atan2(n.y, n.x);

                return angle-Mathf.PI/2;
            }
        }

        // convenience, so the math above is more readable
        private static float lerp(float a, float b, float t) {
            return a*t + b*(1-t);
        }
        
        private static float cos(float t) { return Mathf.Cos(t); }
        private static float sin(float t) { return Mathf.Sin(t); }
        private static float exp(float t) { return Mathf.Exp(t); }
        private static float sqrt(float t) { return Mathf.Sqrt(t); }
        private static float ln(float t) { return Mathf.Log(t); }
    }
}
