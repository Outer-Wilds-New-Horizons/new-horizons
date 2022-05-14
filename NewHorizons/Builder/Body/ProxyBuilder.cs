using NewHorizons.Builder.Atmosphere;
using NewHorizons.Components;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public class ProxyBuilder
    {
        public static void Make(GameObject gameObject, NewHorizonsBody body)
        {
            var proxyName = $"{body.Config.Name}_Proxy";

            // Stars don't work yet so uh
            /*
            if (body.Config.Star != null)
            {
                var sunProxy = SearchUtilities.CachedFind("SunProxy(Clone)");
                var oldProxyEffectController = sunProxy.GetComponentInChildren<SunProxyEffectController>();
                var newProxy = Object.Instantiate(sunProxy, gameObject.transform.position, Quaternion.identity);
                newProxy.name = proxyName;
                var proxyController = newProxy.GetComponent<SunProxy>();
                proxyController._proxySunController = newProxy.GetComponentInChildren<SunProxyEffectController>();
                proxyController._proxySunController._atmosphereMaterial = oldProxyEffectController._atmosphereMaterial;
                proxyController._proxySunController._fogMaterial = oldProxyEffectController._fogMaterial;
                Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
                {
                    proxyController._sunTransform = gameObject.transform;
                    proxyController._realSunController = gameObject.GetComponent<SunController>();
                });
            }
            */

            var newProxy = new GameObject(proxyName);

            // We want to take the largest size I think
            var realSize = body.Config.Base.SurfaceSize;

            if (body.Config.HeightMap != null)
            {
                HeightMapBuilder.Make(newProxy, null, body.Config.HeightMap, body.Mod, 20);
                if(realSize < body.Config.HeightMap.MaxHeight) realSize = body.Config.HeightMap.MaxHeight;
            }
            if (body.Config.Base.GroundSize != 0)
            {
                GeometryBuilder.Make(newProxy, null, body.Config.Base.GroundSize);
                if (realSize < body.Config.Base.GroundSize) realSize = body.Config.Base.GroundSize;
            }
            if (body.Config.Atmosphere != null)
            {
                CloudsBuilder.MakeTopClouds(newProxy, body.Config.Atmosphere, body.Mod);
                if (realSize < body.Config.Atmosphere.Size) realSize = body.Config.Atmosphere.Size;
            }
            if (body.Config.Ring != null)
            {
                RingBuilder.MakeRingGraphics(newProxy, null, body.Config.Ring, body.Mod);
                if (realSize < body.Config.Ring.OuterRadius) realSize = body.Config.Ring.OuterRadius;
            }
            if (body.Config.Star != null)
            {
                StarBuilder.MakeStarGraphics(newProxy, null, body.Config.Star);
                if (realSize < body.Config.Star.Size) realSize = body.Config.Star.Size;
            }
            if(body.Config.ProcGen != null)
            {
                ProcGenBuilder.Make(newProxy, null, body.Config.ProcGen);
                if (realSize < body.Config.ProcGen.Scale) realSize = body.Config.ProcGen.Scale;
            }

            // Remove all collisions if there are any
            foreach (var col in newProxy.GetComponentsInChildren<Collider>())
            {
                GameObject.Destroy(col);
            }
            // Fix render idk
            foreach (var renderer in newProxy.GetComponentsInChildren<Renderer>())
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            }

            var proxyController = newProxy.AddComponent<NHProxy>();
            proxyController.astroName = body.Config.Name;
            proxyController._realObjectDiameter = realSize;
            proxyController.renderers = newProxy.GetComponentsInChildren<Renderer>();
            proxyController.tessellatedRenderers = newProxy.GetComponentsInChildren<TessellatedRenderer>();
        }
    }
}