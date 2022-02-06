using NewHorizons.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using UnityEngine.UI;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    public static class ShipLogBuilder
    {
        public static readonly string PAN_ROOT_PATH = "Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/MapMode/ScaleRoot/PanRoot";
        
        public static ShipLogStarChartMode ShipLogStarChartMode;
        private static Dictionary<string, NewHorizonsBody> astroIdToBody = new Dictionary<string, NewHorizonsBody>();
        
        private static NewHorizonsBody GetConfigFromEntry(ShipLogEntry entry)
        {
            return astroIdToBody[entry._astroObjectID];
        }

        #region Map Mode

        public class MapModeBuilder
        {
            private class MapModeObject
            {
                public int x;
                public int y;
                public int branch_width;
                public int branch_height;
                public int level;
                public NewHorizonsBody mainBody;
                public ShipLogAstroObject astroObject;
                public List<MapModeObject> children;
                public MapModeObject parent;
                public MapModeObject lastSibling;
                public void increment_width()
                {
                    branch_width++;
                    parent?.increment_width();
                }
                public void increment_height()
                {
                    branch_height++;
                    parent?.increment_height();
                }
            }

            public static string GetAstroBodyShipLogName(string id)
            {
                if (astroIdToBody.ContainsKey(id))
                {
                    return astroIdToBody[id].Config.Name;
                }
                else
                {
                    return id;
                }
            }

            public static ShipLogAstroObject[][] ConstructMapMode(string systemName, GameObject transformParent, int layer)
            {
                MapModeObject rootObject = MakePrimaryNode(systemName);
                if (rootObject.mainBody != null)
                {
                    CreateAllNodes(ref rootObject, transformParent, layer);
                }

                const int maxAmount = 20;
                ShipLogAstroObject[][] navMatrix = new ShipLogAstroObject[maxAmount][];
                for (int i = 0; i < maxAmount; i++)
                {
                    navMatrix[i] = new ShipLogAstroObject[maxAmount];
                }

                CreateNavigationMatrix(rootObject, ref navMatrix);
                navMatrix = navMatrix.Where(a => a.Count(c => c != null) > 0).Prepend(new ShipLogAstroObject[1]).ToArray();
                for (var index = 0; index < navMatrix.Length; index++)
                {
                    navMatrix[index] = navMatrix[index].Where(a => a != null).ToArray();
                }
                return navMatrix;
            }

            private static void CreateNavigationMatrix(MapModeObject root, ref ShipLogAstroObject[][] navMatrix)
            {
                if (root.astroObject != null)
                {
                    navMatrix[root.y][root.x] = root.astroObject;
                }
                foreach (MapModeObject child in root.children)
                {
                    CreateNavigationMatrix(child, ref navMatrix);
                }
            }

            private static void CreateAllNodes(ref MapModeObject parentNode, GameObject parent, int layer)
            {
                CreateNode(ref parentNode, parent, layer);
                for (var i = 0; i < parentNode.children.Count; i++)
                {
                    MapModeObject child = parentNode.children[i];
                    CreateAllNodes(ref child, parent, layer);
                    parentNode.children[i] = child;
                }
            }

            private static GameObject CreateImage(GameObject nodeGO, IModAssets assets, string imagePath, string name, int layer)
            {
                GameObject newImageGO = new GameObject(name);
                newImageGO.layer = layer;
                newImageGO.transform.SetParent(nodeGO.transform);

                RectTransform transform = newImageGO.AddComponent<RectTransform>();
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            
                Image newImage = newImageGO.AddComponent<Image>();
                if (imagePath == "DEFAULT")
                {
                    newImage.sprite = Locator.GetShipLogManager()._shipLogLibrary.defaultEntrySprite;
                }
                else
                {
                    Texture2D newTexture = assets.GetTexture(imagePath);
                    Rect rect = new Rect(0, 0, newTexture.width, newTexture.height);
                    Vector2 pivot = new Vector2(newTexture.width / 2, newTexture.height / 2);
                    newImage.sprite = Sprite.Create(newTexture, rect, pivot);
                }
                return newImageGO;
            }
            
            public static T KeyByValue<T, W>(Dictionary<T, W> dict, W val)
            {
                T key = default;
                foreach (KeyValuePair<T, W> pair in dict)
                {
                    if (EqualityComparer<W>.Default.Equals(pair.Value, val))
                    {
                        key = pair.Key;
                        break;
                    }
                }
                return key;
            }

            private static string GetAstroObjectId(NewHorizonsBody body)
            {
                if (astroIdToBody.ContainsValue(body))
                {
                    return KeyByValue(astroIdToBody, body);
                }
                else
                {
                    return body.Config.Name;
                }
            }

            private static void CreateShipLogAstroObject(GameObject nodeGO, ref MapModeObject node, GameObject referenceUnviewedSprite, int layer)
            {
                const float unviewedIconOffset = 15;
                ShipLogAstroObject astroObject = nodeGO.AddComponent<ShipLogAstroObject>();
                astroObject._id = GetAstroObjectId(node.mainBody);
                string imagePath = node.mainBody.Config.ShipLog?.mapMode?.revealedSprite ?? "DEFAULT";
                string outlinePath = node.mainBody.Config.ShipLog?.mapMode?.outlineSprite ?? imagePath;
                astroObject._imageObj = CreateImage(nodeGO, node.mainBody.Mod.Assets, imagePath, "Image", layer);
                astroObject._outlineObj = CreateImage(nodeGO, node.mainBody.Mod.Assets, outlinePath, "Outline", layer);
                astroObject._unviewedObj = GameObject.Instantiate(referenceUnviewedSprite, nodeGO.transform, false);
                astroObject._invisibleWhenHidden = node.mainBody.Config.ShipLog?.mapMode?.invisibleWhenHidden ?? false;
                Rect imageRect = astroObject._imageObj.GetComponent<RectTransform>().rect;
                astroObject._unviewedObj.transform.localPosition = new Vector3(imageRect.width / 2 + unviewedIconOffset, imageRect.height / 2 + unviewedIconOffset, 0);
                node.astroObject = astroObject;
            }

            private static void ConnectNodes(MapModeObject from, MapModeObject to)
            {
                
            }

            private static void CreateNode(ref MapModeObject node, GameObject parent, int layer)
            {
                const float padding = 250f;

                GameObject newNodeGO = new GameObject(node.mainBody.Config.Name + "_ShipLog");
                newNodeGO.layer = layer;
                newNodeGO.transform.SetParent(parent.transform);

                RectTransform transform = newNodeGO.AddComponent<RectTransform>();
                float scale = node.mainBody.Config.ShipLog?.mapMode?.scale?? 1f;
                Vector2 position = Vector2.zero;
                if (node.lastSibling != null)
                {
                    ShipLogAstroObject lastAstroObject = node.lastSibling.astroObject;
                    Vector3 lastPosition = lastAstroObject.transform.localPosition;
                    position = lastPosition;
                    float extraDistance = (node.mainBody.Config.ShipLog?.mapMode?.offset ?? 0f) * 100;
                    if (node.level % 2 == 0)
                    {
                        position.y += padding * (node.y - node.lastSibling.y) + extraDistance;
                    }
                    else
                    {
                        position.x += padding * (node.x - node.lastSibling.x) + extraDistance;
                    }
                }
                transform.localPosition = new Vector3(position.x, position.y, 0);
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one * scale;
                CreateShipLogAstroObject(newNodeGO, ref node, GameObject.Find(PAN_ROOT_PATH + "/TimberHearth/UnviewedIcon"), layer);
            }

            private static MapModeObject MakePrimaryNode(string systemName)
            {
                foreach (NewHorizonsBody body in Main.BodyDict[systemName].Where(b => b.Config.Base.CenterOfSolarSystem))
                {
                    MapModeObject newNode = new MapModeObject
                    {
                        mainBody = body,
                        level = 0,
                        x = 0,
                        y = 0
                    };
                    newNode.children = MakeChildrenNodes(systemName, newNode);
                    return newNode;
                }
                Logger.LogError("Couldn't find center of system!");
                return new MapModeObject();
            }

            private static List<MapModeObject> MakeChildrenNodes(string systemName, MapModeObject parent)
            {
                List<MapModeObject> children = new List<MapModeObject>();
                int newX = parent.x;
                int newY = parent.y;
                int newLevel = parent.level + 1;
                MapModeObject lastSibling = parent;
                foreach (NewHorizonsBody body in Main.BodyDict[systemName].Where(b => b.Config.ShipLog?.mapMode?.remove == false && b.Config.Orbit.PrimaryBody == parent.mainBody.Config.Name))
                {
                    if (body.Config.Orbit.PrimaryBody == parent.mainBody.Config.Name)
                    {
                        bool even = newLevel % 2 == 0;
                        newX = even ? newX : newX + 1;
                        newY = even ? newY + 1 : newY;
                        MapModeObject newNode = new MapModeObject()
                        {
                            mainBody = body,
                            level = newLevel,
                            x = newX,
                            y = newY,
                            parent = parent,
                            lastSibling = lastSibling
                        };
                        newNode.children = MakeChildrenNodes(systemName, newNode);
                        if (even)
                        {
                            newY += newNode.branch_height;
                            parent.increment_height();
                        }
                        else
                        {
                            newX += newNode.branch_width;
                            parent.increment_width();
                        }
                        lastSibling = newNode;
                        children.Add(newNode);
                    }
                }
                return children;
            }
        }
        #endregion

        #region Rumor Mode
        public static class RumorModeBuilder
        {
            private static Dictionary<CuriosityName, Color> curiosityColors = new Dictionary<CuriosityName, Color>();
            private static Dictionary<CuriosityName, Color> curiosityHighlightColors = new Dictionary<CuriosityName, Color>();
            private static Dictionary<string, CuriosityName> rawNameToCuriosityName = new Dictionary<string, CuriosityName>();
            private static Dictionary<string, string> entryIdToRawName = new Dictionary<string, string>();

            public static void AddCuriosityColors(ShipLogModule.CuriosityColor[] newColors)
            {
                foreach (ShipLogModule.CuriosityColor newColor in newColors)
                {
                    if (rawNameToCuriosityName.ContainsKey(newColor.id) == false)
                    {
                        CuriosityName newName = (CuriosityName) 8 + rawNameToCuriosityName.Count;
                        rawNameToCuriosityName.Add(newColor.id, newName);
                        curiosityColors.Add(newName, newColor.color.ToColor());
                        curiosityHighlightColors.Add(newName, newColor.highlightColor.ToColor());
                    }
                }
            }
            
            public static Color GetCuriosityColor(CuriosityName curiosityName, bool highlighted, Color defaultColor, Color defaultHighlight)
            {
                if (curiosityColors.ContainsKey(curiosityName) && curiosityHighlightColors.ContainsKey(curiosityName))
                {
                    return (highlighted ? curiosityHighlightColors : curiosityColors)[curiosityName];
                }
                else
                {
                    return highlighted? defaultHighlight : defaultColor;
                }
            }
            
            public static void AddBodyToShipLog(ShipLogManager manager, NewHorizonsBody body)
            {
                string systemName = body.Config.StarSystem;
                XElement astroBodyFile = XElement.Load(Main.Instance.ModHelper.Manifest.ModFolderPath + body.Config.ShipLog.xmlFile);
                XElement astroBodyId = astroBodyFile.Element("ID");
                if (astroBodyId == null)
                {
                    Logger.LogError("Failed to load ship logs for " + systemName + "!");
                }
                else
                {
                    astroBodyId.SetValue(systemName + "/" + astroBodyId.Value);
                    foreach (XElement entryElement in astroBodyFile.DescendantsAndSelf("Entry"))
                    {
                        XElement curiosityName = entryElement.Element("Curiosity");
                        XElement id = entryElement.Element("ID");
                        if (curiosityName != null && id != null && entryIdToRawName.ContainsKey(id.Value) == false)
                        {
                            entryIdToRawName.Add(id.Value, curiosityName.Value);
                        }
                        AddTranslation(entryElement);
                    }
                    TextAsset newAsset = new TextAsset(astroBodyFile.ToString());
                    List<TextAsset> newBodies = new List<TextAsset>(manager._shipLogXmlAssets) {newAsset};
                    manager._shipLogXmlAssets = newBodies.ToArray();
                    if (astroIdToBody.ContainsKey(astroBodyId.Value) == false)
                    {
                        astroIdToBody.Add(astroBodyId.Value, body);
                    }
                }
            }

            public static void GenerateEntryData(ShipLogManager manager)
            {
                const int step = 400;
                int colAccumulator = 0;
                int rowAccumulator = 0;
                foreach(ShipLogEntry entry in manager._entryList)
                {
                    if (manager._entryDataDict.ContainsKey(entry._id) == false)
                    {
                        NewHorizonsBody body = GetConfigFromEntry(entry);
                        Vector2? manualEntryPosition = GetManualEntryPosition(entry._id, body.Config.ShipLog);
                        Vector2 entryPosition;
                        if (manualEntryPosition == null)
                        {
                            entryPosition = new Vector2(colAccumulator, rowAccumulator);
                        }
                        else
                        {
                            entryPosition = (Vector2) manualEntryPosition;
                        }
                        EntryData newData = new EntryData
                        {
                            id = entry._id,
                            cardPosition = entryPosition,
                            sprite = body.Config.ShipLog.spriteFolder == null? null : GetEntrySprite(entry._id, body)
                        };
                        entry.SetSprite(newData.sprite == null? manager._shipLogLibrary.defaultEntrySprite : newData.sprite);
                        manager._entryDataDict.Add(entry._id, newData);
                        int index = manager._entryList.IndexOf(entry);
                        if (index < manager._entryList.Count - 2 && manager._entryList[index + 1]._astroObjectID != entry._astroObjectID)
                        {
                            rowAccumulator += step;
                            colAccumulator = 0;
                        }
                        else
                        {
                            colAccumulator += step;
                        }
                    }
                }
            }

            private static void AddTranslation(XElement entry)
            {
                Dictionary<string, string> table = TextTranslation.Get().m_table.theShipLogTable;
                XElement nameElement = entry.Element("Name");
                if (nameElement != null)
                {
                    string name = nameElement.Value;
                    table[name] = name;
                    foreach (XElement rumorFact in entry.Elements("RumorFact"))
                    {
                        XElement rumorName = rumorFact.Element("RumorName");
                        if (rumorName != null)
                        {
                            table[rumorName.Value] = rumorName.Value;
                        }

                        XElement rumorText = rumorFact.Element("Text");
                        if (rumorText != null)
                        {
                            table[name + rumorText.Value] = rumorText.Value;
                        }
                    }
                    foreach (XElement exploreFact in entry.Elements("ExploreFact"))
                    {
                        XElement exploreText = exploreFact.Element("Text");
                        if (exploreText != null)
                        {
                            table[name + exploreText.Value] = exploreText.Value;
                        }
                    }
                }
            }
            
            public static void UpdateEntryCuriosity(ref ShipLogEntry entry)
            {
                if (entryIdToRawName.ContainsKey(entry._id))
                {
                    entry._curiosity = rawNameToCuriosityName[entryIdToRawName[entry._id]];
                }
            }
            
            private static Sprite GetEntrySprite(string entryId, NewHorizonsBody body)
            {
                IModAssets assets = body.Mod.Assets;
                string path = body.Config.ShipLog.spriteFolder + "/" + entryId + ".png";
                if (File.Exists(Main.Instance.ModHelper.Manifest.ModFolderPath + path))
                {
                    Texture2D newTexture = assets.GetTexture(path);
                    Rect rect = new Rect(0, 0, newTexture.width, newTexture.height);
                    Vector2 pivot = new Vector2(newTexture.width / 2, newTexture.height / 2);
                    return Sprite.Create(newTexture, rect, pivot);
                }
                else
                {
                    return null;
                }
            }
            
            private static Vector2? GetManualEntryPosition(string entryId, ShipLogModule config)
            {
                if (config.positions == null) return null;
                foreach (ShipLogModule.EntryPosition position in config.positions)
                {
                    if (position.id == entryId)
                    {
                        return position.position;
                    }
                }
                return null;
            }
        }
        #endregion
        
        #region Fact Reveals
        public static class RevealBuilder
        {
            public static void Make(GameObject go, Sector sector, PropModule.RevealInfo info, IModHelper mod)
            {
                GameObject newRevealGO = MakeGameObject(go, sector, info, mod);
                switch (info.revealOn.ToLower())
                {
                    case "enter":
                        MakeTrigger(newRevealGO, sector, info, mod);
                        break;
                    case "observe":
                        MakeObservable(newRevealGO, sector, info, mod);
                        break;
                    case "snapshot":
                        MakeSnapshot(newRevealGO, sector, info, mod);
                        break;
                    default:
                        Logger.LogError("Invalid revealOn: " + info.revealOn);
                        break;
                }

                newRevealGO.SetActive(true);
            }

            private static SphereShape MakeShape(GameObject go, PropModule.RevealInfo info, Shape.CollisionMode collisionMode)
            {
                SphereShape newShape = go.AddComponent<SphereShape>();
                newShape.radius = info.radius;
                newShape.SetCollisionMode(collisionMode);
                return newShape;
            }

            private static GameObject MakeGameObject(GameObject go, Sector sector, PropModule.RevealInfo info, IModHelper mod)
            {
                GameObject revealTriggerVolume = new GameObject("Reveal Volume (" + info.revealOn + ")");
                revealTriggerVolume.SetActive(false);
                revealTriggerVolume.transform.parent = sector?.transform ?? go.transform;
                revealTriggerVolume.transform.localPosition = info.position;
                return revealTriggerVolume;
            }

            private static void MakeTrigger(GameObject go, Sector sector, PropModule.RevealInfo info, IModHelper mod)
            {
                SphereShape newShape = MakeShape(go, info, Shape.CollisionMode.Volume);
                OWTriggerVolume newVolume = go.AddComponent<OWTriggerVolume>();
                newVolume._shape = newShape;
                ShipLogFactListTriggerVolume volume = go.AddComponent<ShipLogFactListTriggerVolume>();
                volume._factIDs = info.reveals;
            }

            private static void MakeObservable(GameObject go, Sector sector, PropModule.RevealInfo info, IModHelper mod)
            {
                go.layer = LayerMask.NameToLayer("Interactible");
                SphereCollider newSphere = go.AddComponent<SphereCollider>();
                newSphere.radius = info.radius;
                OWCollider newCollider = go.AddComponent<OWCollider>();
                ShipLogFactObserveTrigger newObserveTrigger = go.AddComponent<ShipLogFactObserveTrigger>();
                newObserveTrigger._factIDs = info.reveals;
                newObserveTrigger._maxViewDistance = info.maxDistance == -1f ? 2f : info.maxDistance;
                newObserveTrigger._maxViewAngle = info.maxAngle;
                newObserveTrigger._owCollider = newCollider;
                newObserveTrigger._disableColliderOnRevealFact = true;
            }

            private static void MakeSnapshot(GameObject go, Sector sector, PropModule.RevealInfo info, IModHelper mod)
            {
                SphereShape newShape = MakeShape(go, info, Shape.CollisionMode.Manual);
                ShapeVisibilityTracker newTracker = go.AddComponent<ShapeVisibilityTracker>();
                newTracker._shapes = new Shape[] {newShape};
                ShipLogFactSnapshotTrigger newSnapshotTrigger = go.AddComponent<ShipLogFactSnapshotTrigger>();
                newSnapshotTrigger._maxDistance = info.maxDistance == -1f ? 200f : info.maxDistance;
                newSnapshotTrigger._factIDs = info.reveals;
            }
        }
        #endregion

        #region Entry Locations
        public static class EntryLocationBuilder
        {
            private static List<ShipLogEntryLocation> locationsToInitialize = new List<ShipLogEntryLocation>();
            public static void Make(GameObject go, Sector sector, PropModule.EntryLocationInfo info, IModHelper mod)
            {
                GameObject entryLocationGameObject = new GameObject("Entry Location (" + info.id + ")");
                entryLocationGameObject.SetActive(false);
                entryLocationGameObject.transform.parent = sector?.transform ?? go.transform;
                entryLocationGameObject.transform.localPosition = info.position;
                ShipLogEntryLocation newLocation = entryLocationGameObject.AddComponent<ShipLogEntryLocation>();
                newLocation._entryID = info.id;
                newLocation._isWithinCloakField = info.cloaked;
                locationsToInitialize.Add(newLocation);
                entryLocationGameObject.SetActive(true);
            }

            public static void InitializeLocations()
            {
                locationsToInitialize.ForEach(l => l.InitEntry());
                locationsToInitialize.Clear();
            }
        }
        #endregion
        
        public static void Init()
        {
            var shipLogRoot = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas");
            
            var starChartLog = new GameObject("StarChartMode");
            starChartLog.SetActive(false);
            starChartLog.transform.parent = shipLogRoot.transform;
            starChartLog.transform.localScale = Vector3.one * 1f;
            starChartLog.transform.localPosition = Vector3.zero;
            starChartLog.transform.localRotation = Quaternion.Euler(0, 0, 0);
            
            ShipLogStarChartMode = starChartLog.AddComponent<ShipLogStarChartMode>();
            
            var reticleImage = GameObject.Instantiate(GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ReticleImage (1)/"), starChartLog.transform);
            
            var scaleRoot = new GameObject("ScaleRoot");
            scaleRoot.transform.parent = starChartLog.transform;
            scaleRoot.transform.localScale = Vector3.one;
            scaleRoot.transform.localPosition = Vector3.zero;
            scaleRoot.transform.localRotation = Quaternion.Euler(0, 0, 0);

            var panRoot = new GameObject("PanRoot");
            panRoot.transform.parent = scaleRoot.transform;
            panRoot.transform.localScale = Vector3.one;
            panRoot.transform.localPosition = Vector3.zero;
            panRoot.transform.localRotation = Quaternion.Euler(0,0,0);

            var centerPromptList = shipLogRoot.transform.Find("ScreenPromptListScaleRoot/ScreenPromptList_Center")?.GetComponent<ScreenPromptList>();
            var upperRightPromptList = shipLogRoot.transform.Find("ScreenPromptListScaleRoot/ScreenPromptList_UpperRight")?.GetComponent<ScreenPromptList>();
            var oneShotSource = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/OneShotAudio_ShipLog")?.GetComponent<OWAudioSource>();

            ShipLogStarChartMode.Initialize(
                centerPromptList,
                upperRightPromptList,
                oneShotSource);
        }
    }
}
