using OWML.Common;
using System.IO;
using UnityEngine;

namespace NewHorizons.Utility
{
    static class ImageUtilities
    {
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

        public static Texture2D GetTexture(IModBehaviour mod, string filename)
        {
            // Copied from OWML but without the print statement lol
            var path = mod.ModHelper.Manifest.ModFolderPath + filename;
            var data = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            return texture;
        }
    }
}
