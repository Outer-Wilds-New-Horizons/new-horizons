using OWML.Common;
using System.IO;
using UnityEngine;

namespace NewHorizons.Utility.Files.NHTexture;

public class Load : ITextureOperation
{
	private readonly IModBehaviour _mod;
	private readonly string _filename;
	private readonly bool _useMipmaps;
	private readonly bool _wrap;
	private readonly bool _linear;

	public Load(IModBehaviour mod, string filename, bool useMipmaps = true, bool wrap = false, bool linear = false)
	{
		_mod = mod;
		_filename = filename;
		_useMipmaps = useMipmaps;
		_wrap = wrap;
		_linear = linear;

		var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, filename);
		var key = GetKey(path);
		Description = $"load {key} {useMipmaps} {wrap} {linear}";
	}

	public string Description { get; }

	public Texture2D Apply(Texture2D src)
	{
		var path = Path.Combine(_mod.ModHelper.Manifest.ModFolderPath, _filename);

		var data = File.ReadAllBytes(path);
		var dest = new Texture2D(2, 2, TextureFormat.RGBA32, _useMipmaps, _linear);
		dest.name = Description;
		dest.wrapMode = _wrap ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
		dest.LoadImage(data);

		return dest;
	}

	private static string GetKey(string path) => path.Substring(Main.Instance.ModHelper.OwmlConfig.ModsPath.Length);
}
