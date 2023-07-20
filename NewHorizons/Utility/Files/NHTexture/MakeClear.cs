using UnityEngine;

namespace NewHorizons.Utility.Files.NHTexture;

public class MakeClear : ITextureOperation
{
	private readonly int _width;
	private readonly int _height;
	private readonly bool _wrap;

	public MakeClear(int width, int height, bool wrap = false)
	{
		_width = width;
		_height = height;
		_wrap = wrap;
	}

	public string Description => $"clear {_width} {_height} {_wrap}";

	public Texture2D Apply(Texture2D src)
	{
		var dest = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		dest.name = Description;
		var fillColor = Color.clear;
		var fillPixels = new Color[dest.width * dest.height];
		for (int i = 0; i < fillPixels.Length; i++)
		{
			fillPixels[i] = fillColor;
		}
		dest.SetPixels(fillPixels);
		dest.Apply();

		dest.wrapMode = _wrap ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

		return dest;
	}
}
