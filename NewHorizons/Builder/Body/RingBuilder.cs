using NewHorizons.External;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    static class RingBuilder
    {
		public static Shader RingShader;
		public static Shader UnlitShader;

		public static void Make(GameObject body, RingModule ring, IModAssets assets)
        {
			Texture2D ringTexture;
			try
			{
				ringTexture = assets.GetTexture(ring.Texture);
			}
			catch (Exception e)
			{
				Logger.LogError($"Couldn't load Ring texture, {e.Message}, {e.StackTrace}");
				return;
			}

			var ringGO = new GameObject("Ring");
            ringGO.transform.parent = body.transform;
			ringGO.transform.localPosition = Vector3.zero;
			ringGO.transform.localRotation = Quaternion.Euler(0, 0, 0);
			ringGO.transform.Rotate(ringGO.transform.TransformDirection(Vector3.up), ring.LongitudeOfAscendingNode);
			ringGO.transform.Rotate(ringGO.transform.TransformDirection(Vector3.right), ring.Inclination);

            var ringMF = ringGO.AddComponent<MeshFilter>();
            var ringMesh = ringMF.mesh;
            var ringMR = ringGO.AddComponent<MeshRenderer>();
			var texture = ringTexture;

			if (RingShader == null) RingShader = Main.ShaderBundle.LoadAsset<Shader>("Assets/Shaders/Ring.shader");
			if (UnlitShader == null) UnlitShader = Main.ShaderBundle.LoadAsset<Shader>("Assets/Shaders/UnlitTransparent.shader");

			var mat = new Material(ring.Unlit ? UnlitShader : RingShader);
			mat.mainTexture = texture;
			mat.renderQueue = 2895;
			ringMR.material = mat;

			// Make mesh
			var segments = (int)Mathf.Clamp(ring.OuterRadius, 20, 2000); 
			BuildRingMesh(ringMesh, segments, ring.InnerRadius, ring.OuterRadius);
		}

		// Thank you https://github.com/boardtobits/planet-ring-mesh/blob/master/PlanetRing.cs
		public static void BuildRingMesh(Mesh ringMesh, int segments, float innerRadius, float outerRadius)
        {
			Vector3[] vertices = new Vector3[(segments + 1) * 2 * 2];
			int[] triangles = new int[segments * 6 * 2];
			Vector2[] uv = new Vector2[(segments + 1) * 2 * 2];
			int halfway = (segments + 1) * 2;

			for (int i = 0; i < segments + 1; i++)
			{
				float progress = (float)i / (float)segments;
				float angle = progress * 2 * Mathf.PI;
				float x = Mathf.Sin(angle);
				float z = Mathf.Cos(angle);

				vertices[i * 2] = vertices[i * 2 + halfway] = new Vector3(x, 0f, z) * outerRadius;
				vertices[i * 2 + 1] = vertices[i * 2 + 1 + halfway] = new Vector3(x, 0f, z) * innerRadius;
				uv[i * 2] = uv[i * 2 + halfway] = new Vector2(progress, 0f);
				uv[i * 2 + 1] = uv[i * 2 + 1 + halfway] = new Vector2(progress, 1f);

				if (i != segments)
				{
					triangles[i * 12] = i * 2;
					triangles[i * 12 + 1] = triangles[i * 12 + 4] = (i + 1) * 2;
					triangles[i * 12 + 2] = triangles[i * 12 + 3] = i * 2 + 1;
					triangles[i * 12 + 5] = (i + 1) * 2 + 1;

					triangles[i * 12 + 6] = i * 2 + halfway;
					triangles[i * 12 + 7] = triangles[i * 12 + 10] = i * 2 + 1 + halfway;
					triangles[i * 12 + 8] = triangles[i * 12 + 9] = (i + 1) * 2 + halfway;
					triangles[i * 12 + 11] = (i + 1) * 2 + 1 + halfway;
				}

			}

			ringMesh.vertices = vertices;
			ringMesh.triangles = triangles;
			ringMesh.uv = uv;
			ringMesh.RecalculateNormals();
		}
    }
}
