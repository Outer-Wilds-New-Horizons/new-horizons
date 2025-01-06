using NewHorizons.External;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.TranslatorText;
using NewHorizons.External.SerializableData;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.Geometry;
using NewHorizons.Utility.OWML;
using Newtonsoft.Json;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewHorizons.Builder.Props.TranslatorText
{
    public static class TranslatorTextBuilder
    {
        private static Material _ghostArcMaterial;
        private static Material _adultArcMaterial;
        private static Material _childArcMaterial;
        private static GameObject _scrollPrefab;
        public static GameObject ComputerPrefab { get; private set; }
        public static GameObject PreCrashComputerPrefab { get; private set; }
        private static GameObject _cairnBHPrefab;
        private static GameObject _cairnTHPrefab;
        private static GameObject _cairnCTPrefab;
        private static GameObject _recorderPrefab;
        private static GameObject _preCrashRecorderPrefab;
        private static GameObject _trailmarkerPrefab;

        private static Dictionary<NomaiTextArcInfo, GameObject> arcInfoToCorrespondingSpawnedGameObject = new Dictionary<NomaiTextArcInfo, GameObject>();
        public static GameObject GetSpawnedGameObjectByNomaiTextArcInfo(NomaiTextArcInfo arc)
        {
            if (!arcInfoToCorrespondingSpawnedGameObject.ContainsKey(arc)) return null;
            return arcInfoToCorrespondingSpawnedGameObject[arc];
        }

        private static Dictionary<TranslatorTextInfo, GameObject> conversationInfoToCorrespondingSpawnedGameObject = new Dictionary<TranslatorTextInfo, GameObject>();
        
        public static GameObject GetSpawnedGameObjectByTranslatorTextInfo(TranslatorTextInfo convo)
        {
            NHLogger.LogVerbose("Retrieving wall text obj for " + convo);
            if (!conversationInfoToCorrespondingSpawnedGameObject.ContainsKey(convo)) return null;
            return conversationInfoToCorrespondingSpawnedGameObject[convo];
        }

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;
            
            if (_adultArcMaterial == null)
            {
                _adultArcMaterial = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_Crossroads/Interactables_Crossroads/Trailmarkers/Prefab_NOM_BH_Cairn_Arc (2)/Props_TH_ClutterSmall/Arc_Short/Arc") 
                    .GetComponent<MeshRenderer>()
                    .sharedMaterial;
            }

            if (_childArcMaterial == null)
            {
                _childArcMaterial = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_OldSettlement/Fragment OldSettlement 5/Core_OldSettlement 5/Interactables_Core_OldSettlement5/Arc_BH_OldSettlement_ChildrensRhyme/Arc 1") 
                    .GetComponent<MeshRenderer>()
                    .sharedMaterial;
            }

            if (_ghostArcMaterial == null)
            {
                _ghostArcMaterial = SearchUtilities.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone1/Interactables_Zone1/Props_IP_ZoneSign_1/Arc_TestAlienWriting/Arc 1") 
                    .GetComponent<MeshRenderer>()
                    .sharedMaterial;
            }

            if (_scrollPrefab == null)
            {
                _scrollPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District2/Interactables_HangingCity_District2/Prefab_NOM_Scroll").InstantiateInactive().Rename("Prefab_NOM_Scroll").DontDestroyOnLoad();
            }

            if (ComputerPrefab == null)
            {
                ComputerPrefab = SearchUtilities.Find("VolcanicMoon_Body/Sector_VM/Interactables_VM/Prefab_NOM_Computer").InstantiateInactive().Rename("Prefab_NOM_Computer").DontDestroyOnLoad();
            }

            if (PreCrashComputerPrefab == null)
            {
                PreCrashComputerPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_EscapePodCrashSite/Sector_CrashFragment/EscapePod_Socket/Interactibles_EscapePod/Prefab_NOM_Vessel_Computer").InstantiateInactive().Rename("Prefab_NOM_Vessel_Computer").DontDestroyOnLoad();
            }

            if (_cairnBHPrefab == null)
            {
                _cairnBHPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_Crossroads/Interactables_Crossroads/Trailmarkers/Prefab_NOM_BH_Cairn_Arc (1)").InstantiateInactive().Rename("Prefab_NOM_BH_Cairn").DontDestroyOnLoad();
            }

            if (_cairnTHPrefab == null)
            {
                _cairnTHPrefab = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Sector_NomaiMines/Interactables_NomaiMines/Prefab_NOM_TH_Cairn_Arc").InstantiateInactive().Rename("Prefab_NOM_TH_Cairn").DontDestroyOnLoad();
            }

            if (_cairnCTPrefab == null)
            {
                _cairnCTPrefab = SearchUtilities.Find("CaveTwin_Body/Sector_CaveTwin/Sector_NorthHemisphere/Sector_NorthSurface/Sector_TimeLoopExperiment/Interactables_TimeLoopExperiment/Prefab_NOM_CT_Cairn_Arc").InstantiateInactive().Rename("Prefab_NOM_CT_Cairn").DontDestroyOnLoad();
            }

            if (_recorderPrefab == null)
            {
                _recorderPrefab = SearchUtilities.Find("Comet_Body/Prefab_NOM_Shuttle/Sector_NomaiShuttleInterior/Interactibles_NomaiShuttleInterior/Prefab_NOM_Recorder").InstantiateInactive().Rename("Prefab_NOM_Recorder").DontDestroyOnLoad();
            }

            if (_preCrashRecorderPrefab == null)
            {
                _preCrashRecorderPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_EscapePodCrashSite/Sector_CrashFragment/Interactables_CrashFragment/Prefab_NOM_Recorder").InstantiateInactive().Rename("Prefab_NOM_Recorder_Vessel").DontDestroyOnLoad();
            }

            if (_trailmarkerPrefab == null)
            {
                _trailmarkerPrefab = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District2/Interactables_HangingCity_District2/Prefab_NOM_Sign").InstantiateInactive().Rename("Prefab_NOM_Trailmarker").DontDestroyOnLoad();
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, TranslatorTextInfo info, NewHorizonsBody nhBody)
        {
            InitPrefabs();

            var xmlContent = !string.IsNullOrEmpty(info.xmlFile) ? File.ReadAllText(Path.Combine(nhBody.Mod.ModHelper.Manifest.ModFolderPath, info.xmlFile)) : null;

            if (xmlContent == null && info.type != NomaiTextType.Whiteboard)
            {
                NHLogger.LogError($"Failed to create translator text because {nameof(info.xmlFile)} was not set to a valid text .xml file path");
                return null;
            }

            return Make(planetGO, sector, info, nhBody, xmlContent);
        }

        public static GameObject Make(GameObject planetGO, Sector sector, TranslatorTextInfo info, NewHorizonsBody nhBody, string xmlContent)
        {
            switch (info.type)
            {
                case NomaiTextType.Wall:
                    {
                        var nomaiWallTextObj = MakeWallText(planetGO, sector, info, xmlContent, nhBody).gameObject;
                        nomaiWallTextObj = GeneralPropBuilder.MakeFromExisting(nomaiWallTextObj, planetGO, sector, info);

                        if (info.normal != null)
                        {
                            var up = (nomaiWallTextObj.transform.position - planetGO.transform.position).normalized;
                            var forward = planetGO.transform.TransformDirection(info.normal).normalized;

                            nomaiWallTextObj.transform.forward = forward;

                            var desiredUp = Vector3.ProjectOnPlane(up, forward);
                            var zRotation = Vector3.SignedAngle(nomaiWallTextObj.transform.up, desiredUp, forward);
                            nomaiWallTextObj.transform.RotateAround(nomaiWallTextObj.transform.position, forward, zRotation);
                        }

                        // nomaiWallTextObj.GetComponent<NomaiTextArcArranger>().DrawBoundsWithDebugSpheres();

                        nomaiWallTextObj.SetActive(true);
                        conversationInfoToCorrespondingSpawnedGameObject[info] = nomaiWallTextObj;
                        
                        return nomaiWallTextObj;
                    }
                case NomaiTextType.Scroll:
                    {
                        var customScroll = GeneralPropBuilder.MakeFromPrefab(_scrollPrefab, _scrollPrefab.name, planetGO, sector, info);

                        var nomaiWallText = MakeWallText(planetGO, sector, info, xmlContent, nhBody);
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

                        customScroll.SetActive(true);

                        Delay.FireOnNextUpdate(
                            () =>
                            {
                                NHLogger.LogVerbose("Fixing scroll!");
                                scrollItem._nomaiWallText = nomaiWallText;
                                scrollItem.SetSector(sector);
                                customScroll.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Geo").GetComponent<MeshRenderer>().enabled = true;
                                customScroll.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Collider").gameObject.SetActive(true);
                                nomaiWallText.gameObject.GetComponent<Collider>().enabled = false;
                                customScroll.GetComponent<CapsuleCollider>().enabled = true;
                                scrollItem._nomaiWallText.HideImmediate();
                                scrollItem._nomaiWallText._collider.SetActivation(true);
                                scrollItem.SetColliderActivation(true);
                            }
                        );

                        conversationInfoToCorrespondingSpawnedGameObject[info] = customScroll;
                        
                        return customScroll;
                    }
                case NomaiTextType.Computer:
                    {
                        var computerObject = GeneralPropBuilder.MakeFromPrefab(ComputerPrefab, ComputerPrefab.name, planetGO, sector, info);

                        var computer = computerObject.GetComponent<NomaiComputer>();
                        computer.SetSector(sector);

                        computer._location = EnumUtils.Parse<NomaiText.Location>(info.location.ToString());
                        computer._dictNomaiTextData = MakeNomaiTextDict(xmlContent);
                        computer._nomaiTextAsset = new TextAsset(xmlContent);
                        computer._nomaiTextAsset.name = Path.GetFileNameWithoutExtension(info.xmlFile);
                        AddTranslation(xmlContent);

                        // Make sure the computer model is loaded
                        StreamingHandler.SetUpStreaming(computerObject, sector);

                        computerObject.SetActive(true);
                        conversationInfoToCorrespondingSpawnedGameObject[info] = computerObject;
                        
                        return computerObject;
                    }
                case NomaiTextType.PreCrashComputer:
                    {
                        var computerObject = DetailBuilder.Make(planetGO, sector, nhBody.Mod, PreCrashComputerPrefab, new DetailInfo(info));
                        computerObject.SetActive(false);

                        var computer = computerObject.GetComponent<NomaiVesselComputer>();
                        computer.SetSector(sector);

                        computer._location = EnumUtils.Parse<NomaiText.Location>(info.location.ToString());
                        computer._dictNomaiTextData = MakeNomaiTextDict(xmlContent);
                        computer._nomaiTextAsset = new TextAsset(xmlContent);
                        computer._nomaiTextAsset.name = Path.GetFileNameWithoutExtension(info.xmlFile);
                        AddTranslation(xmlContent);

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
                        var cairnPrefab = info.type == NomaiTextType.CairnTimberHearth ? _cairnTHPrefab : (info.type == NomaiTextType.CairnEmberTwin ? _cairnCTPrefab : _cairnBHPrefab);
                        var cairnObject = GeneralPropBuilder.MakeFromPrefab(cairnPrefab, cairnPrefab.name, planetGO, sector, info);

                        // Idk do we have to set it active before finding things?
                        cairnObject.SetActive(true);

                        // Make it do the thing when it finishes being knocked over
                        // idk why, but sometimes stuff is null here, so just wait a frame to let it initialize
                        Delay.FireOnNextUpdate(() =>
                        {
                            foreach (var rock in cairnObject.GetComponent<NomaiCairn>()._rocks)
                            {
                                rock._returning = false;
                                rock._owCollider.SetActivation(true);
                                rock.enabled = false;
                            }
                        });

                        // So we can actually knock it over
                        cairnObject.GetComponent<CapsuleCollider>().enabled = true;

                        var nomaiWallText = cairnObject.transform.Find("Props_TH_ClutterSmall/Arc_Short").GetComponent<NomaiWallText>();
                        nomaiWallText.SetSector(sector);

                        nomaiWallText._location = EnumUtils.Parse<NomaiText.Location>(info.location.ToString());
                        nomaiWallText._dictNomaiTextData = MakeNomaiTextDict(xmlContent);
                        nomaiWallText._nomaiTextAsset = new TextAsset(xmlContent);
                        nomaiWallText._nomaiTextAsset.name = Path.GetFileNameWithoutExtension(info.xmlFile);
                        AddTranslation(xmlContent);

                        // Make sure the computer model is loaded
                        StreamingHandler.SetUpStreaming(cairnObject, sector);
                        conversationInfoToCorrespondingSpawnedGameObject[info] = cairnObject;

                        return cairnObject;
                    }
                case NomaiTextType.PreCrashRecorder:
                case NomaiTextType.Recorder:
                    {
                        var prefab = (info.type == NomaiTextType.PreCrashRecorder ? _preCrashRecorderPrefab : _recorderPrefab);
                        var recorderObject = DetailBuilder.Make(planetGO, sector, nhBody.Mod, prefab, new DetailInfo(info));
                        recorderObject.SetActive(false);

                        var nomaiText = recorderObject.GetComponentInChildren<NomaiText>();
                        nomaiText.SetSector(sector);

                        nomaiText._location = EnumUtils.Parse<NomaiText.Location>(info.location.ToString());
                        nomaiText._dictNomaiTextData = MakeNomaiTextDict(xmlContent);
                        nomaiText._nomaiTextAsset = new TextAsset(xmlContent);
                        nomaiText._nomaiTextAsset.name = Path.GetFileNameWithoutExtension(info.xmlFile);
                        AddTranslation(xmlContent);

                        recorderObject.SetActive(true);

                        recorderObject.transform.Find("InteractSphere").gameObject.GetComponent<SphereShape>().enabled = true;
                        conversationInfoToCorrespondingSpawnedGameObject[info] = recorderObject;
                        return recorderObject;
                    }
                case NomaiTextType.Trailmarker:
                    {
                        var trailmarkerObject = GeneralPropBuilder.MakeFromPrefab(_trailmarkerPrefab, _trailmarkerPrefab.name, planetGO, sector, info);

                        // shrink because that is what mobius does on all trailmarkers or else they are the size of the player
                        trailmarkerObject.transform.localScale = Vector3.one * 0.75f;

                        // Idk do we have to set it active before finding things?
                        trailmarkerObject.SetActive(true);

                        var nomaiWallText = trailmarkerObject.transform.Find("Arc_Short").GetComponent<NomaiWallText>();
                        nomaiWallText.SetSector(sector);

                        nomaiWallText._location = EnumUtils.Parse<NomaiText.Location>(info.location.ToString());
                        nomaiWallText._dictNomaiTextData = MakeNomaiTextDict(xmlContent);
                        nomaiWallText._nomaiTextAsset = new TextAsset(xmlContent);
                        nomaiWallText._nomaiTextAsset.name = Path.GetFileNameWithoutExtension(info.xmlFile);
                        AddTranslation(xmlContent);

                        // Make sure the model is loaded
                        StreamingHandler.SetUpStreaming(trailmarkerObject, sector);
                        conversationInfoToCorrespondingSpawnedGameObject[info] = trailmarkerObject;

                        return trailmarkerObject;
                    }
                case NomaiTextType.Whiteboard:
                    {
                        var whiteboardInfo = new DetailInfo(info)
                        {
                            path = "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District2/Interactables_HangingCity_District2/VisibleFrom_HangingCity/Props_NOM_Whiteboard (1)",
                            rename = info.rename ?? "Props_NOM_Whiteboard",
                        };
                        var whiteboardObject = DetailBuilder.Make(planetGO, sector, nhBody.Mod, whiteboardInfo);

                        // Spawn a scroll and insert it into the whiteboard, but only if text is provided
                        if (!string.IsNullOrEmpty(info.xmlFile))
                        {
                            var scrollSocket = whiteboardObject.GetComponentInChildren<ScrollSocket>();

                            var scrollInfo = new TranslatorTextInfo()
                            {
                                type = NomaiTextType.Scroll,
                                arcInfo = info.arcInfo,
                                seed = info.seed,
                                xmlFile = info.xmlFile,
                            };

                            var scrollObject = Make(planetGO, sector, scrollInfo, nhBody);
                            var scrollItem = scrollObject.GetComponent<ScrollItem>();

                            Delay.FireOnNextUpdate(() =>
                            {
                                scrollSocket.PlaceIntoSocket(scrollItem);
                            });
                        }

                        return whiteboardObject;
                    }
                default:
                    NHLogger.LogError($"Unsupported NomaiText type {info.type}");
                    return null;
            }
        }

        private static NomaiWallText MakeWallText(GameObject go, Sector sector, TranslatorTextInfo info, string xmlContent, NewHorizonsBody nhBody)
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

            var text = new TextAsset(xmlContent);

            // Text assets need a name to be used with VoiceMod
            text.name = Path.GetFileNameWithoutExtension(info.xmlFile);

            BuildArcs(xmlContent, nomaiWallText, nomaiWallTextObj, info, nhBody);
            AddTranslation(xmlContent);
            nomaiWallText._nomaiTextAsset = text;

            nomaiWallText.SetTextAsset(text);

            // #433 fuzzy stranger text
            if (info.arcInfo != null && info.arcInfo.Any(x => x.type == NomaiTextArcInfo.NomaiTextArcType.Stranger))
            {
                StreamingHandler.SetUpStreaming(AstroObject.Name.RingWorld, sector);
            }

            return nomaiWallText;
        }

        internal static void BuildArcs(string xml, NomaiWallText nomaiWallText, GameObject conversationZone, TranslatorTextInfo info, NewHorizonsBody nhBody)
        {
            var dict = MakeNomaiTextDict(xml);

            nomaiWallText._dictNomaiTextData = dict;

            var cacheKey = xml.GetHashCode() + " " + JsonConvert.SerializeObject(info).GetHashCode();
            RefreshArcs(nomaiWallText, conversationZone, info, nhBody, cacheKey);
        }

        [Serializable]
        private struct ArcCacheData 
        {
            public MMesh mesh;
            public MVector3[] skeletonPoints;
            public MVector3 position;
            public float zRotation;
            public bool mirrored;
        }

        internal static void RefreshArcs(NomaiWallText nomaiWallText, GameObject conversationZone, TranslatorTextInfo info, NewHorizonsBody nhBody, string cacheKey)
        {
            var dict = nomaiWallText._dictNomaiTextData;
            Random.InitState(info.seed == 0 ? info.xmlFile.GetHashCode() : info.seed);

            var arcsByID = new Dictionary<int, GameObject>();

            if (info.arcInfo != null && info.arcInfo.Count() != dict.Values.Count())
            {
                NHLogger.LogError($"Can't make NomaiWallText, arcInfo length [{info.arcInfo.Count()}] doesn't equal number of TextBlocks [{dict.Values.Count()}] in the xml");
                return;
            }

            ArcCacheData[] cachedData = null;
            if (nhBody?.Cache?.ContainsKey(cacheKey) ?? false)
                cachedData = nhBody.Cache.Get<ArcCacheData[]>(cacheKey);

            var arranger = nomaiWallText.gameObject.AddComponent<NomaiTextArcArranger>();

            // Generate spiral meshes/GOs

            var i = 0;
            foreach (var textData in dict.Values)
            {
                var arcInfo = info.arcInfo?.Length > i ? info.arcInfo[i] : null;
                var textEntryID = textData.ID;
                var parentID = textData.ParentID;

                var parent = parentID == -1 ? null : arcsByID[parentID];

                GameObject arcReadFromCache = null;
                if (cachedData != null) 
                {
                    var skeletonPoints = cachedData[i].skeletonPoints.Select(mv => (Vector3)mv).ToArray();
                    arcReadFromCache = NomaiTextArcBuilder.BuildSpiralGameObject(skeletonPoints, cachedData[i].mesh);
                    arcReadFromCache.transform.parent = arranger.transform;
                    arcReadFromCache.transform.localScale = new Vector3(cachedData[i].mirrored? -1 : 1, 1, 1);
                    arcReadFromCache.transform.localPosition = cachedData[i].position;
                    arcReadFromCache.transform.localEulerAngles = new Vector3(0, 0, cachedData[i].zRotation);
                }

                GameObject arc = MakeArc(arcInfo, conversationZone, parent, textEntryID, arcReadFromCache);
                arc.name = $"Arc {textEntryID} - Child of {parentID}";

                arcsByID.Add(textEntryID, arc);

                i++;
            }

            // no need to arrange if the cache exists
            if (cachedData == null)
            { 
                NHLogger.LogVerbose("Cache and/or cache entry was null, proceding with wall text arc arrangment.");

                // auto placement

                var overlapFound = true;
                for (var k = 0; k < arranger.spirals.Count*2; k++) 
                {
                    overlapFound = arranger.AttemptOverlapResolution();
                    if (!overlapFound) break;
                    for(var a = 0; a < 10; a++) arranger.FDGSimulationStep();
                }

                if (overlapFound) NHLogger.LogVerbose("Overlap resolution failed!");

                // manual placement

                for (var j = 0; j < info.arcInfo?.Length; j++) 
                {
                    var arcInfo = info.arcInfo[j];
                    var arc = arranger.spirals[j];

                    if (arcInfo.position != null || arcInfo.zRotation != null || arcInfo.mirror != null)
                    {
                        var pos = (Vector2)(arcInfo.position ?? Vector2.zero);
                        arc.transform.localPosition = new Vector3(pos.x, pos.y, 0);
                        arc.transform.localRotation = Quaternion.Euler(0, 0, arcInfo.zRotation.GetValueOrDefault());
                        arc.transform.localScale = arcInfo.mirror.GetValueOrDefault() ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
                    }
                }

                // make an entry in the cache for all these spirals

                if (nhBody?.Cache != null) 
                {
                    var cacheData = arranger.spirals.Select(spiralManipulator => new ArcCacheData() 
                    { 
                        mesh = spiralManipulator.GetComponent<MeshFilter>().sharedMesh,
                        skeletonPoints = spiralManipulator.NomaiTextLine._points.Select(v => (MVector3)v).ToArray(),
                        position = spiralManipulator.transform.localPosition,
                        zRotation = spiralManipulator.transform.localEulerAngles.z,
                        mirrored = spiralManipulator.transform.localScale.x < 0
                    }).ToArray();

                    nhBody.Cache.Set(cacheKey, cacheData);
                }
            }
        }

        internal static GameObject MakeArc(NomaiTextArcInfo arcInfo, GameObject conversationZone, GameObject parent, int textEntryID, GameObject prebuiltArc = null)
        {
            GameObject arc;
            var type = arcInfo != null ? arcInfo.type : NomaiTextArcInfo.NomaiTextArcType.Adult;
            NomaiTextArcBuilder.SpiralProfile profile;
            Material mat;
            Mesh overrideMesh = null;
            Color? overrideColor = null;
            switch (type)
            {
                case NomaiTextArcInfo.NomaiTextArcType.Child:
                    profile = NomaiTextArcBuilder.childSpiralProfile;
                    mat = _childArcMaterial;
                    break;
                case NomaiTextArcInfo.NomaiTextArcType.Teenager:
                    profile = NomaiTextArcBuilder.adultSpiralProfile;
                    mat = _childArcMaterial;
                    break;
                case NomaiTextArcInfo.NomaiTextArcType.Stranger when _ghostArcMaterial != null:
                    profile = NomaiTextArcBuilder.strangerSpiralProfile;
                    mat = _ghostArcMaterial;
                    overrideMesh = MeshUtilities.RectangleMeshFromCorners(new Vector3[]{ 
                        new Vector3(-0.9f, 0.0f, 0.0f), 
                        new Vector3(0.9f, 0.0f, 0.0f), 
                        new Vector3(-0.9f, 2.0f, 0.0f), 
                        new Vector3(0.9f, 2.0f, 0.0f) 
                    });
                    overrideColor = new Color(0.0158f, 1.0f, 0.5601f, 1f);
                    break;
                case NomaiTextArcInfo.NomaiTextArcType.Adult:
                default:
                    profile = NomaiTextArcBuilder.adultSpiralProfile;
                    mat = _adultArcMaterial;
                    break;
            }
            
            if (prebuiltArc != null) 
            {
                arc = prebuiltArc;
            }
            else 
            {
                if (parent != null) arc = parent.GetComponent<SpiralManipulator>().AddChild(profile).gameObject;
                else arc = NomaiTextArcArranger.CreateSpiral(profile, conversationZone).gameObject;
            }

            // Hardcoded stranger point fix
            if (type == NomaiTextArcInfo.NomaiTextArcType.Stranger)
            {
                Delay.FireOnNextUpdate(() =>
                {
                    var text = arc.GetComponent<NomaiTextLine>();
                    for (int i = 0; i < text._points.Length; i++)
                    {
                        text._points[i] = new Vector3(0f, 2f * i / text._points.Length, 0f);
                    }
                });
            }

            if (mat != null) arc.GetComponent<MeshRenderer>().sharedMaterial = mat;

            arc.transform.parent = conversationZone.transform;
            arc.GetComponent<NomaiTextLine>()._prebuilt = false;

            arc.GetComponent<NomaiTextLine>().SetEntryID(textEntryID);
            arc.GetComponent<MeshRenderer>().enabled = false;

            if (overrideMesh != null)
                arc.GetComponent<MeshFilter>().sharedMesh = overrideMesh;

            if (overrideColor != null)
                arc.GetComponent<NomaiTextLine>()._targetColor = (Color)overrideColor;

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
