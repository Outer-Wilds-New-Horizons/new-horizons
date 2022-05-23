#region

using System;
using NewHorizons.Builder.Atmosphere;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Utility;
using UnityEngine;
using UnityEngine.Rendering;
using Logger = NewHorizons.Utility.Logger;
using Object = UnityEngine.Object;

#endregion

namespace NewHorizons.Builder.Body
{
    public static class ProxyBuilder
    {
        private static Material lavaMaterial;

        private static GameObject _blackHolePrefab;
        private static GameObject _whiteHolePrefab;

        private static readonly string _blackHolePath =
            "TowerTwin_Body/Sector_TowerTwin/Sector_Tower_HGT/Interactables_Tower_HGT/Interactables_Tower_TT/Prefab_NOM_WarpTransmitter (1)/BlackHole/BlackHoleSingularity";

        private static readonly string _whiteHolePath =
            "TowerTwin_Body/Sector_TowerTwin/Sector_Tower_HGT/Interactables_Tower_HGT/Interactables_Tower_CT/Prefab_NOM_WarpTransmitter/WhiteHole/WhiteHoleSingularity";

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int MaxDistortRadius = Shader.PropertyToID("_MaxDistortRadius");
        private static readonly int MassScale = Shader.PropertyToID("_MassScale");
        private static readonly int DistortFadeDist = Shader.PropertyToID("_DistortFadeDist");
        private static readonly int Color1 = Shader.PropertyToID("_Color");


        public static void Make(GameObject planetGO, NewHorizonsBody body)
        {
            if (lavaMaterial == null)
                lavaMaterial = SearchUtilities.FindObjectOfTypeAndName<ProxyOrbiter>("VolcanicMoon_Body").transform
                    .Find("LavaSphere").GetComponent<MeshRenderer>().material;

            var proxyName = $"{body.Config.name}_Proxy";

            var newProxy = new GameObject(proxyName);

            try
            {
                // We want to take the largest size I think
                var realSize = body.Config.Base.surfaceSize;

                if (body.Config.HeightMap != null)
                {
                    HeightMapBuilder.Make(newProxy, null, body.Config.HeightMap, body.Mod, 20);
                    if (realSize < body.Config.HeightMap.maxHeight) realSize = body.Config.HeightMap.maxHeight;
                }

                if (body.Config.Base.groundSize != 0)
                {
                    GeometryBuilder.Make(newProxy, null, body.Config.Base.groundSize);
                    if (realSize < body.Config.Base.groundSize) realSize = body.Config.Base.groundSize;
                }

                if (body.Config.Atmosphere?.clouds != null)
                {
                    CloudsBuilder.MakeTopClouds(newProxy, body.Config.Atmosphere, body.Mod);
                    if (realSize < body.Config.Atmosphere.size) realSize = body.Config.Atmosphere.size;
                }

                if (body.Config.Ring != null)
                {
                    RingBuilder.MakeRingGraphics(newProxy, null, body.Config.Ring, body.Mod);
                    if (realSize < body.Config.Ring.outerRadius) realSize = body.Config.Ring.outerRadius;
                }

                if (body.Config.Star != null)
                {
                    var starGO = StarBuilder.MakeStarProxy(planetGO, newProxy, body.Config.Star);

                    if (realSize < body.Config.Star.size) realSize = body.Config.Star.size;
                }

                if (body.Config.ProcGen != null)
                {
                    ProcGenBuilder.Make(newProxy, null, body.Config.ProcGen);
                    if (realSize < body.Config.ProcGen.scale) realSize = body.Config.ProcGen.scale;
                }

                if (body.Config.Lava != null)
                {
                    var sphere = AddColouredSphere(newProxy, body.Config.Lava.size, body.Config.Lava.Curve,
                        Color.black);
                    if (realSize < body.Config.Lava.size) realSize = body.Config.Lava.size;

                    var material = new Material(lavaMaterial);
                    if (body.Config.Lava.tint != null) material.SetColor(EmissionColor, body.Config.Lava.tint);
                    sphere.GetComponent<MeshRenderer>().material = material;
                }

                if (body.Config.Water != null)
                {
                    var colour = body.Config.Water.tint ?? Color.blue;
                    AddColouredSphere(newProxy, body.Config.Water.size, body.Config.Water.Curve, colour);
                    if (realSize < body.Config.Water.size) realSize = body.Config.Water.size;
                }

                if (body.Config.Sand != null)
                {
                    var colour = body.Config.Sand.Tint ?? Color.yellow;
                    AddColouredSphere(newProxy, body.Config.Sand.Size, body.Config.Sand.Curve, colour);
                    if (realSize < body.Config.Sand.Size) realSize = body.Config.Sand.Size;
                }

                // Could improve this to actually use the proper renders and materials
                if (body.Config.Singularity != null)
                {
                    if (body.Config.Singularity.type == SingularityModule.SingularityType.BlackHole)
                        MakeBlackHole(newProxy, body.Config.Singularity.size);
                    else
                        MakeWhiteHole(newProxy, body.Config.Singularity.size);

                    if (realSize < body.Config.Singularity.size) realSize = body.Config.Singularity.size;
                }

                if (body.Config.Base.hasCometTail) CometTailBuilder.Make(newProxy, null, body.Config);
                if (body.Config.Props?.proxyDetails != null)
                    foreach (var detailInfo in body.Config.Props.proxyDetails)
                        DetailBuilder.Make(newProxy, null, body.Config, body.Mod,
                            body.Mod.ModHelper.Manifest.UniqueName, detailInfo);

                // Remove all collisions if there are any
                foreach (var col in newProxy.GetComponentsInChildren<Collider>()) Object.Destroy(col);

                foreach (var renderer in newProxy.GetComponentsInChildren<Renderer>())
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.enabled = true;
                }

                foreach (var tessellatedRenderer in newProxy.GetComponentsInChildren<TessellatedRenderer>())
                    tessellatedRenderer.enabled = true;

                var proxyController = newProxy.AddComponent<NHProxy>();
                proxyController.astroName = body.Config.name;
                proxyController._realObjectDiameter = realSize;
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"Exception thrown when generating proxy for [{body.Config.name}] : {ex.Message}, {ex.StackTrace}");
                Object.Destroy(newProxy);
            }
        }

        private static GameObject AddColouredSphere(GameObject rootObj, float size,
            VariableSizeModule.TimeValuePair[] curve, Color color)
        {
            var sphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereGO.transform.parent = rootObj.transform;
            sphereGO.transform.localScale = Vector3.one * size;
            sphereGO.transform.position = rootObj.transform.position;

            Object.Destroy(sphereGO.GetComponent<Collider>());

            sphereGO.GetComponent<MeshRenderer>().material.color = color;

            if (curve != null) AddSizeController(sphereGO, curve, size);

            return sphereGO;
        }

        private static void AddSizeController(GameObject go, VariableSizeModule.TimeValuePair[] curve, float size)
        {
            var sizeController = go.AddComponent<SizeController>();
            var animCurve = new AnimationCurve();
            foreach (var pair in curve) animCurve.AddKey(new Keyframe(pair.Time, pair.Value));
            sizeController.scaleCurve = animCurve;
            sizeController.size = size;
        }

        private static void MakeBlackHole(GameObject rootObject, float size)
        {
            if (_blackHolePrefab == null) _blackHolePrefab = GameObject.Find(_blackHolePath);

            var blackHoleShader = _blackHolePrefab.GetComponent<MeshRenderer>().material.shader;
            if (blackHoleShader == null)
                blackHoleShader = _blackHolePrefab.GetComponent<MeshRenderer>().sharedMaterial.shader;

            var blackHoleRender = new GameObject("BlackHoleRender");
            blackHoleRender.transform.parent = rootObject.transform;
            blackHoleRender.transform.localPosition = new Vector3(0, 1, 0);
            blackHoleRender.transform.localScale = Vector3.one * size;

            var meshFilter = blackHoleRender.AddComponent<MeshFilter>();
            meshFilter.mesh = _blackHolePrefab.GetComponent<MeshFilter>().mesh;

            var meshRenderer = blackHoleRender.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(blackHoleShader);
            meshRenderer.material.SetFloat(Radius, size * 0.4f);
            meshRenderer.material.SetFloat(MaxDistortRadius, size * 0.95f);
            meshRenderer.material.SetFloat(MassScale, 1);
            meshRenderer.material.SetFloat(DistortFadeDist, size * 0.55f);

            blackHoleRender.SetActive(true);
        }

        private static void MakeWhiteHole(GameObject rootObject, float size)
        {
            if (_whiteHolePrefab == null) _whiteHolePrefab = GameObject.Find(_whiteHolePath);

            var whiteHoleShader = _whiteHolePrefab.GetComponent<MeshRenderer>().material.shader;
            if (whiteHoleShader == null)
                whiteHoleShader = _whiteHolePrefab.GetComponent<MeshRenderer>().sharedMaterial.shader;

            var whiteHoleRenderer = new GameObject("WhiteHoleRenderer");
            whiteHoleRenderer.transform.parent = rootObject.transform;
            whiteHoleRenderer.transform.localPosition = new Vector3(0, 1, 0);
            whiteHoleRenderer.transform.localScale = Vector3.one * size * 2.8f;

            var meshFilter = whiteHoleRenderer.AddComponent<MeshFilter>();
            meshFilter.mesh = _whiteHolePrefab.GetComponent<MeshFilter>().mesh;

            var meshRenderer = whiteHoleRenderer.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(whiteHoleShader);
            meshRenderer.sharedMaterial.SetFloat(Radius, size * 0.4f);
            meshRenderer.sharedMaterial.SetFloat(DistortFadeDist, size);
            meshRenderer.sharedMaterial.SetFloat(MaxDistortRadius, size * 2.8f);
            meshRenderer.sharedMaterial.SetColor(Color1, new Color(1.88f, 1.88f, 1.88f, 1f));

            whiteHoleRenderer.SetActive(true);
        }
    }
}