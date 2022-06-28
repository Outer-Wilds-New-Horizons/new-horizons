using NewHorizons.Components;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NewHorizons.External.Configs.StarSystemConfig;

namespace NewHorizons.Handlers
{
    public class VesselCoordinatePromptHandler
    {
        private static List<Tuple<string, ScreenPrompt>> _factPromptPair;
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
            _factPromptPair = new List<Tuple<string, ScreenPrompt>>();

            foreach (var system in systems)
            {
                var systemName = system.UniqueID;
                var fact = system.Config.factRequiredForWarp;
                var nomaiCoords = system.Config.coords;

                RegisterPrompt(systemName, fact, nomaiCoords);
            }
        }

        private static void RegisterPrompt(string system, string fact, NomaiCoordinates coords)
        {
            var texture = MakeTexture(coords.x, coords.y, coords.z);
            
            _textureCache.Add(texture);

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2f, texture.height / 2f));

            var name = ShipLogStarChartMode.UniqueIDToName(system);

            var prompt = new ScreenPrompt($"{name} <EYE>", sprite, 0);

            _factPromptPair.Add(new (fact, prompt));

            var manager = Locator.GetPromptManager();
            manager.AddScreenPrompt(prompt, manager.GetScreenPromptList(PromptPosition.LowerLeft), manager.GetTextAnchor(PromptPosition.LowerLeft), -1, true);
        }

        private static Texture2D MakeTexture(int[] x, int[] y, int[] z)
        {
            // Put thing here
            return new Texture2D(1, 1);
        }

        // Gets called from the patches
        public static void SetPromptVisibility(bool visible)
        {
            foreach (var pair in _factPromptPair)
            {
                var fact = pair.Item1;
                var prompt = pair.Item2;

                if (visible)
                {
                    if (string.IsNullOrEmpty(fact) || Locator.GetShipLogManager().IsFactRevealed(fact))
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
    }
}
