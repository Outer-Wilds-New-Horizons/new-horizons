using NewHorizons.Builder.Atmosphere;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.Components.Props;
using NewHorizons.Components.SizeControllers;
using NewHorizons.External;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public static class ProxyBuilder
    {
        private static Material lavaMaterial;

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        public static void Make(GameObject planetGO, NewHorizonsBody body, NewHorizonsBody remnant)
        {
            if (lavaMaterial == null) lavaMaterial = SearchUtilities.FindObjectOfTypeAndName<ProxyOrbiter>("VolcanicMoon_Body")?.transform.Find("LavaSphere").GetComponent<MeshRenderer>().material;

            var proxyController = ProxyHandler.GetProxy(body.Config.name);
            var proxy = proxyController != null ? proxyController.gameObject : new GameObject($"{body.Config.name}_Proxy");

            proxy.SetActive(false);
            if (proxyController == null)
            {
                proxyController = proxy.AddComponent<NHProxy>();
                proxyController.astroName = body.Config.name;
                proxyController.planet = planetGO;
            }

            if (Main.Instance.CurrentStarSystem == "EyeOfTheUniverse")
            {
                // Disable any proxies when not at eye, vessel, or vortex.
                EyeStateActivationController eyeStateActivation = SearchUtilities.Find("SolarSystemRoot").AddComponent<EyeStateActivationController>();
                eyeStateActivation._object = proxy;
                eyeStateActivation._activeStates = new EyeState[3]
                {
                    EyeState.AboardVessel,
                    EyeState.WarpedToSurface,
                    EyeState.IntoTheVortex
                };
            }

            var rootProxy = new GameObject("Root");
            rootProxy.transform.parent = proxy.transform;
            rootProxy.transform.localPosition = Vector3.zero;

            var success = SharedMake(planetGO, rootProxy, proxyController, body);
            if (!success)
            {
                UnityEngine.Object.Destroy(proxy);
                return;
            }

            proxyController.root = rootProxy;

            // Add remnants
            if (remnant != null)
            {
                NHLogger.LogVerbose($"Making custom remnant proxy");

                var remnantGO = new GameObject("Remnant");
                remnantGO.transform.parent = proxy.transform;
                remnantGO.transform.localPosition = Vector3.zero;

                SharedMake(planetGO, remnantGO, null, remnant);

                proxyController.stellarRemnantGO = remnantGO;
            }
            else if (body.Config.Star != null && StellarRemnantBuilder.HasRemnant(body.Config.Star))
            {
                NHLogger.LogVerbose($"Making remnant proxy");

                var remnantGO = new GameObject("Remnant");
                remnantGO.transform.parent = proxy.transform;
                remnantGO.transform.localPosition = Vector3.zero;

                StellarRemnantBuilder.MakeProxyRemnant(planetGO, remnantGO, body.Mod, body.Config.Star);

                proxyController.stellarRemnantGO = remnantGO;
            }

            proxy.SetActive(true);
        }

        private static bool SharedMake(GameObject planetGO, GameObject proxy, NHProxy proxyController, NewHorizonsBody body)
        {
            try
            {
                // We want to take the largest size I think
                var realSize = body.Config.Base.surfaceSize;

                if (realSize <= 0)
                {
                    // #941 handle proxy body edge case when all scales = 0
                    realSize = 1;
                }

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
                        fog = FogBuilder.MakeProxy(proxy, body.Config.Atmosphere, body.Mod);
                        fogCurveMaxVal = body.Config.Atmosphere.fogDensity;
                    }

                    if (body.Config.Atmosphere.clouds != null)
                    {
                        if (body.Config.Atmosphere.clouds.cloudsPrefab != External.Modules.CloudPrefabType.Transparent) topClouds = CloudsBuilder.MakeTopClouds(proxy, body.Config.Atmosphere, body.Mod).GetComponent<MeshRenderer>();
                        else topClouds = CloudsBuilder.MakeTransparentClouds(proxy, body.Config.Atmosphere, body.Mod, true).GetAddComponent<MeshRenderer>();

                        if (body.Config.Atmosphere.clouds.hasLightning) lightningGenerator = CloudsBuilder.MakeLightning(proxy, null, body.Config.Atmosphere, true);

                        if (realSize < body.Config.Atmosphere.size) realSize = body.Config.Atmosphere.size;
                    }
                }

                if (body.Config.Rings != null)
                {
                    foreach (var ring in body.Config.Rings)
                    {
                        RingBuilder.MakeRingGraphics(proxy, null, ring, body.Mod);
                        if (realSize < ring.outerRadius) realSize = ring.outerRadius;
                    }
                }

                Renderer starAtmosphere = null;
                Renderer starFog = null;
                if (body.Config.Star != null)
                {
                    (_, starAtmosphere, starFog) = StarBuilder.MakeStarProxy(planetGO, proxy, body.Config.Star, body.Mod, body.Config.isStellarRemnant);

                    if (realSize < body.Config.Star.size) realSize = body.Config.Star.size;
                }

                GameObject procGen = null;
                if (body.Config.ProcGen != null)
                {
                    procGen = ProcGenBuilder.Make(body.Mod, proxy, null, body.Config.ProcGen);
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
                        var polarity = singularity.type == SingularityModule.SingularityType.BlackHole;
                        SingularityBuilder.MakeSingularityProxy(proxy, singularity.position, polarity, singularity.horizonRadius, singularity.distortRadius, singularity.curve, singularity.renderQueueOverride);
                        if (realSize < singularity.distortRadius) realSize = singularity.distortRadius;
                    }
                }

                if (body.Config.CometTail != null)
                {
                    CometTailBuilder.Make(proxy, null, body.Config.CometTail, body.Config);
                }

                if (body.Config.Props?.proxyDetails != null)
                {
                    foreach (var detailInfo in body.Config.Props.proxyDetails)
                    {
                        // Thought about switching these to SimplifiedDetailInfo but we use AlignRadial with these so we can't
                        DetailBuilder.Make(proxy, null, body.Mod, detailInfo);
                    }
                }

                NHSupernovaPlanetEffectController supernovaPlanetEffect = null;
                if ((body.Config.ShockEffect == null || body.Config.ShockEffect.hasSupernovaShockEffect)  && body.Config.Star == null && body.Config.name != "Sun" && body.Config.FocalPoint == null && !body.Config.isStellarRemnant)
                {
                    supernovaPlanetEffect = SupernovaEffectBuilder.Make(proxy, null, body.Config, body.Mod, procGen, null, null, null, atmosphere, fog);
                }

                // Remove all collisions if there are any
                foreach (var col in proxy.GetComponentsInChildren<Collider>()) UnityEngine.Object.Destroy(col);
                foreach (var col in proxy.GetComponentsInChildren<OWCollider>()) UnityEngine.Object.Destroy(col);

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
                    proxyController._atmosphere = atmosphere ?? starAtmosphere;
                    if (fog != null)
                    {
                        proxyController._fog = fog;
                        proxyController._fogCurveMaxVal = fogCurveMaxVal;
                    }
                    else if (starFog != null)
                    {
                        proxyController._fog = starFog;
                        proxyController._fogCurveMaxVal = 0.05f;
                    }
                    proxyController.topClouds = topClouds;
                    proxyController.lightningGenerator = lightningGenerator;
                    proxyController.supernovaPlanetEffectController = supernovaPlanetEffect;
                    proxyController._realObjectDiameter = realSize;
                    proxyController.baseRealObjectDiameter = realSize;
                }

                return true;
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Exception thrown when generating proxy for [{body.Config.name}]:\n{ex}");
                return false;
            }
        }

        private static GameObject AddColouredSphere(GameObject rootObj, float size, TimeValuePair[] curve, Color color)
        {
            GameObject sphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereGO.transform.name = "ProxySphere";

            sphereGO.transform.parent = rootObj.transform;
            sphereGO.transform.localScale = Vector3.one * size;
            sphereGO.transform.position = rootObj.transform.position;

            UnityEngine.Object.Destroy(sphereGO.GetComponent<Collider>());

            sphereGO.GetComponent<MeshRenderer>().material.color = color;

            if (curve != null) AddSizeController(sphereGO, curve, size);

            return sphereGO;
        }

        private static SizeController AddSizeController(GameObject go, TimeValuePair[] curve, float size)
        {
            var sizeController = go.AddComponent<SizeController>();
            sizeController.SetScaleCurve(curve);
            sizeController.size = size;
            return sizeController;
        }
    }
}