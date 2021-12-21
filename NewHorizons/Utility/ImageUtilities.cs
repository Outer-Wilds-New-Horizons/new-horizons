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

        // Thank you PETERSVP
        public static Texture2D Scaled(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new Rect(0, 0, width, height);
            _gpu_scale(src, width, height, mode);

            //Get rendered data back to a new texture
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
            result.Resize(width, height);
            result.ReadPixels(texR, 0, 0, true);
            return result;
        }

        public static void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new Rect(0, 0, width, height);
            _gpu_scale(tex, width, height, mode);

            // Update new texture
            tex.Resize(width, height);
            tex.ReadPixels(texR, 0, 0, true);
            tex.Apply(true);    //Remove this if you hate us applying textures for you :)
        }

        // Internal unility that renders the source texture into the RTT - the scaling method itself.
        static void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
        {
            //We need the source texture in VRAM because we render with it
            src.filterMode = fmode;
            src.Apply(true);

            //Using RTT for best quality and performance. Thanks, Unity 5
            RenderTexture rtt = new RenderTexture(width, height, 32);

            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        }
    }
}
