using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
namespace NewHorizons.External.SerializableData
{
    [JsonObject]
    public class MMesh
    {
        public MMesh(MVector3[] vertices, int[] triangles, MVector3[] normals, MVector2[] uv, MVector2[] uv2)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.normals = normals;
            this.uv = uv;
            this.uv2 = uv2;
        }

        public MVector3[] vertices;
        public int[] triangles;
        public MVector3[] normals;
        public MVector2[] uv;
        public MVector2[] uv2;

        public static implicit operator MMesh(Mesh mesh)
        {
            return new MMesh
            (
                mesh.vertices.Select(v => (MVector3)v).ToArray(),
                mesh.triangles,
                mesh.normals.Select(v => (MVector3)v).ToArray(),
                mesh.uv.Select(v => (MVector2)v).ToArray(),
                mesh.uv2.Select(v => (MVector2)v).ToArray()
            );
        }

        public static implicit operator Mesh(MMesh mmesh)
        {
            var mesh = new Mesh();

            mesh.vertices = mmesh.vertices.Select(mv => (Vector3)mv).ToArray();
            mesh.triangles = mmesh.triangles;
            mesh.normals = mmesh.normals.Select(mv => (Vector3)mv).ToArray();
            mesh.uv = mmesh.uv.Select(mv => (Vector2)mv).ToArray();
            mesh.uv2 = mmesh.uv2.Select(mv => (Vector2)mv).ToArray();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
