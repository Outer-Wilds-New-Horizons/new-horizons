#region

using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

#endregion

namespace NewHorizons.Builder.Body.Geometry
{
    internal static class CubeSphere
    {
        public static Mesh Build(int resolution, Texture2D heightMap, float minHeight, float maxHeight, Vector3 stretch)
        {
            // It breaks if resolution is greater than 100 I don't know why
            if (resolution > 100)
            {
                Logger.LogWarning("Can't make CubeSphere's with resolution higher than 100 for some reason");
                resolution = 100;
            }

            var mesh = new Mesh();
            mesh.name = "CubeSphere";

            float max = 1;
            if (stretch.x > stretch.y && stretch.x > stretch.z)
                max = stretch.x;
            else if (stretch.y > stretch.x && stretch.y > stretch.z)
                max = stretch.y;
            else if (stretch.z > stretch.x && stretch.z > stretch.y)
                max = stretch.z;
            else if (stretch.y == stretch.z && stretch.x > stretch.y)
                max = stretch.x;
            else if (stretch.x == stretch.z && stretch.y > stretch.x)
                max = stretch.y;
            else if (stretch.x == stretch.y && stretch.z > stretch.x)
                max = stretch.z;
            minHeight /= max;
            maxHeight /= max;

            CreateVertices(mesh, resolution, heightMap, minHeight, maxHeight);
            StretchVertices(mesh, stretch);
            CreateTriangles(mesh, resolution);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }

        private static void StretchVertices(Mesh mesh, Vector3 scale)
        {
            var baseVertices = mesh.vertices;

            var vertices = new Vector3[baseVertices.Length];

            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = baseVertices[i];
                vertex.x = vertex.x * scale.x;
                vertex.y = vertex.y * scale.y;
                vertex.z = vertex.z * scale.z;
                vertices[i] = vertex;
            }

            mesh.vertices = vertices;
        }

        // Thank you Catlikecoding
        private static void CreateVertices(Mesh mesh, int resolution, Texture2D heightMap, float minHeight,
            float maxHeight)
        {
            var cornerVertices = 8;
            var edgeVertices = (3 * resolution - 3) * 4;
            var faceVertices = 6 * (resolution - 1) * (resolution - 1);

            var vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];

            var v = 0;
            for (var y = 0; y <= resolution; y++)
            {
                for (var x = 0; x <= resolution; x++)
                    SetVertex(vertices, normals, uvs, v++, x, y, 0, resolution, heightMap, minHeight, maxHeight);
                for (var z = 1; z <= resolution; z++)
                    SetVertex(vertices, normals, uvs, v++, resolution, y, z, resolution, heightMap, minHeight,
                        maxHeight);
                for (var x = resolution - 1; x >= 0; x--)
                    SetVertex(vertices, normals, uvs, v++, x, y, resolution, resolution, heightMap, minHeight,
                        maxHeight);
                for (var z = resolution - 1; z > 0; z--)
                    SetVertex(vertices, normals, uvs, v++, 0, y, z, resolution, heightMap, minHeight, maxHeight);
            }

            for (var z = 1; z < resolution; z++)
            for (var x = 1; x < resolution; x++)
                SetVertex(vertices, normals, uvs, v++, x, resolution, z, resolution, heightMap, minHeight, maxHeight);
            for (var z = 1; z < resolution; z++)
            for (var x = 1; x < resolution; x++)
                SetVertex(vertices, normals, uvs, v++, x, 0, z, resolution, heightMap, minHeight, maxHeight);

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
        }

        private static void SetVertex(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int i, int x, int y, int z,
            int resolution, Texture2D heightMap, float minHeight, float maxHeight)
        {
            var v2 = (new Vector3(x, y, z) - Vector3.one * (resolution / 2f)).normalized;

            var x2 = v2.x * v2.x;
            var y2 = v2.y * v2.y;
            var z2 = v2.z * v2.z;
            Vector3 v;
            v.x = v2.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
            v.y = v2.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
            v.z = v2.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);

            var sphericals = CoordinateUtilities.CartesianToSpherical(v);
            var longitude = sphericals.x;
            var latitude = sphericals.y;

            var sampleX = heightMap.width * longitude / 360f;
            var sampleY = heightMap.height * latitude / 180f;

            var relativeHeight = heightMap.GetPixel((int) sampleX, (int) sampleY).r;

            normals[i] = v.normalized;
            vertices[i] = normals[i] * (relativeHeight * (maxHeight - minHeight) + minHeight);

            var uvX = sampleX / heightMap.width;
            var uvY = sampleY / heightMap.height;
            uvs[i] = new Vector2(uvX, uvY);
        }

        private static void CreateTriangles(Mesh mesh, int resolution)
        {
            var quads = resolution * resolution * 6;
            var triangles = new int[quads * 6];
            var ring = resolution * 4;
            int t = 0, v = 0;

            for (var y = 0; y < resolution; y++, v++)
            {
                for (var q = 0; q < ring - 1; q++, v++) t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
                t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
            }

            t = CreateTopFace(resolution, triangles, t, ring);
            t = CreateBottomFace(resolution, triangles, t, ring, mesh.vertices.Length);

            mesh.triangles = triangles;
        }

        private static int CreateTopFace(int resolution, int[] triangles, int t, int ring)
        {
            var v = ring * resolution;
            for (var x = 0; x < resolution - 1; x++, v++) t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

            var vMin = ring * (resolution + 1) - 1;
            var vMid = vMin + 1;
            var vMax = v + 2;

            for (var z = 1; z < resolution - 1; z++, vMin--, vMid++, vMax++)
            {
                t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + resolution - 1);
                for (var x = 1; x < resolution - 1; x++, vMid++)
                    t = SetQuad(
                        triangles, t,
                        vMid, vMid + 1, vMid + resolution - 1, vMid + resolution);
                t = SetQuad(triangles, t, vMid, vMax, vMid + resolution - 1, vMax + 1);
            }

            var vTop = vMin - 2;
            t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
            for (var x = 1; x < resolution - 1; x++, vTop--, vMid++)
                t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

            return t;
        }

        private static int CreateBottomFace(int resolution, int[] triangles, int t, int ring, int numVertices)
        {
            var v = 1;
            var vMid = numVertices - (resolution - 1) * (resolution - 1);
            t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
            for (var x = 1; x < resolution - 1; x++, v++, vMid++) t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
            t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

            var vMin = ring - 2;
            vMid -= resolution - 2;
            var vMax = v + 2;

            for (var z = 1; z < resolution - 1; z++, vMin--, vMid++, vMax++)
            {
                t = SetQuad(triangles, t, vMin, vMid + resolution - 1, vMin + 1, vMid);
                for (var x = 1; x < resolution - 1; x++, vMid++)
                    t = SetQuad(
                        triangles, t,
                        vMid + resolution - 1, vMid + resolution, vMid, vMid + 1);
                t = SetQuad(triangles, t, vMid + resolution - 1, vMax + 1, vMid, vMax);
            }

            var vTop = vMin - 1;
            t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
            for (var x = 1; x < resolution - 1; x++, vTop--, vMid++)
                t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

            return t;
        }

        private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
        {
            triangles[i] = v00;
            triangles[i + 1] = triangles[i + 4] = v01;
            triangles[i + 2] = triangles[i + 3] = v10;
            triangles[i + 5] = v11;
            return i + 6;
        }
    }
}