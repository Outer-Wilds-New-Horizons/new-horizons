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
        private static Dictionary<CuriosityName, Color> curiosityColors = new Dictionary<CuriosityName, Color>();
        private static Dictionary<CuriosityName, Color> curiosityHighlightColors = new Dictionary<CuriosityName, Color>();
        private static Dictionary<string, CuriosityName> rawNameToCuriosityName = new Dictionary<string, CuriosityName>();
        private static Dictionary<string, string> entryIdToRawName = new Dictionary<string, string>();
        private static Dictionary<string, NewHorizonsBody> astroIdToBody = new Dictionary<string, NewHorizonsBody>();

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
            return astroIdToBody[id].Config.Name;
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
        
        public static T KeyByValue<T, W>(this Dictionary<T, W> dict, W val)
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
            return KeyByValue(astroIdToBody, body);
        }

        private static void CreateAstroObject(GameObject nodeGO, ref MapModeObject node, GameObject referenceUnviewedSprite, int layer)
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

        private static void CreateNode(ref MapModeObject node, GameObject parent, int layer)
        {
            const float padding = 250f;

            GameObject newNodeGO = new GameObject(node.mainBody.Config.Name + "_ShipLog");
            newNodeGO.layer = layer;
            newNodeGO.transform.SetParent(parent.transform);

            RectTransform transform = newNodeGO.AddComponent<RectTransform>();
            float scale = node.mainBody.Config.ShipLog?.mapMode?.scale?? 1f;
            scale = scale <= 0 ? 1f : scale;
            transform.localPosition = new Vector3(node.x * padding, node.y * padding, 0);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one * scale;
            
            if (node.mainBody.Config.ShipLog?.xmlFile == null)
            {
                Image newImage = newNodeGO.AddComponent<Image>();
                string imagePath = node.mainBody.Config.ShipLog?.mapMode?.revealedSprite ?? "DEFAULT";
                if (imagePath == "DEFAULT")
                {
                    newImage.sprite = Locator.GetShipLogManager()._shipLogLibrary.defaultEntrySprite;
                }
                else
                {
                    Texture2D newTexture = node.mainBody.Mod.Assets.GetTexture(imagePath);
                    Rect rect = new Rect(0, 0, newTexture.width, newTexture.height);
                    Vector2 pivot = new Vector2(newTexture.width / 2, newTexture.height / 2);
                    newImage.sprite = Sprite.Create(newTexture, rect, pivot);
                }
            }
            else
            {
                CreateAstroObject(newNodeGO, ref node, GameObject.Find(PAN_ROOT_PATH + "/TimberHearth/UnviewedIcon"), layer);
            }
        }

        private static MapModeObject MakePrimaryNode(string systemName)
        {
            foreach (NewHorizonsBody body in Main.BodyDict[systemName])
            {
                if (!body.Config.Base.CenterOfSolarSystem) continue;
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
            foreach (NewHorizonsBody body in Main.BodyDict[systemName])
            {
                if (body.Config.Orbit.PrimaryBody == parent.mainBody.Config.Name)
                {
                    int newLevel = parent.level + 1;
                    bool even = newLevel % 2 == 0;
                    newX = even ? newX : newX + 1;
                    newY = even ? newY + 1 : newY;
                    MapModeObject newNode = new MapModeObject()
                    {
                        mainBody = body,
                        level = newLevel,
                        x = newX,
                        y = newY,
                        parent=parent
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
                    children.Add(newNode);
                }
            }
            return children;
        }

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

        public static void AddAstroBodyToShipLog(ShipLogManager manager, NewHorizonsBody body)
        {
            string systemName = body.Config.StarSystem;
            XElement astroBodyFile = XElement.Load(Main.Instance.ModHelper.Manifest.ModFolderPath + body.Config.ShipLog.xmlFile);
            XElement astroBodyId = astroBodyFile.Element("ID");
            if (astroBodyId == null)
            {
                Logger.LogError("Failed to load ship log for " + systemName + "!");
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

        private static Sprite GetSprite(string entryId, NewHorizonsBody body)
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
                return Locator.GetShipLogManager()._shipLogLibrary.defaultEntrySprite;
            }
        }

        private static NewHorizonsBody GetConfigFromEntry(ShipLogEntry entry)
        {
            return astroIdToBody[entry._astroObjectID];
        }

        private static Vector2? FindPosition(string entryId, ShipLogModule config)
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
                    Vector2? manualEntryPosition = FindPosition(entry._id, body.Config.ShipLog);
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
                        sprite = body.Config.ShipLog.spriteFolder == null? null : GetSprite(entry._id, body)
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
