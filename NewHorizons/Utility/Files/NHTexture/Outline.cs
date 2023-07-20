using UnityEngine;

namespace NewHorizons.Utility.Files.NHTexture;

public class Outline : ITextureOperation
{
	private readonly Color _color;
	private readonly int _thickness;

	public Outline(Color color, int thickness)
	{
		_color = color;
		_thickness = thickness;
	}

	public string Description => $"outline {_color} {_thickness}";

	public Texture2D Apply(Texture2D src)
	{
		var outlinePixels = new Color[src.width * src.height];
		var pixels = src.GetPixels();

		for (int x = 0; x < src.width; x++)
		{
			for (int y = 0; y < src.height; y++)
			{
				var fillColor = new Color(0, 0, 0, 0);

				if (pixels[x + y * src.width].a == 1 && CloseToTransparent(pixels, src.width, src.height, x, y, _thickness))
				{
					fillColor = _color;
				}
				outlinePixels[x + y * src.width] = fillColor;
			}
		}

		src.SetPixels(outlinePixels);
		src.Apply();

		return null;
	}

	private static bool CloseToTransparent(Color[] pixels, int width, int height, int x, int y, int thickness)
	{
		// Check nearby
		var minX = Mathf.Max(0, x - thickness / 2);
		var minY = Mathf.Max(0, y - thickness / 2);
		var maxX = Mathf.Min(width, x + thickness / 2);
		var maxY = Mathf.Min(height, y + thickness / 2);

		for (int i = minX; i < maxX; i++)
		{
			for (int j = minY; j < maxY; j++)
			{
				if (pixels[i + j * width].a < 1) return true;
			}
		}
		return false;
	}
}
