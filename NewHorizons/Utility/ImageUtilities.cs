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
            return newImage;
        }

        public static Texture2D LoadImage(string filepath)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadRawTextureData(File.ReadAllBytes(filepath));
            return tex;
        }
    }
}
