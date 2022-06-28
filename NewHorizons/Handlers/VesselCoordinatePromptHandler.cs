using NewHorizons.Components;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using static NewHorizons.External.Configs.StarSystemConfig;

namespace NewHorizons.Handlers
{
    public class VesselCoordinatePromptHandler
    {
        private static List<Tuple<string, string, ScreenPrompt>> _factSystemIDPrompt;
        private static List<Texture2D> _textureCache;

        public static void RegisterPrompts(List<NewHorizonsSystem> systems)
        {
            // Have to destroy the images we've created if this isn't the first time it has run
            if (_textureCache != null)
            {
                foreach (var texture in _textureCache)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }

            _textureCache = new List<Texture2D>();
            _factSystemIDPrompt = new List<Tuple<string, string, ScreenPrompt>>();

            foreach (var system in systems)
            {
                var systemName = system.UniqueID;
                var fact = system.Config.factRequiredForWarp;
                var nomaiCoords = system.Config.coords;

                if (system.UniqueID == "EyeOfTheUniverse" || nomaiCoords == null) continue;

                RegisterPrompt(systemName, fact, nomaiCoords);
            }
        }

        private static void RegisterPrompt(string systemID, string fact, NomaiCoordinates coords)
        {
            var texture = MakeTexture(coords.x, coords.y, coords.z);
            
            _textureCache.Add(texture);

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2f, texture.height / 2f));

            var name = ShipLogStarChartMode.UniqueIDToName(systemID);

            var prompt = new ScreenPrompt($"{name}: <EYE>", sprite, 0);

            _factSystemIDPrompt.Add(new (fact, systemID, prompt));

            var manager = Locator.GetPromptManager();
            manager.AddScreenPrompt(prompt, manager.GetScreenPromptList(PromptPosition.LowerLeft), manager.GetTextAnchor(PromptPosition.LowerLeft), -1, false);
        }

        private static Texture2D MakeTexture(int[] x, int[] y, int[] z)
        {
            // Put thing here
            return new Texture2D(1, 1);
        }

        // Gets called from the patches
        public static void SetPromptVisibility(bool visible)
        {
            foreach (var pair in _factSystemIDPrompt)
            {
                var fact = pair.Item1;
                var systemID = pair.Item2;
                var prompt = pair.Item3;

                if (visible)
                {
                    if (Main.Instance.CurrentStarSystem != systemID && (string.IsNullOrEmpty(fact) || Locator.GetShipLogManager().IsFactRevealed(fact)))
                    {
                        prompt.SetVisibility(true);
                    }
                    else
                    {
                        prompt.SetVisibility(false);
                    }
                }
                else
                {
                    prompt.SetVisibility(false);
                }
            }
        }

        public static bool KnowsEyeCoordinates()
        {
            // Works normally in the main system, else check save data directly
            if (Main.Instance.CurrentStarSystem == "SolarSystem") return Locator.GetShipLogManager().IsFactRevealed("OPC_EYE_COORDINATES_X1");
            else return PlayerData._currentGameSave.shipLogFactSaves.ContainsKey("OPC_EYE_COORDINATES_X1");
        }
    }
}
