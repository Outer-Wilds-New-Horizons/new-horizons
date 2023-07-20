using UnityEngine;

namespace NewHorizons.Utility.Files.NHTexture;

public class Tint : ITextureOperation
{
	private readonly Color _tint;

	public Tint(Color tint)
	{
		_tint = tint;
	}

	public string Description => $"tint {_tint}";

	public Texture2D Apply(Texture2D src)
	{
		var pixels = src.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i].r *= _tint.r;
			pixels[i].g *= _tint.g;
			pixels[i].b *= _tint.b;
		}

		src.SetPixels(pixels);
		src.Apply();

		return null;
	}
}
