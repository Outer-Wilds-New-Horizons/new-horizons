using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Body
{
    static class CubeSphere
    {
        public static Mesh Build(int resolution, Texture2D heightMap, float minHeight, float maxHeight)
        {
            // It breaks if resolution is greater than 100 I don't know why
            if(resolution > 100)
            {
                Logger.LogWarning($"Can't make CubeSphere's with resolution higher than 100 for some reason");
                resolution = 100;
            }

            Mesh mesh = new Mesh();
            mesh.name = "CubeSphere";

            CreateVertices(mesh, resolution, heightMap, minHeight, maxHeight);
            CreateTriangles(mesh, resolution);

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

        // Thank you Catlikecoding
        private static void CreateVertices(Mesh mesh, int resolution, Texture2D heightMap, float minHeight, float maxHeight)
        {
            int cornerVertices = 8;
            int edgeVertices = (3 * resolution - 3) * 4;
            int faceVertices = (6 * (resolution - 1) * (resolution - 1));

            Vector3[] vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
            Vector3[] normals = new Vector3[vertices.Length];
            Vector2[] uvs = new Vector2[vertices.Length];

            int v = 0;
            for (int y = 0; y <= resolution; y++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    SetVertex(vertices, normals, uvs, v++, x, y, 0, resolution, heightMap, minHeight, maxHeight);
                }
                for (int z = 1; z <= resolution; z++)
                {
                    SetVertex(vertices, normals, uvs, v++, resolution, y, z, resolution, heightMap, minHeight, maxHeight);
                }
                for (int x = resolution - 1; x >= 0; x--)
                {
                    SetVertex(vertices, normals, uvs, v++, x, y, resolution, resolution, heightMap, minHeight, maxHeight);
                }
                for (int z = resolution - 1; z > 0; z--)
                {
                    SetVertex(vertices, normals, uvs, v++, 0, y, z, resolution, heightMap, minHeight, maxHeight);
                }
            }

            for (int z = 1; z < resolution; z++)
            {
                for (int x = 1; x < resolution; x++)
                {
                    SetVertex(vertices, normals, uvs, v++, x, resolution, z, resolution, heightMap, minHeight, maxHeight);
                }
            }
            for (int z = 1; z < resolution; z++)
            {
                for (int x = 1; x < resolution; x++)
                {
                    SetVertex(vertices, normals, uvs, v++, x, 0, z, resolution, heightMap, minHeight, maxHeight);
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
        }

        private static void SetVertex(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int i, int x, int y, int z, int resolution, Texture2D heightMap, float minHeight, float maxHeight)
        {
            var v2 = (new Vector3(x, y, z) - (Vector3.one * (resolution / 2f))).normalized;

            float x2 = v2.x * v2.x;
            float y2 = v2.y * v2.y;
            float z2 = v2.z * v2.z;
            Vector3 v;
            v.x = v2.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
            v.y = v2.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
            v.z = v2.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);

            float latitude = (Mathf.Rad2Deg * Mathf.Acos(v.z / Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z)));
            float longitude = 180f;
            if(v.x > 0) longitude = Mathf.Rad2Deg * Mathf.Atan(v.y / v.x) + 90f;
            if(v.x < 0) longitude = Mathf.Rad2Deg * (Mathf.Atan(v.y / v.x) + Mathf.PI) + 90f;

            float sampleX = heightMap.width * longitude / 360f;
            float sampleY = heightMap.height * latitude / 180f;

            float relativeHeight = heightMap.GetPixel((int)sampleX, (int)sampleY).r;

            normals[i] = v.normalized;
            vertices[i] = normals[i] * (relativeHeight * (maxHeight - minHeight) + minHeight);

            var uvX = sampleX / (float)heightMap.width;
            var uvY = sampleY / (float)heightMap.height;
            uvs[i] = new Vector2(uvX, uvY);
        }

        private static void CreateTriangles(Mesh mesh, int resolution)
        {
            int quads = resolution * resolution * 6;
            int[] triangles = new int[quads * 6];
            int ring = resolution * 4;
            int t = 0, v = 0;

            for (int y = 0; y < resolution; y++, v++)
            {
                for (int q = 0; q < ring - 1; q++, v++)
                {
                    t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
                }
                t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
            }

            t = CreateTopFace(resolution, triangles, t, ring);
            t = CreateBottomFace(resolution, triangles, t, ring, mesh.vertices.Length);

            mesh.triangles = triangles;
        }

        private static int CreateTopFace(int resolution, int[] triangles, int t, int ring)
        {
            int v = ring * resolution;
            for (int x = 0; x < resolution - 1; x++, v++)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
            }
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

            int vMin = ring * (resolution + 1) - 1;
            int vMid = vMin + 1;
            int vMax = v + 2;

            for (int z = 1; z < resolution - 1; z++, vMin--, vMid++, vMax++)
            {
                t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + resolution - 1);
                for (int x = 1; x < resolution - 1; x++, vMid++)
                {
                    t = SetQuad(
                        triangles, t,
                        vMid, vMid + 1, vMid + resolution - 1, vMid + resolution);
                }
                t = SetQuad(triangles, t, vMid, vMax, vMid + resolution - 1, vMax + 1);
            }
            int vTop = vMin - 2;
            t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
            for (int x = 1; x < resolution - 1; x++, vTop--, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            }
            t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

            return t;
        }

        private static int CreateBottomFace(int resolution, int[] triangles, int t, int ring, int numVertices)
        {
            int v = 1;
            int vMid = numVertices - (resolution - 1) * (resolution - 1);
            t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
            for (int x = 1; x < resolution - 1; x++, v++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
            }
            t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

            int vMin = ring - 2;
            vMid -= resolution - 2;
            int vMax = v + 2;

            for (int z = 1; z < resolution - 1; z++, vMin--, vMid++, vMax++)
            {
                t = SetQuad(triangles, t, vMin, vMid + resolution - 1, vMin + 1, vMid);
                for (int x = 1; x < resolution - 1; x++, vMid++)
                {
                    t = SetQuad(
                        triangles, t,
                        vMid + resolution - 1, vMid + resolution, vMid, vMid + 1);
                }
                t = SetQuad(triangles, t, vMid + resolution - 1, vMax + 1, vMid, vMax);
            }

            int vTop = vMin - 1;
            t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
            for (int x = 1; x < resolution - 1; x++, vTop--, vMid++)
            {
                t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            }
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
