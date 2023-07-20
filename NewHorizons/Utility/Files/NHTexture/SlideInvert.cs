using UnityEngine;

namespace NewHorizons.Utility.Files.NHTexture;

public class SlideInvert : ITextureOperation
{
	public string Description => "slide invert";

	public Texture2D Apply(Texture2D src)
	{
		var pixels = src.GetPixels();
		for (var i = 0; i < pixels.Length; i++)
		{
			var x = i % src.width;
			var y = i / src.height;

			// Needs a black border
			if (x == 0 || y == 0 || x == src.width - 1 || y == src.height - 1)
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

		src.SetPixels(pixels);
		src.Apply();

		return null;
	}
}
