using HarmonyLib;
using NewHorizons.Utility.OWML;
using Newtonsoft.Json;
using OWML.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace NewHorizons.Utility.Files
{
    public static class ImageUtilities
    {
        // key is path + applied effects
        private static readonly Dictionary<string, Texture> _textureCache = new();
        public static bool CheckCachedTexture(string key, out Texture existingTexture) => _textureCache.TryGetValue(key, out existingTexture);
        public static void TrackCachedTexture(string key, Texture texture, bool cacheToDisk = true)
        {
            _textureCache[key] = texture;

            // Save files to disk
            if (cacheToDisk && texture is Texture2D texture2D)
            {
                try
                {
                    var fileName = $"{key.GetHashCode()}.png";

                    // TODO in the future: use GetRawTextureData and LoadRawTextureData, which works when doing Texture2D.Compress.
                    // Should be faster than encoding to png too since its just the raw byte array
                    var path = Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, "Cache", Main.Instance.CurrentStarSystem, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, texture2D.EncodeToPNG());

                    // Track the meta data about the image
                    if (!_cacheTextureLookup.ContainsKey(key))
                    {
                        _cacheTextureLookup[key] = new TextureCacheData()
                        {
                            useMipmaps = texture.mipmapCount != 1,
                            linear = false,
                            wrap = texture.wrapMode == TextureWrapMode.Repeat,
                            relativePath = fileName
                        };
                    }
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Failed to cache image {key} to disk {e}");
                }
            }
        }

        /// <summary>
        /// Loads the image cache for the given system
        /// Assumes that loading of the system will be done after 40 frames and then saves the new cached images
        /// </summary>
        /// <param name="starSystem"></param>
        public static void OnSceneLoaded(string starSystem)
        {
            NHLogger.LogVerbose($"Loading disk image cache for {starSystem}");

            // Read the lookup file for this star system
            var lookupPath = Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, "Cache", starSystem, "lookup.json");
            if (File.Exists(lookupPath))
            {
                _cacheTextureLookup = JsonConvert.DeserializeObject<Dictionary<string, TextureCacheData>>(File.ReadAllText(lookupPath));

                // Load all the individual files and add them to the cache
                foreach (var pair in _cacheTextureLookup)
                {
                    var key = pair.Key;
                    var cacheData = pair.Value;
                    try
                    {
                        LoadTexture(key, Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, "Cache", starSystem, cacheData.relativePath),
                            cacheData.useMipmaps, cacheData.wrap, cacheData.linear);
                    }
                    catch (Exception e)
                    {
                        NHLogger.LogWarning($"Failed to load cached image {key} {e}");
                    }
                }
            }

            Delay.FireInNUpdates(() =>
            {
                NHLogger.LogVerbose("Saving image cache lookup");
                var lookupPath = Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, "Cache", Main.Instance.CurrentStarSystem, "lookup.json");
                File.WriteAllText(lookupPath, JsonConvert.SerializeObject(_cacheTextureLookup));
            }, 40);
        }

        private static Dictionary<string, TextureCacheData> _cacheTextureLookup = new();

        private static string GetKey(string path)
        {
            var key = path.Substring(Main.Instance.ModHelper.OwmlConfig.ModsPath.Length);
            if (File.Exists(path))
            {
                key += File.GetLastWriteTime(path).ToString(CultureInfo.InvariantCulture);
            }
            return key;
        }

        public static bool IsTextureLoaded(IModBehaviour mod, string filename)
        {
            var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, filename);
            var key = GetKey(path);
            return _textureCache.ContainsKey(key);
        }

        // needed for backwards compat :P
        public static Texture2D GetTexture(IModBehaviour mod, string filename, bool useMipmaps, bool wrap) => GetTexture(mod, filename, useMipmaps, wrap, false);
        
        // bug: cache only considers file path, not wrap/mips/linear. oh well
        public static Texture2D GetTexture(IModBehaviour mod, string filename, bool useMipmaps = true, bool wrap = false, bool linear = false)
        {
            // Copied from OWML but without the print statement lol
            var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, filename);
            var key = GetKey(path);

            NHLogger.LogVerbose($"Loading image at path: {path}");
            try
            {
                return LoadTexture(key, path, useMipmaps, wrap, linear);
            }
            catch (Exception ex)
            {
                // Half the time when a texture doesn't load it doesn't need to exist so just log verbose
                NHLogger.LogVerbose($"Exception thrown while loading texture [{filename}]:\n{ex}");
                return null;
            }
        }

        private static Texture2D LoadTexture(string key, string path, bool useMipmaps, bool wrap, bool linear)
        {
            if (_textureCache.TryGetValue(key, out var existingTexture))
            {
                NHLogger.LogVerbose($"Already loaded image at path: {path}");
                return (Texture2D)existingTexture;
            }
            else
            {
                var data = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, useMipmaps, linear);
                texture.name = key;
                texture.wrapMode = wrap ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                texture.LoadImage(data);

                // Since this texture was loaded from the disk it makes no sense to cache it to disk
                TrackCachedTexture(key, texture, false);

                return texture;
            }
        }

        public static void DeleteTexture(IModBehaviour mod, string filename, Texture2D texture)
        {
            var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, filename);
            var key = GetKey(path);
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

        public static Texture2D Invert(Texture2D texture)
        {
            var key = $"{texture.name} > invert";
            if (_textureCache.TryGetValue(key, out var existingTexture)) return (Texture2D)existingTexture;

            var pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                var x = i % texture.width;
                var y = (int)Mathf.Floor(i / texture.height);

                // Needs a black border
                if (x == 0 || y == 0 || x == texture.width - 1 || y == texture.height - 1)
                {
                    pixels[i].r = 1;
                    pixels[i].g = 1;
                    pixels[i].b = 1;
                    pixels[i].a = 1;
                }
                else
                {
                    pixels[i].r = 1f - pixels[i].r;
                    pixels[i].g = 1f - pixels[i].g;
                    pixels[i].b = 1f - pixels[i].b;
                }
            }

            var newTexture = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount != 1);
            newTexture.name = key;
            newTexture.SetPixels(pixels);
            newTexture.Apply();

            newTexture.wrapMode = texture.wrapMode;

            TrackCachedTexture(key, newTexture);

            return newTexture;
        }

        public static Texture2D MakeReelTexture(Texture2D[] textures)
        {
            var key = $"SlideReelAtlas of {textures.Join(x => x.name)}";
            if (_textureCache.TryGetValue(key, out var existingTexture)) return (Texture2D)existingTexture;

            var size = 256;

            var texture = new Texture2D(size * 4, size * 4, TextureFormat.ARGB32, false);
            texture.name = key;

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

            TrackCachedTexture(key, texture);

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

            TrackCachedTexture(key, outline);

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

            TrackCachedTexture(key, newImage);

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

            TrackCachedTexture(key, newImage);

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

            TrackCachedTexture(key, tex);

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

            TrackCachedTexture(key, tex);

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
            var pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            var newTexture = new Texture2D(width, height);
            newTexture.SetPixels(pixels);
            newTexture.Apply();
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

        // Modified from https://stackoverflow.com/a/69141085/9643841
        public class AsyncImageLoader : MonoBehaviour
        {
            public List<(int index, string path)> PathsToLoad { get; private set; } = new();

            public class ImageLoadedEvent : UnityEvent<Texture2D, int> { }
            public ImageLoadedEvent imageLoadedEvent = new();

            private readonly object _lockObj = new();

            public bool FinishedLoading { get; private set; }
            private int _loadedCount = 0;

            // TODO: set up an optional “StartLoading” and “StartUnloading” condition on AsyncTextureLoader,
            // and make use of that for at least for projector stuff (require player to be in the same sector as the slides
            // for them to start loading, and unload when the player leaves)

            void Start()
            {
                imageLoadedEvent.AddListener(OnImageLoaded);
                foreach (var (index, path) in PathsToLoad)
                {
                    StartCoroutine(DownloadTexture(path, index));
                }
            }

            private void OnImageLoaded(Texture texture, int index)
            {
                lock (_lockObj)
                {
                    _loadedCount++;

                    if (_loadedCount >= PathsToLoad.Count)
                    {
                        NHLogger.LogVerbose($"Finished loading all textures for {gameObject.name} (one was {PathsToLoad.FirstOrDefault()}");
                        FinishedLoading = true;
                    }
                }
            }

            IEnumerator DownloadTexture(string url, int index)
            {
                var key = GetKey(url);
                lock (_textureCache)
                {
                    if (_textureCache.TryGetValue(key, out var existingTexture))
                    {
                        NHLogger.LogVerbose($"Already loaded image {index}:{url}");
                        imageLoadedEvent?.Invoke((Texture2D)existingTexture, index);
                        yield break;
                    }
                }

                using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);

                yield return uwr.SendWebRequest();

                var hasError = uwr.error != null && uwr.error != "";

                if (hasError)
                {
                    NHLogger.LogError($"Failed to load {index}:{url} - {uwr.error}");
                }
                else
                {
                    var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    texture.name = key;
                    texture.wrapMode = TextureWrapMode.Clamp;

                    var handler = (DownloadHandlerTexture)uwr.downloadHandler;
                    texture.LoadImage(handler.data);

                    lock (_textureCache)
                    {
                        if (_textureCache.TryGetValue(key, out var existingTexture))
                        {
                            NHLogger.LogVerbose($"Already loaded image {index}:{url}");
                            Destroy(texture);
                            texture = (Texture2D)existingTexture;
                        }
                        else
                        {
                            TrackCachedTexture(key, texture);
                        }

                        imageLoadedEvent?.Invoke(texture, index);
                    }
                }
            }
        }
    }
}
