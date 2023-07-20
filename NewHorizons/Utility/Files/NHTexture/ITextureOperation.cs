using UnityEngine;

namespace NewHorizons.Utility.Files.NHTexture;

public interface ITextureOperation
{
	public string Description { get; }
	public Texture2D Apply(Texture2D src);
}