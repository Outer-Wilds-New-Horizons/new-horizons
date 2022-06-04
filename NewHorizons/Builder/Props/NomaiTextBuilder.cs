using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Random = UnityEngine.Random;
namespace NewHorizons.Builder.Props
{
    public static class NomaiTextBuilder
    {
        internal static List<GameObject> _arcPrefabs;
        internal static List<GameObject> _childArcPrefabs;
        internal static List<GameObject> _ghostArcPrefabs;
        private static GameObject _scrollPrefab;
        private static GameObject _computerPrefab;
        private static GameObject _cairnPrefab;
        private static GameObject _recorderPrefab;
        
        private static Dictionary<PropModule.NomaiTextArcInfo, GameObject> arcInfoToCorrespondingSpawnedGameObject = new Dictionary<PropModule.NomaiTextArcInfo, GameObject>();
        public static GameObject GetSpawnedGameObjectByNomaiTextArcInfo(PropModule.NomaiTextArcInfo arc)
        {
            if (!arcInfoToCorrespondingSpawnedGameObject.ContainsKey(arc)) return null;
            return arcInfoToCorrespondingSpawnedGameObject[arc];
        }
        
        private static Dictionary<PropModule.NomaiTextInfo, GameObject> conversationInfoToCorrespondingSpawnedGameObject = new Dictionary<PropModule.NomaiTextInfo, GameObject>();
        public static GameObject GetSpawnedGameObjectByNomaiTextInfo(PropModule.NomaiTextInfo convo)
        {
            Logger.Log("retrieving wall text obj for " + convo);
            if (!conversationInfoToCorrespondingSpawnedGameObject.ContainsKey(convo)) return null;
            return conversationInfoToCorrespondingSpawnedGameObject[convo];
        }


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
                arc.name = "Arc";
                _ghostArcPrefabs.Add(arc);
            }

            _scrollPrefab = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District2/Interactables_HangingCity_District2/Prefab_NOM_Scroll").InstantiateInactive();
            _scrollPrefab.name = "Prefab_NOM_Scroll";

            _computerPrefab = GameObject.Find("VolcanicMoon_Body/Sector_VM/Interactables_VM/Prefab_NOM_Computer").InstantiateInactive();
            _computerPrefab.name = "Prefab_NOM_Computer";
            _computerPrefab.transform.rotation = Quaternion.identity;

            _cairnPrefab = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_Crossroads/Interactables_Crossroads/Trailmarkers/Prefab_NOM_BH_Cairn_Arc (1)").InstantiateInactive();
            _cairnPrefab.name = "Prefab_NOM_Cairn";
            _cairnPrefab.transform.rotation = Quaternion.identity;

            _recorderPrefab = GameObject.Find("Comet_Body/Prefab_NOM_Shuttle/Sector_NomaiShuttleInterior/Interactibles_NomaiShuttleInterior/Prefab_NOM_Recorder").InstantiateInactive();
            _recorderPrefab.name = "Prefab_NOM_Recorder";
            _recorderPrefab.transform.rotation = Quaternion.identity;
        }

        public static void Make(GameObject planetGO, Sector sector, PropModule.NomaiTextInfo info, IModBehaviour mod)
        {
            if (_scrollPrefab == null) InitPrefabs();

            var xmlPath = System.IO.File.ReadAllText(mod.ModHelper.Manifest.ModFolderPath + info.xmlFile);

            switch (info.type)
            {
                case PropModule.NomaiTextInfo.NomaiTextType.Wall:
                {
                    var nomaiWallTextObj = MakeWallText(planetGO, sector, info, xmlPath).gameObject;

                    nomaiWallTextObj.transform.parent = sector?.transform ?? planetGO.transform;
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
                    break;
                }
                case PropModule.NomaiTextInfo.NomaiTextType.Scroll:
                {
                    var customScroll = _scrollPrefab.InstantiateInactive();

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
                    customScroll.transform.position = planetGO.transform.TransformPoint(info.position ?? Vector3.zero);

                    var up = planetGO.transform.InverseTransformPoint(customScroll.transform.position).normalized;
                    customScroll.transform.rotation = Quaternion.FromToRotation(customScroll.transform.up, up) * customScroll.transform.rotation;

                    customScroll.SetActive(true);

                    // Enable the collider and renderer
                    Main.Instance.ModHelper.Events.Unity.RunWhen(
                        () => Main.IsSystemReady,
                        () =>
                        {
                            Logger.Log("Fixing scroll!");
                            scrollItem._nomaiWallText = nomaiWallText;
                            scrollItem.SetSector(sector);
                            customScroll.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Geo").GetComponent<MeshRenderer>().enabled = true;
                            customScroll.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Collider").gameObject.SetActive(true);
                            nomaiWallText.gameObject.GetComponent<Collider>().enabled = false;
                            customScroll.GetComponent<CapsuleCollider>().enabled = true;
                        }
                    );
                    conversationInfoToCorrespondingSpawnedGameObject[info] = customScroll;
                    break;
                }
                case PropModule.NomaiTextInfo.NomaiTextType.Computer:
                {
                    var computerObject = _computerPrefab.InstantiateInactive();

                    computerObject.transform.parent = sector?.transform ?? planetGO.transform;
                    computerObject.transform.position = planetGO.transform.TransformPoint(info?.position ?? Vector3.zero);

                    var up = computerObject.transform.position - planetGO.transform.position;
                    if (info.normal != null) up = planetGO.transform.TransformDirection(info.normal);
                    computerObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * computerObject.transform.rotation;

                    var computer = computerObject.GetComponent<NomaiComputer>();
                    computer.SetSector(sector);

                    computer._dictNomaiTextData = MakeNomaiTextDict(xmlPath);
                    computer._nomaiTextAsset = new TextAsset(xmlPath);
                    AddTranslation(xmlPath);

                    // Make sure the computer model is loaded
                    OWAssetHandler.LoadObject(computerObject);
                    sector.OnOccupantEnterSector.AddListener((x) => OWAssetHandler.LoadObject(computerObject));

                    computerObject.SetActive(true);
                    conversationInfoToCorrespondingSpawnedGameObject[info] = computerObject;
                    break;
                }
                case PropModule.NomaiTextInfo.NomaiTextType.Cairn:
                {
                    var cairnObject = _cairnPrefab.InstantiateInactive();

                    cairnObject.transform.parent = sector?.transform ?? planetGO.transform;
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

                    nomaiWallText._dictNomaiTextData = MakeNomaiTextDict(xmlPath);
                    nomaiWallText._nomaiTextAsset = new TextAsset(xmlPath);
                    AddTranslation(xmlPath);

                    // Make sure the computer model is loaded
                    OWAssetHandler.LoadObject(cairnObject);
                    sector.OnOccupantEnterSector.AddListener((x) => OWAssetHandler.LoadObject(cairnObject));
                    conversationInfoToCorrespondingSpawnedGameObject[info] = cairnObject;
                    break;
                }
                case PropModule.NomaiTextInfo.NomaiTextType.Recorder:
                {
                    var recorderObject = _recorderPrefab.InstantiateInactive();

                    recorderObject.transform.parent = sector?.transform ?? planetGO.transform;
                    recorderObject.transform.position = planetGO.transform.TransformPoint(info?.position ?? Vector3.zero);

                    if (info.rotation != null)
                    {
                        recorderObject.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(info.rotation));
                    }
                    else
                    {
                        var up = recorderObject.transform.position - planetGO.transform.position;
                        recorderObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * recorderObject.transform.rotation;
                    }

                    var nomaiText = recorderObject.GetComponentInChildren<NomaiText>();
                    nomaiText.SetSector(sector);

                    nomaiText._dictNomaiTextData = MakeNomaiTextDict(xmlPath);
                    nomaiText._nomaiTextAsset = new TextAsset(xmlPath);
                    AddTranslation(xmlPath);

                    // Make sure the recorder model is loaded
                    OWAssetHandler.LoadObject(recorderObject);
                    sector.OnOccupantEnterSector.AddListener((x) => OWAssetHandler.LoadObject(recorderObject));

                    recorderObject.SetActive(true);

                    recorderObject.transform.Find("InteractSphere").gameObject.GetComponent<SphereShape>().enabled = true;
                    conversationInfoToCorrespondingSpawnedGameObject[info] = recorderObject;
                    break;
                }
                default:
                    Logger.LogError($"Unsupported NomaiText type {info.type}");
                    break;
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

            var text = new TextAsset(xmlPath);

            BuildArcs(xmlPath, nomaiWallText, nomaiWallTextObj, info);
            AddTranslation(xmlPath);
            nomaiWallText._nomaiTextAsset = text;

            nomaiWallText.SetTextAsset(text);

            return nomaiWallText;
        }

        private static void BuildArcs(string xml, NomaiWallText nomaiWallText, GameObject conversationZone, PropModule.NomaiTextInfo info)
        {
            var dict = MakeNomaiTextDict(xml);

            nomaiWallText._dictNomaiTextData = dict;

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
            var variation = arcInfo.variation;
            switch (type)
            {
                case PropModule.NomaiTextArcInfo.NomaiTextArcType.Child:
                    variation = arcInfo.variation < 0
                        ? Random.Range(0, _childArcPrefabs.Count())
                        : (arcInfo.variation % _childArcPrefabs.Count());
                    arc = _childArcPrefabs[variation].InstantiateInactive();
                    break;
                case PropModule.NomaiTextArcInfo.NomaiTextArcType.Stranger when _ghostArcPrefabs.Any():
                    variation = arcInfo.variation < 0
                        ? Random.Range(0, _ghostArcPrefabs.Count())
                        : (arcInfo.variation % _ghostArcPrefabs.Count());
                    arc = _ghostArcPrefabs[variation].InstantiateInactive();
                    break;
                case PropModule.NomaiTextArcInfo.NomaiTextArcType.Adult:
                default:
                    variation = arcInfo.variation < 0
                        ? Random.Range(0, _arcPrefabs.Count())
                        : (arcInfo.variation % _arcPrefabs.Count());
                    arc = _arcPrefabs[variation].InstantiateInactive();
                    break;
            }
            arcInfo.variation = variation;

            arc.transform.parent = conversationZone.transform;
            arc.GetComponent<NomaiTextLine>()._prebuilt = false;

            if (arcInfo != null)
            {
                var a = arcInfo;
                if (a.position == null) arc.transform.localPosition = Vector3.zero;
                else arc.transform.localPosition = new Vector3(a.position.x, a.position.y, 0);

                arc.transform.localRotation = Quaternion.Euler(0, 0, a.zRotation);

                if (a.mirror) arc.transform.localScale = new Vector3(-1, 1, 1);
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
    
            arcInfoToCorrespondingSpawnedGameObject[arcInfo] = arc;

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
        // Construct spiral meshes from the mathematical spirals generated below
        //
        //

        public class SpiralMesh : Spiral
        {
            public new List<SpiralMesh> children;

            public List<Vector3> skeleton;

            public int numSkeletonPoints = 51; // seems to be Mobius' default

            public float innerWidth = 0.001f; // width at the tip
            public float outerWidth = 0.05f;//0.107f; // width at the base
            public float uvScale = 4.9f; //2.9f;
            private float baseUVScale = 1f/300f;
            public float uvOffset = 0;

            public Mesh mesh;

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
                        /*  
                           2       3
                            *-----*                   
                            |⟍    |                   
                            |  ⟍  |                   
                            |    ⟍|                   
                            *-----*         
                           0       1
                         */
                        triangles.Add(i+2);
                        triangles.Add(i+1);
                        triangles.Add(i);
            
                        triangles.Add(i+2);
                        triangles.Add(i+3);
                        triangles.Add(i+1);
                    }


                    //var startT = spiralStartT(startIndex, a, b); // let's try switching this up. define a set starting point T using the Desmos graph, and make it the larger value so that the skeleton generates in the right direction
                    //var startS = tToArcLen(startT, a, b);
                    //var endS = startS + len; // remember the spiral is defined backwards, so the start is the inner part of the spiral
                    var startT = tFromArcLen(startS, a, b);
                    var endT = tFromArcLen(endS, a, b);

                    Logger.Log($"START AND END S: {startS}   {endS}"); // 42.87957 342.8796 

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
                        float inputS = tToArcLen(inputT, a, b);
                        float sFraction = (inputS-startS)/rangeS;
                        float absoluteS = (inputS-startS);

                        float u = absoluteS * uvScale * baseUVScale + uvOffset;
                        uvs[i*2]   = new Vector2(u, 0);
                        uvs[i*2+1] = new Vector2(u, 1);

                        uv2s[i*2]   = new Vector2(1-sFraction, 0);
                        uv2s[i*2+1] = new Vector2(1-sFraction, 1);
                    }

                    //Vector2[] uvs = new Vector2[newVerts.Length];
                    //Vector2[] uv2s = new Vector2[newVerts.Length];
                    //for (int i = 0; i < newVerts.Length; i+= 2)
                    //{
                    //    float frac = 1f-((float)i) / ((float)newVerts.Length);
                    //    float u = uvScale * frac;
                    //    uvs[i]   = new Vector2(1-u, 0);
                    //    uvs[i+1] = new Vector2(1-u, 1);

                    //    uv2s[i]   = new Vector2(frac, 0);
                    //    uv2s[i+1] = new Vector2(frac, 1);
                    //}
                    
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
            
            internal void updateChild(SpiralMesh child)
            {
                Vector3 pointAndNormal = getDrawnSpiralPointAndNormal(child.startSOnParent, this.x, this.y, this.ang, this.mirror, this.scale, this.a, this.b); //, this.len); // len is not needed - this function pretends the spiral is infinite
                var cx = pointAndNormal.x;
                var cy = pointAndNormal.y;
                var cang = pointAndNormal.z;
                child.x = cx;
                child.y = cy;
                child.ang = cang+(child.mirror?Mathf.PI:0);
            
                child.updateMesh();
            }

            public void addChild(SpiralMesh child)
            {
                updateChild(child);
                this.children.Add(child);
            }

            
            public virtual void updateChildren() 
            {
                this.updateMesh();
                base.updateChildren();
            }
        }


        //
        //
        // Construct the mathematical spirals that Nomai arcs are built from
        //
        //

        // TODO: replace the len param with a start and end param
        
        public class Spiral {
            public bool mirror;
            public float a;
            public float b;
            public float len;
            public float startSOnParent;
            public float scale;
            public List<Spiral> children;
            
            public float x;
            public float y;
            public float ang;


            public float startIndex = 2.5f;

            public float startS = 42.87957f; 
            public float endS = 342.8796f;


            // (float startSOnParent=0, bool mirror=false, float len=300, float a=0.7f, float b=0.305f, float scale=0.01f) 
            public Spiral(float startSOnParent=0, bool mirror=false, float len=300, float a=0.5f, float b=0.43f, float scale=0.01f) 
            {
                this.mirror = mirror;
                this.a = a;
                this.b = b;
                this.len = len;
                this.startSOnParent = startSOnParent;
                this.scale = scale;

                this.children = new List<Spiral>();

                //this.params = [mirror, scale, a, b, len]

                this.x = 0;
                this.y = 0;
                this.ang = 0;
            }

            internal virtual void updateChild(Spiral child)
            {
                Vector3 pointAndNormal = getDrawnSpiralPointAndNormal(child.startSOnParent, this.x, this.y, this.ang, this.mirror, this.scale, this.a, this.b); //, this.len); // len is not needed - this function pretends the spiral is infinite
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
                // TODO: wherever len is used, undo the change from endT-startT to startT-endT
                // and also make spiralStartT a param of the spiral

                //var startT = spiralStartT(startIndex, a, b); // let's try switching this up. define a set starting point T using the Desmos graph, and make it the larger value so that the skeleton generates in the right direction
                //var startS = tToArcLen(startT, a, b);
                //var endS = startS + len; // remember the spiral is defined backwards, so the start is the inner part of the spiral
                var endT = tFromArcLen(endS, a, b);
                var startT = tFromArcLen(startS, a, b);
                var rangeT = endT-startT;    
                
                Logger.Log($"STARTING PARAMS FOR SKELE: {startT}, {startS}, {endS}, {len}, {endT}, {rangeT}");

                List<Vector3> skeleton = new List<Vector3>();
                for (int i = 0; i < numPoints; i++)
                {
                    float fraction = ((float)i)/((float)numPoints); // casting is so uuuuuuuugly

                    // note: cutting the sprial into numPoints equal slices of arclen would
                    // provide evenly spaced skeleton points
                    // on the other hand, cutting the spiral into numPoints equal slices of t
                    // will cluster points in areas of higher detail. this is the way Mobius does it, so it is the way we also will do it
                    float inputT = startT + rangeT*fraction;
                    float inputS = tToArcLen(inputT, a, b);

                    Logger.Log($"GENREATING INPUT S: {fraction}, {inputT}, {inputS}");

                    skeleton.Add(getDrawnSpiralPointAndNormal(inputS, x, y, ang, mirror, scale, a, b));
                    
                    Logger.Log(fraction + ": " + skeleton[skeleton.Count()-1]);
                }

                skeleton.Reverse();
                return skeleton;
            }

            //draw() {
            //    drawSpiral(drawMsg, this.x, this.y, this.ang, ...this.params)

            //    this.children.forEach(child => child.draw())
            //}
        }

        

        // all of this math is based off of this:
        // https://www.desmos.com/calculator/9gdfgyuzf6

        // note: t refers to theta, and s refers to arc length
        //


        private static float lerp(float a, float b, float t) {
            return a*t + b*(1-t);
        }
        
        private static float cos(float t) { return Mathf.Cos(t); }
        private static float sin(float t) { return Mathf.Sin(t); }
        private static float exp(float t) { return Mathf.Exp(t); }
        private static float sqrt(float t) { return Mathf.Sqrt(t); }
        private static float ln(float t) { return Mathf.Log(t); }


        // get the (x, y) coordinates and the normal angle at the given location (measured in arcLen) of a spiral with the given parameters 
        // note: arcLen is inverted so that 0 refers to what we consider the start of the Nomai spiral
        private static Vector3 getDrawnSpiralPointAndNormal(float arcLen, float offsetX=0, float offsetY=0, float offsetAngle=0, bool mirror=false, float scale=0.01f, float a=0.7f, float b=0.305f) {
        
            //Logger.Log($"params: {arcLen}, {a}, {b}, {mirror}, {offsetX}, {offsetY}, {offsetAngle}, {scale}");

            var startIndex = 2.5f;

            var startT = spiralStartT(startIndex, a, b);
            var startS = tToArcLen(startT, a, b);

            var width = spiralBoundingBoxWidth(startIndex, a, b);

            var startPoint = spiralPoint(startT, a, b);
            var startX = startPoint.x;
            var startY = startPoint.y;

            var t = tFromArcLen(arcLen, a, b);
            var point = spiralPoint(t, a, b);
            var x = point.x; 
            var y = point.y;
            var ang = normalAngle(t, a, b);

            Logger.Log($"T AND Ses HREEEEERE: {startS} => {startT} ;;; {startS} - {arcLen} = {startS-arcLen} => {t}");

            Logger.Log("start point: " + startPoint + "  point: " + point);

            if (mirror) { 
                // x = -(x-startX-width/2) +width/2+startX;
                x = x + 2*(startX-x);

                ang = -ang+Mathf.PI;
            } 

            var retX = 0f;
            var retY = 0f;

    
    
            retX += scale*(x-startX);
            retY += scale*(y-startY);

            // rotate offsetAngle rads 
            var retX2=retX*cos(offsetAngle)
                     -retY*sin(offsetAngle);
            var retY2=retX*sin(offsetAngle)        
                     +retY*cos(offsetAngle);

            retX = retX2;
            retY = retY2;

            retX += offsetX;
            retY += offsetY;

            //Logger.Log("returning point: " + new Vector3(retX, retY, ang+offsetAngle+Mathf.PI/2f));

            return new Vector3(retX, retY, ang+offsetAngle+Mathf.PI/2f);
        } 

        private static Vector2 spiralPoint(float t, float a, float b) {
            var r = a*exp(b*t);
            var retval = new Vector2(r*cos(t), r*sin(t));
            //Logger.Log($"Point for {t}, {a}, {b}: " + retval.x + ", " + retval.y);
            return retval;
        }

        // the spiral's got two functions: x(t) and y(t)
        // so it's got two derrivatives (with respect to t) x'(t) and y'(t)
        private static Vector2 spiralDerivative(float t, float a, float b) { // derrivative with respect to t
            var r = a*exp(b*t);
            return new Vector2(
                -r*(sin(t)-b*cos(t)),
                 r*(b*sin(t)+cos(t))
            );
        }

        // returns the length of the spiral between t0 and t1
        private static float spiralArcLength(float t0, float t1, float a, float b) {
            return (a/b)*sqrt(b*b+1)*(exp(b*t1)-exp(b*t0));
        }

        // converts from a value of t to the equivalent value of s (the value of s that corresponds to the same point on the spiral as t)
        private static float tToArcLen(float t, float a, float b) {
            return spiralArcLength(0, t, a, b);
        }

        // reverse of above
        private static float tFromArcLen(float s, float a, float b) {
            return ln(
                    1+s/(
                        (a/b)*
                        sqrt(b*b+1)
                    )
                )/b;
        }

        // returns the value of t where the spiral starts
        // nomai spirals are reversed from the way the math is defined. in the math, they start at the center and spiral out, whereas Nomai writing spirals in
        // so this really returns the largest allowed value of t for this spiral
        // note: n is just an index. what it's an index of is irrelevant, but 2.5 is a good value
        private static float spiralStartT(float n, float a, float b) {
            return Mathf.Atan(b)+Mathf.PI*n;
        }

        // returns the angle of the spiral's normal at a given point
        private static float normalAngle(float t, float a, float b) {
            var d = spiralDerivative(t, a, b);
            var n = new Vector2(d.y, -d.x);
            var angle = Mathf.Atan2(n.y, n.x);

            return angle-Mathf.PI/2;
        }

        // startN refers to the same n as spiralStartT
        private static float spiralBoundingBoxWidth(float startN, float a, float b) {
            var topT = Mathf.Atan(-1/b)+Mathf.PI*startN;
            var startT = spiralStartT(startN, a, b);

            var topX = spiralPoint(topT, a, b).x;
            var startX = spiralPoint(startT, a, b).x;

            return Mathf.Abs(topX-startX);
        }
    }
}
