using UnityEngine;

namespace NewHorizons.Utility.Files.NHTexture;

public class CanvasScale : ITextureOperation
{
	private readonly int _width;
	private readonly int _height;

	public CanvasScale(int width, int height)
	{
		_width = width;
		_height = height;
	}

	public string Description => $"canvas scale {_width} {_height}";

	public Texture2D Apply(Texture2D src)
	{
		var dest = new Texture2D(_width, _height, src.format, src.mipmapCount != 1);
		dest.name = Description;
		var fillPixels = new Color[dest.width * dest.height];
		for (int i = 0; i < dest.width; i++)
		{
			for (int j = 0; j < dest.height; j++)
			{
				var x = i + (dest.width - _width) / 2;
				var y = j + (dest.height - _height) / 2;

				var colour = Color.black;
				if (x < dest.width && y < dest.height) colour = dest.GetPixel(i, j);

				fillPixels[i + j * dest.width] = colour;
			}
		}
		dest.SetPixels(fillPixels);
		dest.Apply();

		dest.wrapMode = dest.wrapMode;

		return dest;
	}
}
