using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

namespace NewHorizons.Builder.ShipLog
{
    public static class RumorModeBuilder
    {
        private static Dictionary<CuriosityName, Color> _curiosityColors;
        private static Dictionary<CuriosityName, Color> _curiosityHighlightColors;
        private static Dictionary<string, string> _entryIdToRawName;

        public static void Init()
        {
            _curiosityColors = new Dictionary<CuriosityName, Color>();
            _curiosityHighlightColors = new Dictionary<CuriosityName, Color>();
            _entryIdToRawName = new Dictionary<string, string>();
        }

        public static void AddCuriosityColors(ShipLogModule.CuriosityColorInfo[] newColors)
        {
            foreach (ShipLogModule.CuriosityColorInfo newColor in newColors)
            {
                if (!EnumUtils.IsDefined<CuriosityName>(newColor.id))
                {
                    CuriosityName newName = EnumUtilities.Create<CuriosityName>(newColor.id);
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
            string systemName = body.Config.starSystem;
            XElement astroBodyFile = XElement.Load(Path.Combine(body.Mod.ModHelper.Manifest.ModFolderPath, body.Config.ShipLog.xmlFile));
            AddShipLogXML(manager, astroBodyFile, body);
        }

        public static void AddShipLogXML(ShipLogManager manager, XElement xml, NewHorizonsBody body)
        {
            XElement astroBodyId = xml.Element("ID");
            if (astroBodyId == null)
            {
                NHLogger.LogError("Failed to load ship logs for " + body.Config.name + "!");
            }
            else
            {
                var entryIDs = new List<string>();
                foreach (XElement entryElement in xml.DescendantsAndSelf("Entry"))
                {
                    XElement curiosityName = entryElement.Element("Curiosity");
                    XElement id = entryElement.Element("ID");
                    if (id != null)
                    {
                        entryIDs.Add(id.Value);
                        if (curiosityName != null && _entryIdToRawName.ContainsKey(id.Value) == false)
                        {
                            _entryIdToRawName.Add(id.Value, curiosityName.Value);
                        }
                    }
                    foreach (XElement childEntryElement in entryElement.Elements("Entry"))
                    {
                        XElement childCuriosityName = childEntryElement.Element("Curiosity");
                        XElement childId = childEntryElement.Element("ID");
                        if (childId != null)
                        {
                            entryIDs.Add(childId.Value);
                            if (_entryIdToRawName.ContainsKey(childId.Value))
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
                        }
                        AddTranslation(childEntryElement);
                    }
                    AddTranslation(entryElement);
                }
                var newAsset = new TextAsset(xml.ToString());
                var newBodies = new List<TextAsset>(manager._shipLogXmlAssets) { newAsset };
                manager._shipLogXmlAssets = newBodies.ToArray();
                ShipLogHandler.AddConfig(astroBodyId.Value, entryIDs, body);
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
                    NewHorizonsBody body = ShipLogHandler.GetConfigFromEntryID(entry._id);
                    Vector2? manualEntryPosition = GetManualEntryPosition(entry._id, body.Config);
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
                        sprite = body.Config.ShipLog.spriteFolder == null ? null : GetEntrySprite(entry._id, body, true),
                        altSprite = body.Config.ShipLog.spriteFolder == null ? null : GetEntrySprite(entry._id + "_ALT", body, false)
                    };
                    entry.SetSprite(newData.sprite == null ? manager._shipLogLibrary.defaultEntrySprite : newData.sprite);
                    entry.SetAltSprite(newData.sprite == null ? manager._shipLogLibrary.defaultEntrySprite : newData.altSprite);
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

        public static void MergeEntries(ShipLogManager manager, ShipLogEntry entry, ShipLogEntry existing)
        {
            foreach (var fact in entry.GetRumorFacts())
            {
                existing._rumorFacts.Add(fact);
                fact.OnFactRevealed += existing.OnFactRevealed;

                manager._factRevealCount = Mathf.Max(manager._factRevealCount, fact.GetRevealOrder());
                manager._factList.Add(fact);
                manager._factDict.Add(fact.GetID(), fact);
            }
            foreach (var fact in entry.GetExploreFacts())
            {
                existing._exploreFacts.Add(fact);
                existing._completionFacts.Add(fact);
                fact.OnFactRevealed += existing.OnFactRevealed;

                manager._factRevealCount = Mathf.Max(manager._factRevealCount, fact.GetRevealOrder());
                manager._factList.Add(fact);
                manager._factDict.Add(fact.GetID(), fact);
            }
            foreach (var child in entry.GetChildren())
            {
                existing._childEntries.Add(child);

                manager.AddEntry(child);
            }
        }

        private static void AddTranslation(XElement entry)
        {
            XElement nameElement = entry.Element("Name");
            if (nameElement != null)
            {
                string name = nameElement.Value;
                TranslationHandler.AddShipLog(name);
                foreach (XElement rumorFact in entry.Elements("RumorFact"))
                {
                    AddTranslationForElement(rumorFact, "RumorName", string.Empty);
                    AddTranslationForElement(rumorFact, "Text", name);
                    AddTranslationForAltText(rumorFact, name);
                }
                foreach (XElement exploreFact in entry.Elements("ExploreFact"))
                {
                    AddTranslationForElement(exploreFact, "Text", name);
                    AddTranslationForAltText(exploreFact, name);
                }
            }
        }

        private static void AddTranslationForElement(XElement parent, string elementName, string keyName)
        {
            XElement element = parent.Element(elementName);
            if (element != null)
            {
                TranslationHandler.AddShipLog(element.Value, keyName);
            }
        }

        private static void AddTranslationForAltText(XElement fact, string keyName)
        {
            XElement altText = fact.Element("AltText");
            if (altText != null)
            {
                AddTranslationForElement(altText, "Text", keyName);
            }
        }

        public static void UpdateEntryCuriosity(ref ShipLogEntry entry)
        {
            if (_entryIdToRawName.ContainsKey(entry._id))
            {
                var raw = _entryIdToRawName[entry._id];
                if (EnumUtils.TryParse<CuriosityName>(raw, out CuriosityName name))
                {
                    entry._curiosity = name;
                }
                else
                {
                    NHLogger.LogError($"Couldn't find {raw}. Did you define the curiosity in a json config? Because you have to.");
                }
            }
        }

        private static Sprite GetEntrySprite(string entryId, NewHorizonsBody body, bool logError)
        {
            string relativePath = Path.Combine(body.Config.ShipLog.spriteFolder, entryId + ".png");
            try
            {
                Texture2D newTexture = ImageUtilities.GetTexture(body.Mod, relativePath);
                Rect rect = new Rect(0, 0, newTexture.width, newTexture.height);
                Vector2 pivot = new Vector2(newTexture.width / 2, newTexture.height / 2);
                return Sprite.Create(newTexture, rect, pivot);
            }
            catch (Exception)
            {
                if (logError) NHLogger.LogError($"Couldn't load image for {entryId} at {relativePath}");
                return null;
            }
        }

        private static Vector2? GetManualEntryPosition(string entryId, PlanetConfig config)
        {
            Main.SystemDict.TryGetValue(config.starSystem, out var system);
            var entryPositions = system?.Config?.entryPositions;

            if (entryPositions == null) return null;

            foreach (ShipLogModule.EntryPositionInfo position in entryPositions)
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
