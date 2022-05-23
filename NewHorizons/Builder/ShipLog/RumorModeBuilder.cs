#region

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

#endregion

namespace NewHorizons.Builder.ShipLog
{
    public static class RumorModeBuilder
    {
        private static Dictionary<CuriosityName, Color> _curiosityColors;
        private static Dictionary<CuriosityName, Color> _curiosityHighlightColors;
        private static Dictionary<string, CuriosityName> _rawNameToCuriosityName;
        private static Dictionary<string, string> _entryIdToRawName;

        public static void Init()
        {
            _curiosityColors = new Dictionary<CuriosityName, Color>();
            _curiosityHighlightColors = new Dictionary<CuriosityName, Color>();
            _rawNameToCuriosityName = new Dictionary<string, CuriosityName>();
            _entryIdToRawName = new Dictionary<string, string>();
        }

        public static void AddCuriosityColors(ShipLogModule.CuriosityColorInfo[] newColors)
        {
            foreach (var newColor in newColors)
                if (_rawNameToCuriosityName.ContainsKey(newColor.id) == false)
                {
                    var newName = (CuriosityName) 8 + _rawNameToCuriosityName.Count;
                    _rawNameToCuriosityName.Add(newColor.id, newName);
                    _curiosityColors.Add(newName, newColor.color);
                    _curiosityHighlightColors.Add(newName, newColor.highlightColor);
                }
        }

        public static Color GetCuriosityColor(CuriosityName curiosityName, bool highlighted, Color defaultColor,
            Color defaultHighlight)
        {
            if (_curiosityColors.ContainsKey(curiosityName) && _curiosityHighlightColors.ContainsKey(curiosityName))
                return (highlighted ? _curiosityHighlightColors : _curiosityColors)[curiosityName];
            return highlighted ? defaultHighlight : defaultColor;
        }

        public static void AddBodyToShipLog(ShipLogManager manager, NewHorizonsBody body)
        {
            var systemName = body.Config.starSystem;
            var astroBodyFile =
                XElement.Load(body.Mod.ModHelper.Manifest.ModFolderPath + "/" + body.Config.ShipLog.xmlFile);
            var astroBodyId = astroBodyFile.Element("ID");
            if (astroBodyId == null)
            {
                Logger.LogError("Failed to load ship logs for " + systemName + "!");
            }
            else
            {
                var entryIDs = new List<string>();
                foreach (var entryElement in astroBodyFile.DescendantsAndSelf("Entry"))
                {
                    var curiosityName = entryElement.Element("Curiosity");
                    var id = entryElement.Element("ID");
                    if (id != null)
                    {
                        entryIDs.Add(id.Value);
                        if (curiosityName != null && _entryIdToRawName.ContainsKey(id.Value) == false)
                            _entryIdToRawName.Add(id.Value, curiosityName.Value);
                    }

                    foreach (var childEntryElement in entryElement.Elements("Entry"))
                    {
                        var childCuriosityName = childEntryElement.Element("Curiosity");
                        var childId = childEntryElement.Element("ID");
                        if (childId != null)
                        {
                            entryIDs.Add(childId.Value);
                            if (_entryIdToRawName.ContainsKey(childId.Value))
                            {
                                if (childCuriosityName == null && curiosityName != null)
                                    _entryIdToRawName.Add(childId.Value, curiosityName.Value);
                                else if (childCuriosityName != null)
                                    _entryIdToRawName.Add(childId.Value, childCuriosityName.Value);
                            }
                        }

                        AddTranslation(childEntryElement);
                    }

                    AddTranslation(entryElement);
                }

                var newAsset = new TextAsset(astroBodyFile.ToString());
                var newBodies = new List<TextAsset>(manager._shipLogXmlAssets) {newAsset};
                manager._shipLogXmlAssets = newBodies.ToArray();
                ShipLogHandler.AddConfig(astroBodyId.Value, entryIDs, body);
            }
        }

        public static void GenerateEntryData(ShipLogManager manager)
        {
            const int step = 400;
            var colAccumulator = 0;
            var rowAccumulator = 0;
            foreach (var entry in manager._entryList)
                if (manager._entryDataDict.ContainsKey(entry._id) == false)
                {
                    var body = ShipLogHandler.GetConfigFromEntryID(entry._id);
                    var manualEntryPosition = GetManualEntryPosition(entry._id, body.Config.ShipLog);
                    Vector2 entryPosition;
                    if (manualEntryPosition == null)
                        entryPosition = new Vector2(colAccumulator, rowAccumulator);
                    else
                        entryPosition = (Vector2) manualEntryPosition;
                    var newData = new EntryData
                    {
                        id = entry._id,
                        cardPosition = entryPosition,
                        sprite =
                            body.Config.ShipLog.spriteFolder == null ? null : GetEntrySprite(entry._id, body, true),
                        altSprite = body.Config.ShipLog.spriteFolder == null
                            ? null
                            : GetEntrySprite(entry._id + "_ALT", body, false)
                    };
                    entry.SetSprite(
                        newData.sprite == null ? manager._shipLogLibrary.defaultEntrySprite : newData.sprite);
                    entry.SetAltSprite(newData.sprite == null
                        ? manager._shipLogLibrary.defaultEntrySprite
                        : newData.altSprite);
                    manager._entryDataDict.Add(entry._id, newData);
                    var index = manager._entryList.IndexOf(entry);
                    if (index < manager._entryList.Count - 2 &&
                        manager._entryList[index + 1]._astroObjectID != entry._astroObjectID)
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

        private static void AddTranslation(XElement entry)
        {
            var nameElement = entry.Element("Name");
            if (nameElement != null)
            {
                var name = nameElement.Value;
                TranslationHandler.AddShipLog(name);
                foreach (var rumorFact in entry.Elements("RumorFact"))
                {
                    AddTranslationForElement(rumorFact, "RumorName", string.Empty);
                    AddTranslationForElement(rumorFact, "Text", name);
                    AddTranslationForAltText(rumorFact, name);
                }

                foreach (var exploreFact in entry.Elements("ExploreFact"))
                {
                    AddTranslationForElement(exploreFact, "Text", name);
                    AddTranslationForAltText(exploreFact, name);
                }
            }
        }

        private static void AddTranslationForElement(XElement parent, string elementName, string keyName)
        {
            var element = parent.Element(elementName);
            if (element != null) TranslationHandler.AddShipLog(element.Value, keyName);
        }

        private static void AddTranslationForAltText(XElement fact, string keyName)
        {
            var altText = fact.Element("AltText");
            if (altText != null) AddTranslationForElement(altText, "Text", keyName);
        }

        public static void UpdateEntryCuriosity(ref ShipLogEntry entry)
        {
            if (_entryIdToRawName.ContainsKey(entry._id))
            {
                var raw = _entryIdToRawName[entry._id];
                if (_rawNameToCuriosityName.ContainsKey(raw))
                    entry._curiosity = _rawNameToCuriosityName[raw];
                else
                    Logger.LogError(
                        $"Couldn't find {raw}. Did you define the curiosity in a json config? Because you have to.");
            }
        }

        private static Sprite GetEntrySprite(string entryId, NewHorizonsBody body, bool logError)
        {
            var relativePath = body.Config.ShipLog.spriteFolder + "/" + entryId + ".png";
            try
            {
                var newTexture = ImageUtilities.GetTexture(body.Mod, relativePath);
                var rect = new Rect(0, 0, newTexture.width, newTexture.height);
                var pivot = new Vector2(newTexture.width / 2, newTexture.height / 2);
                return Sprite.Create(newTexture, rect, pivot);
            }
            catch (Exception)
            {
                if (logError) Logger.LogError($"Couldn't load image for {entryId} at {relativePath}");
                return null;
            }
        }

        private static Vector2? GetManualEntryPosition(string entryId, ShipLogModule config)
        {
            if (config.entryPositions == null) return null;
            foreach (var position in config.entryPositions)
                if (position.id == entryId)
                    return position.position;
            return null;
        }
    }
}