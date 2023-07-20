using UnityEngine;

namespace NewHorizons.Utility.Files.NHTexture;

public class LerpGreyscale : ITextureOperation
{
	private readonly Color _lightTint;
	private readonly Color _darkTint;

	public LerpGreyscale(Color lightTint, Color darkTint)
	{
		_lightTint = lightTint;
		_darkTint = darkTint;
	}

	public string Description => $"lerp greyscale {_lightTint} {_darkTint}";

	public Texture2D Apply(Texture2D src)
	{
		var pixels = src.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i].r = Mathf.Lerp(_darkTint.r, _lightTint.r, pixels[i].r);
			pixels[i].g = Mathf.Lerp(_darkTint.g, _lightTint.g, pixels[i].g);
			pixels[i].b = Mathf.Lerp(_darkTint.b, _lightTint.b, pixels[i].b);
		}

		src.SetPixels(pixels);
		src.Apply();

		return null;
	}
}
