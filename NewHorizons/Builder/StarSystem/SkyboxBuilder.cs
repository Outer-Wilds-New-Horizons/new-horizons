using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using NewHorizons.Utility.OuterWilds;
using OWML.Common;
using UnityEngine;


namespace NewHorizons.Builder.StarSystem
{
    public static class SkyboxBuilder
    {
        private static readonly Shader _unlitShader = Shader.Find("Unlit/Texture");

        public static void Make(SkyboxModule module, IModBehaviour mod)
        {
            NHLogger.Log("Building Skybox");
            BuildSkySphere(module, mod);
        }

        public static GameObject BuildSkySphere(SkyboxModule module, IModBehaviour mod)
        {
            var skybox = SearchUtilities.Find("Skybox");

            var rightTex = ImageUtilities.GetTexture(mod, module.rightPath);
            var leftTex = ImageUtilities.GetTexture(mod, module.leftPath);
            var topTex = ImageUtilities.GetTexture(mod, module.topPath);
            var bottomTex = ImageUtilities.GetTexture(mod, module.bottomPath);
            var frontTex = ImageUtilities.GetTexture(mod, module.frontPath);
            var backTex = ImageUtilities.GetTexture(mod, module.backPath);

            var mesh = BuildSkySphereFaceMesh(module.useCube ? 1 : 32);

            var skySphere = new GameObject("Sky Sphere");
            skySphere.transform.SetParent(skybox.transform, false);
            skySphere.layer = Layer.Skybox;
            skySphere.transform.localScale = Vector3.one * 5f;

            BuildSkySphereFace(skySphere, "Right", Quaternion.Euler(0f, 90f, 0f), mesh, rightTex);
            BuildSkySphereFace(skySphere, "Left", Quaternion.Euler(0f, 270f, 0f), mesh, leftTex);
            BuildSkySphereFace(skySphere, "Top", Quaternion.Euler(270f, 0f, 0f), mesh, topTex);
            BuildSkySphereFace(skySphere, "Bottom", Quaternion.Euler(90f, 0f, 0f), mesh, bottomTex);
            BuildSkySphereFace(skySphere, "Front", Quaternion.Euler(0f, 0f, 0f), mesh, frontTex);
            BuildSkySphereFace(skySphere, "Back", Quaternion.Euler(0f, 180f, 0f), mesh, backTex);

            return skySphere;
        }

        public static GameObject BuildSkySphereFace(GameObject skySphere, string name, Quaternion rotation, Mesh mesh, Texture2D tex)
        {
            if (!tex)
            {
                NHLogger.LogError($"Failed to load texture for skybox {name.ToLowerInvariant()} face");
                return null;
            }

            var go = new GameObject(name)
            {
                layer = Layer.Skybox
            };

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mat = new Material(_unlitShader)
            {
                name = $"Sky Sphere {name}",
                mainTexture = tex
            };

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;

            var sr = go.AddComponent<SkyboxRenderer>();
            Delay.RunWhen(() => SkyboxRenderer.s_active.Contains(sr), () =>
            {
                SkyboxRenderer.s_active.Remove(sr);
                SkyboxRenderer.s_active.Insert(0, sr);
            });

            go.transform.SetParent(skySphere.transform, false);
            go.transform.localRotation = rotation;
            go.transform.localScale = Vector3.one;

            return go;
        }

        public static Mesh BuildSkySphereFaceMesh(int quadsPerAxis)
        {
            var mesh = new Mesh
            {
                name = $"Sky Sphere Face"
            };

            var vertices = new Vector3[(quadsPerAxis + 1) * (quadsPerAxis + 1)];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            var tris = new int[quadsPerAxis * quadsPerAxis * 2 * 3];

            for (var x = 0; x <= quadsPerAxis; x++)
            {
                for (var y = 0; y <= quadsPerAxis; y++)
                {
                    var i = y * (quadsPerAxis + 1) + x;
                    var fx = (float)x / quadsPerAxis;
                    var fy = (float)y / quadsPerAxis;
                    vertices[i] = new Vector3(-0.5f + fx, -0.5f + fy, 0.5f).normalized;
                    normals[i] = -vertices[i].normalized;
                    uvs[i] = new Vector2(fx, fy);
                }
            }

            int t = 0;
            for (var x = 0; x < quadsPerAxis; x++)
            {
                for (var y = 0; y < quadsPerAxis; y++)
                {
                    var i0 = (y + 1) * (quadsPerAxis + 1) + (x + 0);
                    var i1 = (y + 1) * (quadsPerAxis + 1) + (x + 1);
                    var i2 = (y + 0) * (quadsPerAxis + 1) + (x + 1);
                    var i3 = (y + 0) * (quadsPerAxis + 1) + (x + 0);

                    tris[t++] = i0;
                    tris[t++] = i1;
                    tris[t++] = i2;

                    tris[t++] = i2;
                    tris[t++] = i3;
                    tris[t++] = i0;
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = tris;

            return mesh;
        }
    }
}