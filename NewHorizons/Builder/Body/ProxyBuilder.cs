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
            // TODO: Make it not do it on vanilla bodies 
            var newModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(newModel.GetComponent<Collider>());
            var newProxy = new GameObject(proxyName);
            newModel.transform.SetParent(newProxy.transform);
            newModel.transform.position = Vector3.zero;
            newModel.transform.localScale = 1000 * Vector3.one;
            newModel.transform.rotation = Quaternion.identity;
            var proxyController = newProxy.AddComponent<NHProxy>();
            proxyController.astroName = body.Config.Name;
            proxyController._realObjectDiameter = body.Config.Base?.SurfaceSize ?? 100f;
            proxyController.renderer = newProxy.GetComponentInChildren<Renderer>();
        }

        private static void MakeHeightMapProxy(NewHorizonsBody body, IModBehaviour mod)
        {
            
        }
    }
}