using NewHorizons.External;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.External.VariableSize;
using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;

namespace NewHorizons.Builder.Body
{
    public static class RingBuilder
    {
        public static Shader RingShader;
        public static Shader RingShader1Pixel;
        public static Shader UnlitRingShader;
        public static Shader UnlitRingShader1Pixel;

        public static GameObject Make(GameObject planetGO, Sector sector, RingModule ring, IModBehaviour mod)
        {
            // Properly lit shader doesnt work yet
            ring.Unlit = true;

            Texture2D ringTexture;
            try
            {
                ringTexture = ImageUtilities.GetTexture(mod, ring.Texture);
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load Ring texture, {e.Message}, {e.StackTrace}");
                return null;
            }

            var ringGO = new GameObject("Ring");
            ringGO.transform.parent = sector?.transform ?? planetGO.transform;
            ringGO.transform.position = planetGO.transform.position;
            ringGO.transform.rotation = planetGO.transform.rotation;
            ringGO.transform.Rotate(ringGO.transform.TransformDirection(Vector3.up), ring.LongitudeOfAscendingNode);
            ringGO.transform.Rotate(ringGO.transform.TransformDirection(Vector3.right), ring.Inclination);

            var ringMF = ringGO.AddComponent<MeshFilter>();
            var ringMesh = ringMF.mesh;
            var ringMR = ringGO.AddComponent<MeshRenderer>();
            var texture = ringTexture;

            if (RingShader == null) RingShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/Ring.shader");
            if (UnlitRingShader == null) UnlitRingShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/UnlitTransparent.shader");
            if (RingShader1Pixel == null) RingShader1Pixel = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/Ring1Pixel.shader");
            if (UnlitRingShader1Pixel == null) UnlitRingShader1Pixel = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/UnlitRing1Pixel.shader");

            var mat = new Material(ring.Unlit ? UnlitRingShader : RingShader);
            if (texture.width == 1)
            {
                mat = new Material(ring.Unlit ? UnlitRingShader1Pixel : RingShader1Pixel);
                mat.SetFloat("_InnerRadius", 0);
            }
            ringMR.receiveShadows = !ring.Unlit;

            mat.mainTexture = texture;
            mat.renderQueue = 3000;
            ringMR.material = mat;

            // Make mesh
            var segments = (int)Mathf.Clamp(ring.OuterRadius, 20, 2000);
            BuildRingMesh(ringMesh, segments, ring.InnerRadius, ring.OuterRadius);

            if (ring.RotationSpeed != 0)
            {
                var rot = ringGO.AddComponent<RotateTransform>();
                rot._degreesPerSecond = ring.RotationSpeed;
                rot._localAxis = Vector3.down;
            }

            // Funny collider thing
            var ringVolume = new GameObject("RingVolume");
            ringVolume.SetActive(false);
            ringVolume.transform.parent = ringGO.transform;
            ringVolume.transform.localPosition = Vector3.zero;
            ringVolume.transform.localScale = Vector3.one;
            ringVolume.transform.localRotation = Quaternion.identity;
            ringVolume.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var ringShape = ringVolume.AddComponent<RingShape>();
            ringShape.innerRadius = ring.InnerRadius;
            ringShape.outerRadius = ring.OuterRadius;
            ringShape.height = 2f;
            ringShape.center = Vector3.zero;
            ringShape.SetCollisionMode(Shape.CollisionMode.Volume);
            ringShape.SetLayer(Shape.Layer.Default);
            ringShape.layerMask = -1;
            ringShape.pointChecksOnly = true;

            var trigger = ringVolume.AddComponent<OWTriggerVolume>();
            trigger._shape = ringShape;

            var sfv = ringVolume.AddComponent<SimpleFluidVolume>();
            var fluidType = FluidVolume.Type.NONE;

            if (!string.IsNullOrEmpty(ring.FluidType))
            {
                try
                {
                    fluidType = (FluidVolume.Type)Enum.Parse(typeof(FluidVolume.Type), ring.FluidType.ToUpper());
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Couldn't parse fluid volume type [{ring.FluidType}]: {ex.Message}, {ex.StackTrace}");
                }
            }

            sfv._fluidType = fluidType;
            sfv._density = 1f;

            ringVolume.SetActive(true);

            if (ring.Curve != null)
            {
                var levelController = ringGO.AddComponent<SizeController>();
                var curve = new AnimationCurve();
                foreach (var pair in ring.Curve)
                {
                    curve.AddKey(new Keyframe(pair.Time, pair.Value));
                }
                levelController.scaleCurve = curve;
            }

            return ringGO;
        }

        public static void BuildQuadMesh(Mesh mesh, float width)
        {
            Vector3[] vertices;
            int[] tri;
            Vector3[] normals;
            Vector2[] uv;

            vertices = new Vector3[4];
            tri = new int[6];
            normals = new Vector3[4];
            uv = new Vector2[4];

            vertices[0] = new Vector3(-width / 2, 0, -width / 2);
            vertices[1] = new Vector3(width / 2, 0, -width / 2);
            vertices[2] = new Vector3(-width / 2, 0, width / 2);
            vertices[3] = new Vector3(width / 2, 0, width / 2);

            tri[0] = 0;
            tri[1] = 2;
            tri[2] = 1;

            tri[3] = 2;
            tri[4] = 3;
            tri[5] = 1;

            normals[0] = Vector3.up;
            normals[1] = Vector3.up;
            normals[2] = Vector3.up;
            normals[3] = Vector3.up;

            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(1, 0);
            uv[2] = new Vector2(0, 1);
            uv[3] = new Vector2(1, 1);

            mesh.vertices = vertices;
            mesh.triangles = tri;
            mesh.normals = normals;
            mesh.uv = uv;
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
                //uv[i * 2] = uv[i * 2 + halfway] = new Vector2(progress, 0f);
                //uv[i * 2 + 1] = uv[i * 2 + 1 + halfway] = new Vector2(progress, 1f);	  				
                uv[i * 2] = uv[i * 2 + halfway] = (new Vector2(x, z) / 2f) + Vector2.one * 0.5f;
                uv[i * 2 + 1] = uv[i * 2 + 1 + halfway] = new Vector2(0.5f, 0.5f);

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

        public static void BuildCircleMesh(Mesh mesh, int segments, float outerRadius)
        {
            float angleStep = 360.0f / (float)segments;
            List<Vector3> vertexList = new List<Vector3>();
            List<int> triangleList = new List<int>();
            List<Vector2> uv = new List<Vector2>();
            Quaternion quaternion = Quaternion.Euler(0.0f, angleStep, 0f);
            // Make first triangle.
            vertexList.Add(new Vector3(0.0f, 0.0f, 0.0f));  // 1. Circle center.
            vertexList.Add(new Vector3(0.0f, 0f, outerRadius));  // 2. First vertex on circle outline (radius = 0.5f)
            vertexList.Add(quaternion * vertexList[1]);     // 3. First vertex on circle outline rotated by angle)
                                                            // Add triangle indices.
            uv.Add(new Vector2(0.5f, 0.5f));
            uv.Add(new Vector2(0.5f, 1f));
            uv.Add(quaternion * uv[1]);

            triangleList.Add(0);
            triangleList.Add(1);
            triangleList.Add(2);
            for (int i = 0; i < segments - 1; i++)
            {
                triangleList.Add(0);                      // Index of circle center.
                triangleList.Add(vertexList.Count - 1);
                triangleList.Add(vertexList.Count);
                vertexList.Add(quaternion * vertexList[vertexList.Count - 1]);
                uv.Add(quaternion * (uv[uv.Count - 1] - Vector2.one * 0.5f) + Vector3.one * 0.5f);
            }

            mesh.vertices = vertexList.ToArray();
            mesh.triangles = triangleList.ToArray();
            mesh.uv = uv.ToArray();
            mesh.RecalculateNormals();

        }
    }
}
