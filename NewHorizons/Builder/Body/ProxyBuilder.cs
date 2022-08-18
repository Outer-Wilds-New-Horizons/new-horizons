using NewHorizons.Builder.Atmosphere;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;
using NewHorizons.Utility;
using System;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Handlers;

namespace NewHorizons.Builder.Body
{
    public static class ProxyBuilder
    {
        private static Material lavaMaterial;

        private static GameObject _blackHolePrefab;
        private static GameObject _whiteHolePrefab;

        private static readonly string _blackHolePath = "TowerTwin_Body/Sector_TowerTwin/Sector_Tower_HGT/Interactables_Tower_HGT/Interactables_Tower_TT/Prefab_NOM_WarpTransmitter (1)/BlackHole/BlackHoleSingularity";
        private static readonly string _whiteHolePath = "TowerTwin_Body/Sector_TowerTwin/Sector_Tower_HGT/Interactables_Tower_HGT/Interactables_Tower_CT/Prefab_NOM_WarpTransmitter/WhiteHole/WhiteHoleSingularity";
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int MaxDistortRadius = Shader.PropertyToID("_MaxDistortRadius");
        private static readonly int MassScale = Shader.PropertyToID("_MassScale");
        private static readonly int DistortFadeDist = Shader.PropertyToID("_DistortFadeDist");
        private static readonly int Color1 = Shader.PropertyToID("_Color");


        public static void Make(GameObject planetGO, NewHorizonsBody body)
        {
            if (lavaMaterial == null) lavaMaterial = SearchUtilities.FindObjectOfTypeAndName<ProxyOrbiter>("VolcanicMoon_Body").transform.Find("LavaSphere").GetComponent<MeshRenderer>().material;

            var proxyController = ProxyHandler.GetProxy(body.Config.name);
            var proxy = proxyController != null ? proxyController.gameObject : new GameObject($"{body.Config.name}_Proxy");
            proxy.SetActive(false);
            if (proxyController == null)
            {
                proxyController = proxy.AddComponent<NHProxy>();
                proxyController.astroName = body.Config.name;
            }

            var success = SharedMake(planetGO, proxy, proxyController, body);
            if (!success)
            {
                GameObject.Destroy(proxy);
                return;
            }

            proxy.SetActive(true);
        }

        internal static bool SharedMake(GameObject planetGO, GameObject proxy, NHProxy proxyController, NewHorizonsBody body, StellarRemnantProxy stellarRemnantProxy = null)
        {
            try
            {
                // We want to take the largest size I think
                var realSize = body.Config.Base.surfaceSize;

                if (body.Config.HeightMap != null)
                {
                    HeightMapBuilder.Make(proxy, null, body.Config.HeightMap, body.Mod, 20);
                    if (realSize < body.Config.HeightMap.maxHeight) realSize = body.Config.HeightMap.maxHeight;
                }

                if (body.Config.Base.groundSize != 0)
                {
                    GeometryBuilder.Make(proxy, null, body.Config.Base.groundSize);
                    if (realSize < body.Config.Base.groundSize) realSize = body.Config.Base.groundSize;
                }

                Renderer atmosphere = null;
                Renderer fog = null;
                float fogCurveMaxVal = 0;
                Renderer topClouds = null;
                CloudLightningGenerator lightningGenerator = null;
                if (body.Config.Atmosphere != null)
                {
                    atmosphere = AtmosphereBuilder.Make(proxy, null, body.Config.Atmosphere, body.Config.Base.surfaceSize, true).GetComponentInChildren<MeshRenderer>();

                    if (body.Config.Atmosphere.fogSize != 0)
                    {
                        fog = FogBuilder.MakeProxy(proxy, body.Config.Atmosphere);
                        fogCurveMaxVal = body.Config.Atmosphere.fogDensity;
                    }

                    if (body.Config.Atmosphere.clouds != null)
                    {
                        topClouds = CloudsBuilder.MakeTopClouds(proxy, body.Config.Atmosphere, body.Mod).GetComponent<MeshRenderer>();

                        if (body.Config.Atmosphere.clouds.hasLightning) lightningGenerator = CloudsBuilder.MakeLightning(proxy, null, body.Config.Atmosphere, true);

                        if (realSize < body.Config.Atmosphere.size) realSize = body.Config.Atmosphere.size;
                    }
                }

                if (body.Config.Ring != null)
                {
                    RingBuilder.MakeRingGraphics(proxy, null, body.Config.Ring, body.Mod);
                    if (realSize < body.Config.Ring.outerRadius) realSize = body.Config.Ring.outerRadius;
                }

                if (body.Config.Star != null)
                {
                    StarBuilder.MakeStarProxy(planetGO, proxy, body.Config.Star, body.Mod);

                    if (realSize < body.Config.Star.size) realSize = body.Config.Star.size;
                }

                GameObject procGen = null;
                if (body.Config.ProcGen != null)
                {
                    procGen = ProcGenBuilder.Make(proxy, null, body.Config.ProcGen);
                    if (realSize < body.Config.ProcGen.scale) realSize = body.Config.ProcGen.scale;
                }

                if (body.Config.Lava != null)
                {
                    var sphere = AddColouredSphere(proxy, body.Config.Lava.size, body.Config.Lava.curve, Color.black);
                    if (realSize < body.Config.Lava.size) realSize = body.Config.Lava.size;

                    var material = new Material(lavaMaterial);
                    if (body.Config.Lava.tint != null) material.SetColor(EmissionColor, body.Config.Lava.tint.ToColor());
                    sphere.GetComponent<MeshRenderer>().material = material;
                }

                if (body.Config.Water != null)
                {
                    var colour = body.Config.Water.tint?.ToColor() ?? Color.blue;
                    AddColouredSphere(proxy, body.Config.Water.size, body.Config.Water.curve, colour);
                    if (realSize < body.Config.Water.size) realSize = body.Config.Water.size;
                }

                if (body.Config.Sand != null)
                {
                    var colour = body.Config.Sand.tint?.ToColor() ?? Color.yellow;
                    AddColouredSphere(proxy, body.Config.Sand.size, body.Config.Sand.curve, colour);
                    if (realSize < body.Config.Sand.size) realSize = body.Config.Sand.size;
                }

                // Could improve this to actually use the proper renders and materials
                if (body.Config.Props?.singularities != null)
                {
                    foreach (var singularity in body.Config.Props.singularities)
                    {
                        if (singularity.type == SingularityModule.SingularityType.BlackHole)
                        {
                            MakeBlackHole(proxy, singularity.position, singularity.size, singularity.curve);
                        }
                        else
                        {
                            MakeWhiteHole(proxy, singularity.position, singularity.size, singularity.curve);
                        }

                        if (realSize < singularity.size) realSize = singularity.size;
                    }
                }

                if (body.Config.Base.hasCometTail)
                {
                    CometTailBuilder.Make(proxy, null, body.Config);
                }

                if (body.Config.Props?.proxyDetails != null)
                {
                    foreach (var detailInfo in body.Config.Props.proxyDetails)
                    {
                        DetailBuilder.Make(proxy, null, body.Config, body.Mod, detailInfo);
                    }
                }

                NHSupernovaPlanetEffectController supernovaPlanetEffect = null;
                if ((body.Config.ShockEffect == null || body.Config.ShockEffect.hasSupernovaShockEffect)  && body.Config.Star == null && body.Config.name != "Sun" && body.Config.FocalPoint == null && !body.Config.isStellarRemnant)
                {
                    supernovaPlanetEffect = SupernovaEffectBuilder.Make(proxy, null, body.Config, body.Mod, procGen, null, null, null, atmosphere, fog);
                }

                // Remove all collisions if there are any
                foreach (var col in proxy.GetComponentsInChildren<Collider>())
                {
                    GameObject.Destroy(col);
                }

                foreach (var renderer in proxy.GetComponentsInChildren<Renderer>())
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.enabled = true;
                }
                foreach (var tessellatedRenderer in proxy.GetComponentsInChildren<TessellatedRenderer>())
                {
                    tessellatedRenderer.enabled = true;
                }

                if (proxyController != null)
                {
                    proxyController._atmosphere = atmosphere;
                    proxyController._fog = fog;
                    proxyController._fogCurveMaxVal = fogCurveMaxVal;
                    proxyController._topClouds = topClouds;
                    proxyController._lightningGenerator = lightningGenerator;
                    proxyController._supernovaPlanetEffectController = supernovaPlanetEffect;
                    proxyController._realObjectDiameter = realSize;
                    proxyController._baseRealObjectDiameter = realSize;
                }
                else if (stellarRemnantProxy != null)
                {
                    stellarRemnantProxy._atmosphere = atmosphere;
                    stellarRemnantProxy._fog = fog;
                    stellarRemnantProxy._fogCurveMaxVal = fogCurveMaxVal;
                    stellarRemnantProxy._topClouds = topClouds;
                    stellarRemnantProxy._lightningGenerator = lightningGenerator;
                    stellarRemnantProxy._realObjectDiameter = realSize;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception thrown when generating proxy for [{body.Config.name}]:\n{ex}");
                return false;
            }
        }

        internal static GameObject AddColouredSphere(GameObject rootObj, float size, VariableSizeModule.TimeValuePair[] curve, Color color)
        {
            GameObject sphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereGO.transform.name = "ProxySphere";

            sphereGO.transform.parent = rootObj.transform;
            sphereGO.transform.localScale = Vector3.one * size;
            sphereGO.transform.position = rootObj.transform.position;

            GameObject.Destroy(sphereGO.GetComponent<Collider>());

            sphereGO.GetComponent<MeshRenderer>().material.color = color;

            if (curve != null) AddSizeController(sphereGO, curve, size);

            return sphereGO;
        }

        internal static SizeController AddSizeController(GameObject go, VariableSizeModule.TimeValuePair[] curve, float size)
        {
            var sizeController = go.AddComponent<SizeController>();
            sizeController.SetScaleCurve(curve);
            sizeController.size = size;
            return sizeController;
        }

        internal static GameObject MakeBlackHole(GameObject rootObject, MVector3 position, float size, VariableSizeModule.TimeValuePair[] curve = null)
        {
            if (_blackHolePrefab == null) _blackHolePrefab = SearchUtilities.Find(_blackHolePath);

            var blackHoleShader = _blackHolePrefab.GetComponent<MeshRenderer>().material.shader;
            if (blackHoleShader == null) blackHoleShader = _blackHolePrefab.GetComponent<MeshRenderer>().sharedMaterial.shader;

            var blackHoleRender = new GameObject("BlackHoleRender");
            blackHoleRender.transform.parent = rootObject.transform;
            if (position != null) blackHoleRender.transform.localPosition = position;
            else blackHoleRender.transform.localPosition = Vector3.zero;
            blackHoleRender.transform.localScale = Vector3.one * size;

            var meshFilter = blackHoleRender.AddComponent<MeshFilter>();
            meshFilter.mesh = _blackHolePrefab.GetComponent<MeshFilter>().mesh;

            var meshRenderer = blackHoleRender.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(blackHoleShader);
            meshRenderer.material.SetFloat(Radius, size * 0.4f);
            meshRenderer.material.SetFloat(MaxDistortRadius, size * 0.95f);
            meshRenderer.material.SetFloat(MassScale, 1);
            meshRenderer.material.SetFloat(DistortFadeDist, size * 0.55f);

            if (curve != null) AddSizeController(blackHoleRender, curve, size);

            blackHoleRender.SetActive(true);
            return blackHoleRender;
        }

        internal static GameObject MakeWhiteHole(GameObject rootObject, MVector3 position, float size, VariableSizeModule.TimeValuePair[] curve = null)
        {
            if (_whiteHolePrefab == null) _whiteHolePrefab = SearchUtilities.Find(_whiteHolePath);

            var whiteHoleShader = _whiteHolePrefab.GetComponent<MeshRenderer>().material.shader;
            if (whiteHoleShader == null) whiteHoleShader = _whiteHolePrefab.GetComponent<MeshRenderer>().sharedMaterial.shader;

            var whiteHoleRenderer = new GameObject("WhiteHoleRenderer");
            whiteHoleRenderer.transform.parent = rootObject.transform;
            if (position != null) whiteHoleRenderer.transform.localPosition = position;
            else whiteHoleRenderer.transform.localPosition = Vector3.zero;
            whiteHoleRenderer.transform.localScale = Vector3.one * size * 2.8f;

            var meshFilter = whiteHoleRenderer.AddComponent<MeshFilter>();
            meshFilter.mesh = _whiteHolePrefab.GetComponent<MeshFilter>().mesh;

            var meshRenderer = whiteHoleRenderer.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(whiteHoleShader);
            meshRenderer.sharedMaterial.SetFloat(Radius, size * 0.4f);
            meshRenderer.sharedMaterial.SetFloat(DistortFadeDist, size);
            meshRenderer.sharedMaterial.SetFloat(MaxDistortRadius, size * 2.8f);
            meshRenderer.sharedMaterial.SetColor(Color1, new Color(1.88f, 1.88f, 1.88f, 1f));

            if (curve != null) AddSizeController(whiteHoleRenderer, curve, size);

            whiteHoleRenderer.SetActive(true);
            return whiteHoleRenderer;
        }
    }
}