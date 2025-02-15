using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
namespace NewHorizons.Builder.Body.Geometry
{
    public static class Icosphere
    {
        private static readonly float t = (1f + Mathf.Sqrt(5f)) / 2f;
        // By subdivisions, will add to this to memoize computation of icospheres
        private static List<Vector3[]> vertices = new List<Vector3[]>()
        {
            new Vector3[]
            {
                new Vector3(-1, t, 0).normalized,
                new Vector3( 1,  t,  0).normalized,
                new Vector3(-1, -t,  0).normalized,
                new Vector3( 1, -t,  0).normalized,
                new Vector3( 0, -1,  t).normalized,
                new Vector3( 0,  1,  t).normalized,
                new Vector3( 0, -1, -t).normalized,
                new Vector3( 0,  1, -t).normalized,
                new Vector3( t,  0, -1).normalized,
                new Vector3( t,  0,  1).normalized,
                new Vector3(-t,  0, -1).normalized,
                new Vector3(-t,  0,  1).normalized,
            }
        };
        private static List<int[]> triangles = new List<int[]>()
        {
            new int[]
            {
                0, 11, 5,
                0, 5, 1,
                0, 1, 7,
                0, 7, 10,
                0, 10, 11,

                1, 5, 9,
                5, 11, 4,
                11, 10, 2,
                10, 7, 6,
                7, 1, 8,

                3, 9, 4,
                3, 4, 2,
                3, 2, 6,
                3, 6, 8,
                3, 8, 9,

                4, 9, 5,
                2, 4, 11,
                6, 2, 10,
                8, 6, 7,
                9, 8, 1
            }
        };

        public static Mesh Build(int subdivisions, float minHeight, float maxHeight)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Icosphere";

            if (vertices.Count <= subdivisions) RefineFaces(subdivisions);

            var verticesToCopy = vertices[subdivisions];

            Vector3[] newVertices = new Vector3[verticesToCopy.Length];
            Vector3[] normals = new Vector3[verticesToCopy.Length];
            Vector2[] uvs = new Vector2[verticesToCopy.Length];

            var randomOffset = new Vector3(Random.Range(0, 10f), Random.Range(0, 10f), Random.Range(0, 10f));

            for (int i = 0; i < verticesToCopy.Length; i++)
            {
                var v = verticesToCopy[i];

                float latitude = Mathf.Repeat(Mathf.Rad2Deg * Mathf.Acos(v.z / Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z)), 180f);
                float longitude = Mathf.Repeat(Mathf.Rad2Deg * (v.x > 0 ? Mathf.Atan(v.y / v.x) : Mathf.Atan(v.y / v.x) + Mathf.PI) + 90f, 360f);

                float height = Perlin.Noise(v + randomOffset) * (maxHeight - minHeight) + minHeight;

                newVertices[i] = verticesToCopy[i] * height;
                normals[i] = v.normalized;

                var x = longitude / 360f;
                var y = latitude / 180f;

                uvs[i] = new Vector2(x, y);
            }

            // Higher than this and we have to use a different indexFormat
            if (newVertices.Length > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.vertices = newVertices;
            mesh.triangles = triangles[subdivisions];
            mesh.normals = normals;
            mesh.uv = uvs;

            mesh.RecalculateBounds();
            // Unity recalculate normals does not support smooth normals
            //mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

        private static void RefineFaces(int level)
        {
            if (level < vertices.Count) return;

            for (int i = vertices.Count - 1; i < level; i++)
            {
                // Each triangle will be subdivided into 4 new ones
                int[] oldTriangles = triangles[i];
                int[] newTriangles = new int[oldTriangles.Length * 4];

                // Making too many vertices but its fine I guess. Three per old triangle.
                Vector3[] oldVertices = vertices[i];
                Vector3[] newVertices = new Vector3[oldVertices.Length + oldTriangles.Length];
                Array.Copy(oldVertices, newVertices, oldVertices.Length);

                int v = oldVertices.Length;
                int newTrianglesIndex = 0;
                for (int j = 0; j < oldTriangles.Length; j += 3, v += 3)
                {
                    // Old vertex indices
                    var v0Ind = oldTriangles[j];
                    var v1Ind = oldTriangles[j + 1];
                    var v2Ind = oldTriangles[j + 2];

                    // Old vertices
                    var v0 = oldVertices[v0Ind];
                    var v1 = oldVertices[v1Ind];
                    var v2 = oldVertices[v2Ind];

                    // New vertex indices
                    var aInd = v;
                    var bInd = v + 1;
                    var cInd = v + 2;

                    // New vertices
                    var a = GetMidPoint(v0, v1);
                    var b = GetMidPoint(v1, v2);
                    var c = GetMidPoint(v2, v0);

                    // Add the three new vertices to the vertex list
                    newVertices[aInd] = a;
                    newVertices[bInd] = b;
                    newVertices[cInd] = c;

                    // Add the four triangles
                    newTriangles[newTrianglesIndex++] = v0Ind;
                    newTriangles[newTrianglesIndex++] = aInd;
                    newTriangles[newTrianglesIndex++] = cInd;

                    newTriangles[newTrianglesIndex++] = v1Ind;
                    newTriangles[newTrianglesIndex++] = bInd;
                    newTriangles[newTrianglesIndex++] = aInd;

                    newTriangles[newTrianglesIndex++] = v2Ind;
                    newTriangles[newTrianglesIndex++] = cInd;
                    newTriangles[newTrianglesIndex++] = bInd;

                    newTriangles[newTrianglesIndex++] = aInd;
                    newTriangles[newTrianglesIndex++] = bInd;
                    newTriangles[newTrianglesIndex++] = cInd;
                }

                vertices.Add(newVertices);
                triangles.Add(newTriangles);
            }
        }

        private static Vector3 GetMidPoint(Vector3 a, Vector3 b)
        {
            return ((a + b) / 2f).normalized;
        }
    }
}
