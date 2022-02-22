using System;
using System.IO;
using UnityEngine;

namespace NewHorizons.Utility
{
    static class ImageUtilities
    {
        public static Texture2D MakeOutline(Texture2D texture, Color color, int thickness)
        {
            var outline = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            var outlinePixels = new Color[texture.width * texture.height];
            var pixels = texture.GetPixels();

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    var fillColor = new Color(0, 0, 0, 0);

                    if(pixels[x + y * texture.width].a == 1 && CloseToTransparent(pixels, texture.width, texture.height, x, y, thickness))
                    {
                        fillColor = color;
                    }
                    outlinePixels[x + y * texture.width] = fillColor;
                }
            }

            outline.SetPixels(outlinePixels);
            outline.Apply();

            return outline;
        }

        private static bool CloseToTransparent(Color[] pixels, int width, int height, int x, int y, int thickness)
        {
            // Check nearby
            var minX = Math.Max(0, x - thickness/2);
            var minY = Math.Max(0, y - thickness/2);
            var maxX = Math.Min(width, x + thickness/2);
            var maxY = Math.Min(height, y + thickness/2);

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
            var pixels = image.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r *= tint.r;
                pixels[i].g *= tint.g;
                pixels[i].b *= tint.b;
            }

            var newImage = new Texture2D(image.width, image.height);
            newImage.SetPixels(pixels);
            newImage.Apply();
            return newImage;
        }

        public static Texture2D LerpGreyscaleImage(Texture2D image, Color lightTint, Color darkTint)
        {
            var pixels = image.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r = Mathf.Lerp(darkTint.r, lightTint.r, pixels[i].r);
                pixels[i].g = Mathf.Lerp(darkTint.g, lightTint.g, pixels[i].g);
                pixels[i].b = Mathf.Lerp(darkTint.b, lightTint.b, pixels[i].b);
            }

            var newImage = new Texture2D(image.width, image.height);
            newImage.SetPixels(pixels);
            newImage.Apply();
            return newImage;
        }

        public static Texture2D LoadImage(string filepath)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadRawTextureData(File.ReadAllBytes(filepath));
            return tex;
        }

        public static Texture2D ClearTexture(int width, int height)
        {
            var tex = (new Texture2D(1, 1, TextureFormat.ARGB32, false));
            Color fillColor = Color.clear;
            Color[] fillPixels = new Color[tex.width * tex.height];
            for(int i = 0; i < fillPixels.Length; i++)
            {
                fillPixels[i] = fillColor;
            }
            tex.SetPixels(fillPixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D CanvasScaled(Texture2D src, int width, int height)
        {
            var tex = (new Texture2D(width, height, TextureFormat.ARGB32, false));
            Color[] fillPixels = new Color[tex.width * tex.height];
            for (int i = 0; i < tex.width; i++)
            {
                for(int j = 0; j < tex.height; j++)
                {
                    fillPixels[i + j * tex.width] = src.GetPixel(i, j);
                }
            }
            tex.SetPixels(fillPixels);
            tex.Apply();
            return tex;
        }

        public static Color GetAverageColor(Texture2D src)
        {
            var pixels = src.GetPixels32();
            var r = 0f;
            var g = 0f;
            var b = 0f;
            var length = pixels.Length;
            for(int i = 0; i < pixels.Length; i++)
            {
                var color = pixels[i];
                r += (float)color.r / length;
                g += (float)color.g / length;
                b += (float)color.b / length;
            }

            return new Color(r / 255, g / 255, b / 255);
        }
    }
}
