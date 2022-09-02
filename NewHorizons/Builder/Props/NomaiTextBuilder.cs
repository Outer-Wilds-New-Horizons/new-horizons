using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using Enum = System.Enum;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Random = UnityEngine.Random;
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

        private static void InitPrefabs()
        {
            // Just take every scroll and get the first arc
            var existingArcs = GameObject.FindObjectsOfType<ScrollItem>().Select(x => x?._nomaiWallText?.gameObject?.transform?.Find("Arc 1")?.gameObject).Where(x => x != null).ToArray();
            _arcPrefabs = new List<GameObject>();
            _childArcPrefabs = new List<GameObject>();
            foreach (var existingArc in existingArcs)
            {
                if (existingArc.GetComponent<MeshRenderer>().material.name.Contains("Child"))
                {
                    var arc = existingArc.InstantiateInactive();
                    arc.name = "Arc (Child)";
                    _childArcPrefabs.Add(arc);
                }
                else
                {
                    var arc = existingArc.InstantiateInactive();
                    arc.name = "Arc";
                    _arcPrefabs.Add(arc);
                }
            }

            var existingGhostArcs = GameObject.FindObjectsOfType<GhostWallText>().Select(x => x?._textLine?.gameObject).Where(x => x != null).ToArray();
            _ghostArcPrefabs = new List<GameObject>();
            foreach (var existingArc in existingGhostArcs)
            {
                var arc = existingArc.InstantiateInactive();
                arc.name = "Arc (Ghost)";
                _ghostArcPrefabs.Add(arc);
            }

            _scrollPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District2/Interactables_HangingCity_District2/Prefab_NOM_Scroll").InstantiateInactive();
            _scrollPrefab.name = "Prefab_NOM_Scroll";

            _computerPrefab = SearchUtilities.Find("VolcanicMoon_Body/Sector_VM/Interactables_VM/Prefab_NOM_Computer").InstantiateInactive();
            _computerPrefab.name = "Prefab_NOM_Computer";
            _computerPrefab.transform.rotation = Quaternion.identity;

            _preCrashComputerPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_EscapePodCrashSite/Sector_CrashFragment/EscapePod_Socket/Interactibles_EscapePod/Prefab_NOM_Vessel_Computer").InstantiateInactive();
            _preCrashComputerPrefab.name = "Prefab_NOM_Vessel_Computer";
            _preCrashComputerPrefab.transform.rotation = Quaternion.identity;

            _cairnPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_Crossroads/Interactables_Crossroads/Trailmarkers/Prefab_NOM_BH_Cairn_Arc (1)").InstantiateInactive();
            _cairnPrefab.name = "Prefab_NOM_Cairn";
            _cairnPrefab.transform.rotation = Quaternion.identity;

            _recorderPrefab = SearchUtilities.Find("Comet_Body/Prefab_NOM_Shuttle/Sector_NomaiShuttleInterior/Interactibles_NomaiShuttleInterior/Prefab_NOM_Recorder").InstantiateInactive();
            _recorderPrefab.name = "Prefab_NOM_Recorder";
            _recorderPrefab.transform.rotation = Quaternion.identity;

            _preCrashRecorderPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_EscapePodCrashSite/Sector_CrashFragment/Interactables_CrashFragment/Prefab_NOM_Recorder").InstantiateInactive();
            _preCrashRecorderPrefab.name = "Prefab_NOM_Recorder_Vessel";
            _preCrashRecorderPrefab.transform.rotation = Quaternion.identity;

            _trailmarkerPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District2/Interactables_HangingCity_District2/Prefab_NOM_Sign").InstantiateInactive();
            _trailmarkerPrefab.name = "Prefab_NOM_Trailmarker";
            _trailmarkerPrefab.transform.rotation = Quaternion.identity;
        }

        public static GameObject Make(GameObject planetGO, Sector sector, PropModule.NomaiTextInfo info, IModBehaviour mod)
        {
            if (_scrollPrefab == null) InitPrefabs();

            var xmlPath = File.ReadAllText(mod.ModHelper.Manifest.ModFolderPath + info.xmlFile);

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

                        nomaiWallTextObj.transform.position = planetGO.transform.TransformPoint(info.position);
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

                        customScroll.transform.position = planetGO.transform.TransformPoint(info.position ?? Vector3.zero);

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

                        computerObject.transform.position = planetGO.transform.TransformPoint(info?.position ?? Vector3.zero);

                        var up = computerObject.transform.position - planetGO.transform.position;
                        if (info.normal != null) up = planetGO.transform.TransformDirection(info.normal);
                        computerObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * computerObject.transform.rotation;

                        var computer = computerObject.GetComponent<NomaiComputer>();
                        computer.SetSector(sector);

                        computer._location = (NomaiText.Location)Enum.Parse(typeof(NomaiText.Location), Enum.GetName(typeof(PropModule.NomaiTextInfo.NomaiTextLocation), info.location));
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
                            position = info.position
                        };
                        var computerObject = DetailBuilder.Make(planetGO, sector, _preCrashComputerPrefab, detailInfo);
                        computerObject.SetActive(false);

                        if (!string.IsNullOrEmpty(info.rename))
                        {
                            computerObject.name = info.rename;
                        }

                        if (!string.IsNullOrEmpty(info.parentPath))
                        {
                            var newParent = planetGO.transform.Find(info.parentPath);
                            if (newParent != null)
                            {
                                computerObject.transform.SetParent(newParent, true);
                            }
                            else
                            {
                                Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                            }
                        }

                        var up = computerObject.transform.position - planetGO.transform.position;
                        if (info.normal != null) up = planetGO.transform.TransformDirection(info.normal);
                        computerObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * computerObject.transform.rotation;

                        var computer = computerObject.GetComponent<NomaiVesselComputer>();
                        computer.SetSector(sector);

                        computer._location = (NomaiText.Location)Enum.Parse(typeof(NomaiText.Location), Enum.GetName(typeof(PropModule.NomaiTextInfo.NomaiTextLocation), info.location));
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
                    {
                        var cairnObject = _cairnPrefab.InstantiateInactive();

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

                        cairnObject.transform.position = planetGO.transform.TransformPoint(info?.position ?? Vector3.zero);

                        if (info.rotation != null)
                        {
                            cairnObject.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(info.rotation));
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

                        nomaiWallText._location = (NomaiText.Location)Enum.Parse(typeof(NomaiText.Location), Enum.GetName(typeof(PropModule.NomaiTextInfo.NomaiTextLocation), info.location));
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
                            position = info.position
                        };
                        var recorderObject = DetailBuilder.Make(planetGO, sector, prefab, detailInfo);
                        recorderObject.SetActive(false);

                        if (!string.IsNullOrEmpty(info.rename))
                        {
                            recorderObject.name = info.rename;
                        }

                        recorderObject.transform.position = planetGO.transform.TransformPoint(info?.position ?? Vector3.zero);

                        if (info.rotation == null)
                        {
                            var up = recorderObject.transform.position - planetGO.transform.position;
                            recorderObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * recorderObject.transform.rotation;
                        }

                        var nomaiText = recorderObject.GetComponentInChildren<NomaiText>();
                        nomaiText.SetSector(sector);

                        nomaiText._location = (NomaiText.Location)Enum.Parse(typeof(NomaiText.Location), Enum.GetName(typeof(PropModule.NomaiTextInfo.NomaiTextLocation), info.location));
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

                        trailmarkerObject.transform.position = planetGO.transform.TransformPoint(info?.position ?? Vector3.zero);
                        trailmarkerObject.transform.localScale = Vector3.one * 0.75f;

                        if (info.rotation != null)
                        {
                            trailmarkerObject.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(info.rotation));
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

                        nomaiWallText._location = (NomaiText.Location)Enum.Parse(typeof(NomaiText.Location), Enum.GetName(typeof(PropModule.NomaiTextInfo.NomaiTextLocation), info.location));
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

            nomaiWallText._location = (NomaiText.Location)Enum.Parse(typeof(NomaiText.Location), Enum.GetName(typeof(PropModule.NomaiTextInfo.NomaiTextLocation), info.location));

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
    }
}
