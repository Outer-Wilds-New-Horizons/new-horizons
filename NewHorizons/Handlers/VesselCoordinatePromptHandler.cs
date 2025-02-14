using NewHorizons.Components.ShipLog;
using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static NewHorizons.External.Configs.StarSystemConfig;

namespace NewHorizons.Handlers
{
    public static class VesselCoordinatePromptHandler
    {
        private static List<Tuple<string, string, ScreenPrompt>> _factSystemIDPrompt;
        // TODO: move this to ImageUtilities
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
                var fact = system.Config.Vessel.promptFact;
                var nomaiCoords = system.Config.Vessel?.coords;

                if (system.UniqueID == "EyeOfTheUniverse" || nomaiCoords == null) continue;

                RegisterPrompt(systemName, fact, nomaiCoords);
            }
        }

        private static void RegisterPrompt(string systemID, string fact, NomaiCoordinates coords)
        {
            var texture = MakeTexture(coords.x, coords.y, coords.z);

            if (_textureCache == null) _textureCache = new List<Texture2D>();
            _textureCache.Add(texture);

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2f, texture.height / 2f), 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);

            var name = ShipLogStarChartMode.UniqueIDToName(systemID);

            var prompt = new ScreenPrompt($"{name}: <EYE>", sprite, 0);

            _factSystemIDPrompt.Add(new (fact, systemID, prompt));

            var manager = Locator.GetPromptManager();
            manager.AddScreenPrompt(prompt, manager.GetScreenPromptList(PromptPosition.LowerLeft), manager.GetTextAnchor(PromptPosition.LowerLeft), -1, false);
        }

        // Gets called from the patches
        public static void SetPromptVisibility(bool visible)
        {
            if (_factSystemIDPrompt == null) return;
            foreach (var pair in _factSystemIDPrompt)
            {
                var fact = pair.Item1;
                var systemID = pair.Item2;
                var prompt = pair.Item3;

                if (prompt != null)
                {
                    if (visible)
                    {
                        if (Main.Instance.CurrentStarSystem != systemID && (string.IsNullOrEmpty(fact) || ShipLogHandler.KnowsFact(fact)))
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

        private static Texture2D MakeTexture(params int[][] coords)
        {
            if (coords == null || coords.Length == 0)
            {
                return null;
            }
            int width = 96;
            int height = 96;
            Texture2D texture = new Texture2D(width * coords.Length, height, TextureFormat.RGBA32, false, false);
            texture.SetPixels(Enumerable.Repeat(Color.clear, texture.width * texture.height).ToArray());
            float offset = 0f;
            for (int i = 0; i < coords.Length; i++)
            {
                if (coords[i] == null || coords[i].Length < 2)
                {
                    continue;
                }
                // Remove extra space if coordinate doesn't use left slot
                if (!coords[i].Contains(5))
                {
                    offset -= width * 0.175f;
                }
                Rect rect = new Rect(offset, 0f, width, height);
                DrawCoordinateLines(texture, rect, coords[i]);
                // Remove extra space if coordinate doesn't use right slot
                if (!coords[i].Contains(2))
                {
                    offset -= width * 0.175f;
                }
                offset += width;
            }
            texture.Apply();
            return texture;
        }

        private static void DrawCoordinateLines(Texture2D texture, Rect rect, int[] coords)
        {
            if (coords == null || coords.Length < 2)
            {
                return;
            }
            float lineWidth = 3f;
            for (int i = 0; i < coords.Length - 1; i++)
            {
                // Calculate start and end points
                Vector2 size = rect.size;
                Vector2 center = size * 0.5f;
                float radius = Mathf.Min(size.x, size.y) * 0.475f - lineWidth;

                float angle0 = Mathf.Deg2Rad * (120f - (60f * coords[i + 0]));
                Vector2 pos0 = rect.position + center + new Vector2(Mathf.Cos(angle0), Mathf.Sin(angle0)) * radius;
                Vector2 start = new Vector2(Mathf.Round(pos0.x), Mathf.Round(pos0.y));

                float angle1 = Mathf.Deg2Rad * (120f - (60f * coords[i + 1]));
                Vector2 pos1 = rect.position + center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
                Vector2 end = new Vector2(Mathf.Round(pos1.x), Mathf.Round(pos1.y));

                // Draw lines
                int x0 = Mathf.FloorToInt(Mathf.Min(start.x, end.x) - lineWidth * 2f);
                int y0 = Mathf.FloorToInt(Mathf.Min(start.y, end.y) - lineWidth * 2f);
                int x1 = Mathf.CeilToInt(Mathf.Max(start.x, end.x) + lineWidth * 2f);
                int y1 = Mathf.CeilToInt(Mathf.Max(start.y, end.y) + lineWidth * 2f);

                Vector2 dir = end - start;
                float length = dir.magnitude;
                dir.Normalize();

                for (int x = x0; x <= x1; x++)
                {
                    for (int y = y0; y <= y1; y++)
                    {
                        Vector2 p = new Vector2(x, y);
                        float dot = Vector2.Dot(p - start, dir);
                        dot = Mathf.Clamp(dot, 0f, length);
                        Vector2 pointOnLine = start + dir * dot;
                        float distToLine = Mathf.Max(0f, Vector2.Distance(p, pointOnLine) - lineWidth);
                        if (distToLine <= 1f)
                        {
                            // Line is within 1 pixel, fill with color (with anti-aliased blending)
                            Color color = Color.white;
                            float blend = 1f - Mathf.Clamp01(distToLine);

                            if (color.a * blend < 1f)
                            {
                                Color existing = texture.GetPixel(x, y);
                                if (existing.a > 0f)
                                {
                                    float colorA = color.a;
                                    color.a = 1f;
                                    texture.SetPixel(x, y, Color.Lerp(existing, color, Mathf.Clamp01(colorA * blend)));
                                } else
                                {
                                    color.a *= blend;
                                    texture.SetPixel(x, y, color);
                                }
                            } else
                            {
                                color.a *= blend;
                                texture.SetPixel(x, y, color);
                            }
                        }
                    }
                }
            }
        }

        public static bool KnowsEyeCoordinates() => ShipLogHandler.KnowsFact("OPC_EYE_COORDINATES_X1");
    }
}
