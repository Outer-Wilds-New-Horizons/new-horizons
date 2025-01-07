using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;
using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.Geometry;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public static class RingBuilder
    {
        public static Shader RingV2Shader;
        public static Shader RingV2UnlitShader;
        public static Shader RingV2TransparentShader;
        public static Shader RingV2UnlitTransparentShader;
        public static Texture2D NoiseTexture;
        private static readonly int OnePixelMode = Shader.PropertyToID("_1PixelMode");
        private static readonly int InnerRadius = Shader.PropertyToID("_InnerRadius");
        private static readonly int Noise = Shader.PropertyToID("_Noise");
        private static readonly int NoiseRotation = Shader.PropertyToID("_NoiseRotation");
        private static readonly int NoiseRotationSpeed = Shader.PropertyToID("_NoiseRotationSpeed");
        private static readonly int NoiseMap = Shader.PropertyToID("_NoiseMap");
        private static readonly int SmoothnessMap = Shader.PropertyToID("_SmoothnessMap");
        private static readonly int NormalMap = Shader.PropertyToID("_NormalMap");
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int Translucency = Shader.PropertyToID("_Translucency");
        private static readonly int TranslucentGlow = Shader.PropertyToID("_TranslucentGlow");
        private static readonly int TranslucentGlowExponent = Shader.PropertyToID("_TranslucentGlowExponent");

        public static GameObject Make(GameObject planetGO, Sector sector, RingModule ring, IModBehaviour mod)
        {
            var ringGO = MakeRingGraphics(planetGO, sector, ring, mod);

            // Funny collider thing
            var ringVolume = new GameObject("RingVolume");
            ringVolume.SetActive(false);
            ringVolume.transform.parent = ringGO.transform;
            ringVolume.transform.localPosition = Vector3.zero;
            ringVolume.transform.localScale = Vector3.one;
            ringVolume.transform.localRotation = Quaternion.identity;
            ringVolume.layer = Layer.BasicEffectVolume;

            var ringShape = ringVolume.AddComponent<RingShape>();
            ringShape.innerRadius = ring.innerRadius;
            ringShape.outerRadius = ring.outerRadius;
            ringShape.height = 20f;
            ringShape.center = Vector3.zero;
            ringShape.SetCollisionMode(Shape.CollisionMode.Volume);
            ringShape.SetLayer(Shape.Layer.Default);
            ringShape.layerMask = -1;
            ringShape.pointChecksOnly = true;

            var trigger = ringVolume.AddComponent<OWTriggerVolume>();
            trigger._shape = ringShape;

            var sfv = ringVolume.AddComponent<RingFluidVolume>();
            sfv._fluidType = ring.fluidType.ConvertToOW();
            sfv._density = 5f;

            if (ringGO.TryGetComponent<RingOpacityController>(out var ringOC)) ringOC.SetRingFluidVolume(sfv);

            ringVolume.SetActive(true);



            return ringGO;
        }

        public static GameObject MakeRingGraphics(GameObject rootObject, Sector sector, RingModule ring, IModBehaviour mod)
        {
            var albedoTexture = ImageUtilities.GetTexture(mod, ring.texture ?? "");

            if (albedoTexture == null)
            {
                NHLogger.LogError($"Couldn't load ring texture [{ring.texture}]");
                return null;
            }

            var ringGO = new GameObject(!string.IsNullOrEmpty(ring.rename) ? ring.rename : "Ring");
            ringGO.transform.parent = sector?.transform ?? rootObject.transform;
            ringGO.transform.position = rootObject.transform.position;
            ringGO.transform.rotation = rootObject.transform.rotation;
            ringGO.transform.Rotate(ringGO.transform.TransformDirection(Vector3.up), ring.longitudeOfAscendingNode);
            ringGO.transform.Rotate(ringGO.transform.TransformDirection(Vector3.left), ring.inclination);

            var ringMF = ringGO.AddComponent<MeshFilter>();
            var ringMesh = ringMF.mesh;
            var ringMR = ringGO.AddComponent<MeshRenderer>();

            if (RingV2Shader == null) RingV2Shader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/RingV2.shader");
            if (RingV2TransparentShader == null) RingV2TransparentShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/RingV2Transparent.shader");
            if (RingV2UnlitShader == null) RingV2UnlitShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/RingV2UnlitShader.shader");
            if (RingV2UnlitTransparentShader == null) RingV2UnlitTransparentShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/RingV2UnlitTransparentShader.shader");
            if (NoiseTexture == null) NoiseTexture = Main.NHAssetBundle.LoadAsset<Texture2D>("Assets/Resources/BlueNoise470.png");

            var mat = new Material(ring.unlit ? RingV2UnlitShader : RingV2Shader);
            if (ring.transparencyType == RingTransparencyType.Fade)
            {
                mat = new Material(ring.unlit ? RingV2UnlitTransparentShader : RingV2TransparentShader);
            }
            else
            {
                albedoTexture.anisoLevel = 0; // otherwise it dies from afar if it has a lot of transparency
            }
            
            mat.SetFloat(OnePixelMode, albedoTexture.width == 1 ? 1 : 0);
            mat.mainTexture = albedoTexture;
            mat.SetFloat(InnerRadius, 0);

            mat.SetFloat(Noise, ring.useNoise ?? ring.transparencyType == RingTransparencyType.AlphaClip ? 1 : 0);
            mat.SetFloat(NoiseRotation, ring.noiseRotationSpeed != 0 ? 1 : 0);
            mat.SetFloat(NoiseRotationSpeed, ring.noiseRotationSpeed / 360f);
            mat.SetTexture(NoiseMap, NoiseTexture);
            var middleCircumference = 2f * Mathf.PI * Mathf.Lerp(ring.innerRadius, ring.outerRadius, 0.5f);
            var radiusFix = 1f - (ring.innerRadius / ring.outerRadius);
            var scale = new Vector2(middleCircumference / NoiseTexture.width / ring.noiseScale, radiusFix * ring.outerRadius / NoiseTexture.width / ring.noiseScale);
            mat.SetTextureScale(NoiseMap, scale);

            if (!ring.unlit)
            {
                var smoothnessMap = ImageUtilities.GetTexture(mod, ring.smoothnessMap ?? "");
                var normalMap = ImageUtilities.GetTexture(mod, ring.normalMap ?? "");
                var emissionMap = ImageUtilities.GetTexture(mod, ring.emissionMap ?? "");
                if (smoothnessMap != null) mat.SetTexture(SmoothnessMap, smoothnessMap);
                if (normalMap != null) mat.SetTexture(NormalMap, normalMap);
                if (emissionMap != null) mat.SetTexture(EmissionMap, emissionMap);

                mat.SetFloat(Translucency, ring.translucency);
                mat.SetFloat(TranslucentGlow, ring.translucentGlow ? 1 : 0);
            }

            // Black holes vanish behind rings
            // However if we lower this to where black holes don't vanish, water becomes invisible when seen through rings
            // Vanishing black holes is the lesser bug
            if (ring.transparencyType == RingTransparencyType.Fade) mat.renderQueue = 3000;
            ringMR.material = mat;

            // Make mesh
            var segments = (int)Mathf.Clamp(ring.outerRadius, 20, 2000);
            BuildRingMesh(ringMesh, segments, ring.innerRadius, ring.outerRadius);

            if (ring.rotationSpeed != 0)
            {
                var rot = ringGO.AddComponent<RotateTransform>();
                rot._degreesPerSecond = ring.rotationSpeed;
                rot._localAxis = Vector3.down;
            }

            if (ring.scaleCurve != null)
            {
                var levelController = ringGO.AddComponent<SizeController>();
                levelController.SetScaleCurve(ring.scaleCurve);
            }

            if (ring.opacityCurve != null)
            {
                var ringOC = ringGO.AddComponent<RingOpacityController>();
                ringOC.SetOpacityCurve(ring.opacityCurve);
                ringOC.SetMeshRenderer(ringMR);
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
            Vector2[] uv2 = new Vector2[(segments + 1) * 2 * 2];
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
                uv[i * 2] = uv[i * 2 + halfway] = (new Vector2(x, z) * 0.5f) + new Vector2(0.5f, 0.5f);
                uv[i * 2 + 1] = uv[i * 2 + 1 + halfway] = new Vector2(0.5f, 0.5f);

                float halfwayProgress = progress + 0.5f / segments;
                uv2[i * 2] = new Vector2(progress, 1f);
                uv2[i * 2 + halfway] = new Vector2(halfwayProgress, 1f);
                uv2[i * 2 + 1] = new Vector2(progress, 0f);
                uv2[i * 2 + 1 + halfway] = new Vector2(halfwayProgress, 0f);

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
            ringMesh.uv2 = uv2;
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
