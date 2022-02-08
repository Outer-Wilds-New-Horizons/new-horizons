using NewHorizons.Components;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using UnityEngine.UI;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.Builder.Handlers;

namespace NewHorizons.Builder.ShipLog
{
    public static class RumorModeBuilder
    {
        private static readonly Dictionary<CuriosityName, Color> _curiosityColors = new Dictionary<CuriosityName, Color>();
        private static readonly Dictionary<CuriosityName, Color> _curiosityHighlightColors = new Dictionary<CuriosityName, Color>();
        private static readonly Dictionary<string, CuriosityName> _rawNameToCuriosityName = new Dictionary<string, CuriosityName>();
        private static readonly Dictionary<string, string> _entryIdToRawName = new Dictionary<string, string>();

        public static void AddCuriosityColors(ShipLogModule.CuriosityColorInfo[] newColors)
        {
            foreach (ShipLogModule.CuriosityColorInfo newColor in newColors)
            {
                if (_rawNameToCuriosityName.ContainsKey(newColor.id) == false)
                {
                    CuriosityName newName = (CuriosityName)8 + _rawNameToCuriosityName.Count;
                    _rawNameToCuriosityName.Add(newColor.id, newName);
                    _curiosityColors.Add(newName, newColor.color.ToColor());
                    _curiosityHighlightColors.Add(newName, newColor.highlightColor.ToColor());
                }
            }
        }

        public static Color GetCuriosityColor(CuriosityName curiosityName, bool highlighted, Color defaultColor, Color defaultHighlight)
        {
            if (_curiosityColors.ContainsKey(curiosityName) && _curiosityHighlightColors.ContainsKey(curiosityName))
            {
                return (highlighted ? _curiosityHighlightColors : _curiosityColors)[curiosityName];
            }
            else
            {
                return highlighted ? defaultHighlight : defaultColor;
            }
        }

        public static void AddBodyToShipLog(ShipLogManager manager, NewHorizonsBody body)
        {
            string systemName = body.Config.StarSystem;
            XElement astroBodyFile = XElement.Load(body.Mod.Manifest.ModFolderPath + "/" + body.Config.ShipLog.xmlFile);
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
                    if (curiosityName != null && id != null && _entryIdToRawName.ContainsKey(id.Value) == false)
                    {
                        _entryIdToRawName.Add(id.Value, curiosityName.Value);
                    }
                    foreach (XElement childEntryElement in entryElement.Elements("Entry"))
                    {
                        XElement childCuriosityName = childEntryElement.Element("Curiosity");
                        XElement childId = childEntryElement.Element("ID");
                        if (childId != null && _entryIdToRawName.ContainsKey(childId.Value))
                        {
                            if (childCuriosityName == null && curiosityName != null)
                            {
                                _entryIdToRawName.Add(childId.Value, curiosityName.Value);
                            }
                            else if (childCuriosityName != null)
                            {
                                _entryIdToRawName.Add(childId.Value, childCuriosityName.Value);
                            }
                        }
                        AddTranslation(childEntryElement);
                    }
                    AddTranslation(entryElement);
                }
                TextAsset newAsset = new TextAsset(astroBodyFile.ToString());
                List<TextAsset> newBodies = new List<TextAsset>(manager._shipLogXmlAssets) { newAsset };
                manager._shipLogXmlAssets = newBodies.ToArray();
                ShipLogHandler.AddConfig(astroBodyId.Value, body);
            }
        }

        public static void GenerateEntryData(ShipLogManager manager)
        {
            const int step = 400;
            int colAccumulator = 0;
            int rowAccumulator = 0;
            foreach (ShipLogEntry entry in manager._entryList)
            {
                if (manager._entryDataDict.ContainsKey(entry._id) == false)
                {
                    NewHorizonsBody body = ShipLogHandler.GetConfigFromID(entry._astroObjectID);
                    Vector2? manualEntryPosition = GetManualEntryPosition(entry._id, body.Config.ShipLog);
                    Vector2 entryPosition;
                    if (manualEntryPosition == null)
                    {
                        entryPosition = new Vector2(colAccumulator, rowAccumulator);
                    }
                    else
                    {
                        entryPosition = (Vector2)manualEntryPosition;
                    }
                    EntryData newData = new EntryData
                    {
                        id = entry._id,
                        cardPosition = entryPosition,
                        sprite = body.Config.ShipLog.spriteFolder == null ? null : GetEntrySprite(entry._id, body)
                    };
                    entry.SetSprite(newData.sprite == null ? manager._shipLogLibrary.defaultEntrySprite : newData.sprite);
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
            if (_entryIdToRawName.ContainsKey(entry._id))
            {
                entry._curiosity = _rawNameToCuriosityName[_entryIdToRawName[entry._id]];
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
            foreach (ShipLogModule.EntryPositionInfo position in config.positions)
            {
                if (position.id == entryId)
                {
                    return position.position;
                }
            }
            return null;
        }
    }
}
