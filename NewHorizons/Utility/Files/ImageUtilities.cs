using NewHorizons.Builder.Props;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NewHorizons.Utility.Files
{
    public static class ImageUtilities
    {
        // key is path + applied effects
        private static readonly Dictionary<string, Texture> _textureCache = new();
        public static bool CheckCachedTexture(string key, out Texture existingTexture) => _textureCache.TryGetValue(key, out existingTexture);
        public static void TrackCachedTexture(string key, Texture texture) => _textureCache.Add(key, texture); // dont reinsert cuz that causes memory leak!

        public static string GetKey(string path) =>
            path.Substring(Main.Instance.ModHelper.OwmlConfig.ModsPath.Length + 1).Replace('\\', '/');

        public static bool IsTextureLoaded(IModBehaviour mod, string filename)
        {
            var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, filename);
            var key = GetKey(path);
            return _textureCache.ContainsKey(key);
        }

        #region obsolete
        // needed for backwards compat :P
        // idk what mod used it
        [Obsolete]
        public static Texture2D GetTexture(IModBehaviour mod, string filename, bool useMipmaps, bool wrap) => GetTexture(mod, filename, useMipmaps, wrap, false);
        #endregion
        // bug: cache only considers file path, not wrap/mips/linear. oh well
        public static Texture2D GetTexture(IModBehaviour mod, string filename, bool useMipmaps = true, bool wrap = false, bool linear = false)
        {
            if (mod == null)
            {
                NHLogger.LogError("Couldn't get texture, mod is null.");
                return null;
            }
            // Copied from OWML but without the print statement lol
            var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, filename);
            var key = GetKey(path);
            if (_textureCache.TryGetValue(key, out var existingTexture))
            {
                NHLogger.LogVerbose($"Already loaded image at path: {path}");
                return (Texture2D)existingTexture;
            }

            NHLogger.LogVerbose($"Loading image at path: {path}");
            try
            {
                var data = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, useMipmaps, linear);
                texture.name = key;
                texture.wrapMode = wrap ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                texture.LoadImage(data);
                _textureCache.Add(key, texture);

                return texture;
            }
            catch (Exception ex)
            {
                // Half the time when a texture doesn't load it doesn't need to exist so just log verbose
                NHLogger.LogVerbose($"Exception thrown while loading texture [{filename}]:\n{ex}");
                return null;
            }
        }

        public static void DeleteTexture(IModBehaviour mod, string filename, Texture2D texture)
        {
            var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, filename);
            var key = GetKey(path);
            DeleteTexture(key, texture);
        }

        public static void DeleteTexture(string key, Texture2D texture) 
        { 
            if (_textureCache.ContainsKey(key))
            {
                if (_textureCache[key] == texture)
                {
                    _textureCache.Remove(key);
                    UnityEngine.Object.Destroy(texture);
                }
            }

            UnityEngine.Object.Destroy(texture);
        }

        public static void ClearCache()
        {
            NHLogger.LogVerbose("Clearing image cache");

            foreach (var texture in _textureCache.Values)
            {
                if (texture == null) continue;
                UnityEngine.Object.Destroy(texture);
            }
            _textureCache.Clear();
        }

        /// <summary>
        /// used specifically for projected slides.
        /// also adds a border (to prevent weird visual bug) and makes the texture linear (otherwise the projected image is too bright).
        /// </summary>
        public static Texture2D InvertSlideReel(IModBehaviour mod, Texture2D texture, string originalPath)
        {
            var key = $"{texture.name} > invert";
            var cachedPath = "";

            // If we're going to end up caching the texture we must make sure it will end up using the same key
            // Not sure why we check if the originalPath is null but it did that before so
            if (!string.IsNullOrEmpty(originalPath))
            {
                cachedPath = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, ProjectionBuilder.InvertedSlideReelCacheFolder, originalPath.Replace(mod.ModHelper.Manifest.ModFolderPath, ""));
                key = GetKey(cachedPath);
            }

            if (_textureCache.TryGetValue(key, out var existingTexture)) return (Texture2D)existingTexture;

            var pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                var x = i % texture.width;
                var y = (int)Mathf.Floor(i / texture.height);

                // Needs a black border
                if (x == 0 || y == 0 || x == texture.width - 1 || y == texture.height - 1)
                {
                    pixels[i] = Color.white;
                }
                else
                {
                    // convert gamma to linear, then invert
                    // outer wilds will invert, then convert linear to gamma
                    // reversing the process
                    pixels[i] = Color.white - pixels[i].linear;
                }
            }

            var newTexture = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount != 1);
            newTexture.name = key;
            newTexture.SetPixels(pixels);
            newTexture.Apply();

            newTexture.wrapMode = texture.wrapMode;

            _textureCache.Add(key, newTexture);

            // Since doing this is expensive we cache the results to the disk
            // Preloading cached values is done in ProjectionBuilder
            if (!string.IsNullOrEmpty(cachedPath))
            {
                NHLogger.LogVerbose($"Caching inverted image to {cachedPath}");
                Directory.CreateDirectory(Path.GetDirectoryName(cachedPath));
                File.WriteAllBytes(cachedPath, newTexture.EncodeToPNG());
            }

            return newTexture;
        }

        public static Texture2D MakeReelTexture(IModBehaviour mod, Texture2D[] textures, string uniqueSlideReelID)
        {
            if (_textureCache.TryGetValue(uniqueSlideReelID, out var existingTexture)) return (Texture2D)existingTexture;

            var size = 256;

            var texture = new Texture2D(size * 4, size * 4, TextureFormat.ARGB32, false);
            texture.name = uniqueSlideReelID;

            var fillPixels = new Color[size * size * 4 * 4];
            for (int xIndex = 0; xIndex < 4; xIndex++)
            {
                for (int yIndex = 0; yIndex < 4; yIndex++)
                {
                    int index = yIndex * 4 + xIndex;
                    var srcTexture = index < textures.Length ? textures[index] : null;

                    for (int i = 0; i < size; i++)
                    {
                        for (int j = 0; j < size; j++)
                        {
                            var colour = Color.black;

                            if (srcTexture)
                            {
                                var srcX = i * srcTexture.width / (float)size;
                                var srcY = j * srcTexture.height / (float)size;
                                if (srcX >= 0 && srcX < srcTexture.width && srcY >= 0 && srcY < srcTexture.height)
                                {
                                    colour = srcTexture.GetPixel((int)srcX, (int)srcY);
                                }
                            }

                            var x = xIndex * size + i;
                            // Want it to start from the first row from the bottom then go down then modulo around idk
                            // 5 because no pos mod idk
                            var y = (5 - yIndex) % 4 * size + j;

                            var pixelIndex = x + y * size * 4;

                            if (pixelIndex < fillPixels.Length && pixelIndex >= 0) fillPixels[pixelIndex] = colour;
                        }
                    }
                }
            }

            texture.SetPixels(fillPixels);
            texture.Apply();

            _textureCache.Add(uniqueSlideReelID, texture);

            // Since doing this is expensive we cache the results to the disk
            // Preloading cached values is done in ProjectionBuilder
            var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, ProjectionBuilder.AtlasSlideReelCacheFolder, $"{uniqueSlideReelID}.png");
            NHLogger.LogVerbose($"Caching atlas image to {path}");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, texture.EncodeToPNG());

            return texture;
        }

        public static Texture2D MakeOutline(Texture2D texture, Color color, int thickness)
        {
            var key = $"{texture.name} > outline {color} {thickness}";
            if (_textureCache.TryGetValue(key, out var existingTexture)) return (Texture2D)existingTexture;

            var outline = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount != 1);
            outline.name = key;
            var outlinePixels = new Color[texture.width * texture.height];
            var pixels = texture.GetPixels();

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    var fillColor = new Color(0, 0, 0, 0);

                    if (pixels[x + y * texture.width].a == 1 && CloseToTransparent(pixels, texture.width, texture.height, x, y, thickness))
                    {
                        fillColor = color;
                    }
                    outlinePixels[x + y * texture.width] = fillColor;
                }
            }

            outline.SetPixels(outlinePixels);
            outline.Apply();

            outline.wrapMode = texture.wrapMode;

            _textureCache.Add(key, outline);

            return outline;
        }

        private static bool CloseToTransparent(Color[] pixels, int width, int height, int x, int y, int thickness)
        {
            // Check nearby
            var minX = Math.Max(0, x - thickness / 2);
            var minY = Math.Max(0, y - thickness / 2);
            var maxX = Math.Min(width, x + thickness / 2);
            var maxY = Math.Min(height, y + thickness / 2);

            for (int i = minX; i < maxX; i++)
            {
                for (int j = minY; j < maxY; j++)
                {
                    if (pixels[i + j * width].a < 1) return true;
                }
            }
            return false;
        }

        public static Texture2D TintImage(Texture2D image, Color tint)
        {
            var key = $"{image.name} > tint {tint}";
            if (_textureCache.TryGetValue(key, out var existingTexture)) return (Texture2D)existingTexture;

            if (image == null)
            {
                NHLogger.LogError($"Tried to tint null image");
                return null;
            }

            var pixels = image.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r *= tint.r;
                pixels[i].g *= tint.g;
                pixels[i].b *= tint.b;
            }

            var newImage = new Texture2D(image.width, image.height, image.format, image.mipmapCount != 1);
            newImage.name = key;
            newImage.SetPixels(pixels);
            newImage.Apply();

            newImage.wrapMode = image.wrapMode;

            _textureCache.Add(key, newImage);

            return newImage;
        }

        public static Texture2D LerpGreyscaleImage(Texture2D image, Color lightTint, Color darkTint)
        {
            var key = $"{image.name} > lerp greyscale {lightTint} {darkTint}";
            if (_textureCache.TryGetValue(key, out var existingTexture)) return (Texture2D)existingTexture;

            var pixels = image.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r = Mathf.Lerp(darkTint.r, lightTint.r, pixels[i].r);
                pixels[i].g = Mathf.Lerp(darkTint.g, lightTint.g, pixels[i].g);
                pixels[i].b = Mathf.Lerp(darkTint.b, lightTint.b, pixels[i].b);
            }

            var newImage = new Texture2D(image.width, image.height, image.format, image.mipmapCount != 1);
            newImage.name = key;
            newImage.SetPixels(pixels);
            newImage.Apply();

            newImage.wrapMode = image.wrapMode;

            _textureCache.Add(key, newImage);

            return newImage;
        }

        public static Color LerpColor(Color start, Color end, float amount)
        {
            return new Color(Mathf.Lerp(start.r, end.r, amount), Mathf.Lerp(start.g, end.g, amount), Mathf.Lerp(start.b, end.b, amount));
        }

        public static Texture2D LerpGreyscaleImageAlongX(Texture2D image, Color lightTintStart, Color darkTintStart, Color lightTintEnd, Color darkTintEnd)
        {
            var key = $"{image.name} > lerp greyscale {lightTintStart} {darkTintStart} {lightTintEnd} {darkTintEnd}";
            if (_textureCache.TryGetValue(key, out var existingTexture)) return (Texture2D)existingTexture;

            var pixels = image.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                var amount = (i % image.width) / (float) image.width;
                var lightTint = LerpColor(lightTintStart, lightTintEnd, amount);
                var darkTint = LerpColor(darkTintStart, darkTintEnd, amount);

                pixels[i].r = Mathf.Lerp(darkTint.r, lightTint.r, pixels[i].r);
                pixels[i].g = Mathf.Lerp(darkTint.g, lightTint.g, pixels[i].g);
                pixels[i].b = Mathf.Lerp(darkTint.b, lightTint.b, pixels[i].b);
            }

            var newImage = new Texture2D(image.width, image.height, image.format, image.mipmapCount != 1);
            newImage.name = key;
            newImage.SetPixels(pixels);
            newImage.Apply();

            newImage.wrapMode = image.wrapMode;

            _textureCache.Add(key, newImage);

            return newImage;
        }

        public static Texture2D ClearTexture(int width, int height, bool wrap = false)
        {
            var key = $"Clear {width} {height} {wrap}";
            if (_textureCache.TryGetValue(key, out var existingTexture)) return (Texture2D)existingTexture;

            var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.name = key;
            var fillColor = Color.clear;
            var fillPixels = new Color[tex.width * tex.height];
            for (int i = 0; i < fillPixels.Length; i++)
            {
                fillPixels[i] = fillColor;
            }
            tex.SetPixels(fillPixels);
            tex.Apply();

            tex.wrapMode = wrap ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

            _textureCache.Add(key, tex);

            return tex;
        }

        public static Texture2D CanvasScaled(Texture2D src, int width, int height)
        {
            var key = $"{src.name} > canvas scaled {width} {height}";
            if (_textureCache.TryGetValue(key, out var existingTexture)) return (Texture2D)existingTexture;

            var tex = new Texture2D(width, height, src.format, src.mipmapCount != 1);
            tex.name = key;
            var fillPixels = new Color[tex.width * tex.height];
            for (int i = 0; i < tex.width; i++)
            {
                for (int j = 0; j < tex.height; j++)
                {
                    var x = i + (src.width - width) / 2;
                    var y = j + (src.height - height) / 2;

                    var colour = Color.black;
                    if (x < src.width && y < src.height) colour = src.GetPixel(i, j);

                    fillPixels[i + j * tex.width] = colour;
                }
            }
            tex.SetPixels(fillPixels);
            tex.Apply();

            tex.wrapMode = src.wrapMode;

            _textureCache.Add(key, tex);

            return tex;
        }

        public static Color GetAverageColor(Texture2D src)
        {
            var pixels = src.GetPixels32();
            var r = 0f;
            var g = 0f;
            var b = 0f;
            var length = pixels.Length;
            for (int i = 0; i < pixels.Length; i++)
            {
                var color = pixels[i];
                r += (float)color.r / length;
                g += (float)color.g / length;
                b += (float)color.b / length;
            }

            return new Color(r / 255, g / 255, b / 255);
        }
        public static Texture2D MakeSolidColorTexture(int width, int height, Color color)
        {
            var key = $"{color} {width} {height}";
            if (_textureCache.TryGetValue(key, out var existingTexture)) return (Texture2D)existingTexture;

            var pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            var newTexture = new Texture2D(width, height);
            newTexture.SetPixels(pixels);
            newTexture.Apply();

            _textureCache.Add(key, newTexture);

            return newTexture;
        }

        public static Sprite GetButtonSprite(JoystickButton button) => GetButtonSprite(ButtonPromptLibrary.SharedInstance.GetButtonTexture(button));
        public static Sprite GetButtonSprite(KeyCode key) => GetButtonSprite(ButtonPromptLibrary.SharedInstance.GetButtonTexture(key));
        private static Sprite GetButtonSprite(Texture2D texture)
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);
            sprite.name = texture.name;
            return sprite;
        }
    }
}
