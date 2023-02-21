using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility
{
    public class MeshUtilities
    {
        public static Mesh RectangleMeshFromCorners(Vector3[] corners)
        {
            MVector3[] verts = corners.Select(v => (MVector3)v).ToArray();

            int[] triangles = new int[] {
                0, 1, 2,
                1, 3, 2,
            };

            MVector3[] normals = new MVector3[verts.Length];
            for (int i = 0; i<verts.Length; i++) normals[i] = new Vector3(0, 0, 1);

            MVector2[] uv = new MVector2[] {
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(1, 0), new Vector2(1, 1),
            };

            MVector2[] uv2 = new MVector2[] {
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(1, 0), new Vector2(1, 1),
            };

            return new MMesh(verts, triangles, normals, uv, uv2);
        }
    }
}
