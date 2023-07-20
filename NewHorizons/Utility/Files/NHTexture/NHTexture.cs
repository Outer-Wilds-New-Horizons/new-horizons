using HarmonyLib;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NewHorizons.Utility.Files.NHTexture;

public class NHTexture
{
	private static readonly Dictionary<string, Texture2D> _cache = new();

	private readonly List<ITextureOperation> _operations = new();

	public void AddOperation(ITextureOperation operation)
	{
		_operations.Add(operation);
	}

	public Texture2D Apply()
	{
		var cacheKey = _operations.Join(x => x.Description, " > ");
		if (_cache.TryGetValue(cacheKey, out var texture))
		{
			return texture;
		}

		ITextureOperation operation = null;
		try
		{
			for (var i = 0; i < _operations.Count; i++)
			{
				operation = _operations[i];
				var src = texture;
				var dest = operation.Apply(src);
				if (dest != null)
				{
					// makes new texture. destroy the old one
					Object.DestroyImmediate(src);
					texture = dest;
				}
				else
				{
					// mutates src. keep it around
					texture = src;
				}
			}
		}
		catch (Exception e)
		{
			NHLogger.LogVerbose($"Exception thrown while performing NHTexture operation {(operation != null ? operation.Description : "NULL")} on texture {(texture != null ? texture.name : "NULL")}:\n{e}");
			return null;
		}

		_cache.Add(cacheKey, texture);
		return texture;
	}
}
