using NewHorizons.Components;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class ProxyHandler
    {
        private static List<NHProxy> _proxies = new();
        private static Dictionary<Transform, ProxyBody> _vanillaProxyBody = new();
        private static Dictionary<Transform, ProxyOrbiter> _vanillaProxyOrbiter = new();

        public static void ClearCache()
        {
            _proxies.Clear();
            _vanillaProxyBody.Clear();
            _vanillaProxyOrbiter.Clear();
        }

        public static NHProxy GetProxy(string astroName)
        {
            foreach (NHProxy proxy in _proxies)
            {
                if (proxy.astroName.Equals(astroName))
                    return proxy;
            }
            return null;
        }

        public static void RegisterVanillaProxyBody(ProxyBody proxy)
        {
            if (proxy.realObjectTransform != null)
            {
                _vanillaProxyBody.Add(proxy.realObjectTransform, proxy);
            }
        }

        public static ProxyBody GetVanillaProxyBody(Transform t)
        {
            if (_vanillaProxyBody.TryGetValue(t, out ProxyBody proxy))
            {
                return proxy;
            }
            else
            {
                return null;
            }
        }

        public static void RegisterVanillaProxyOrbiter(ProxyOrbiter proxy)
        {
            if (proxy._originalPlanetBody != null)
            {
                _vanillaProxyOrbiter.Add(proxy._originalPlanetBody, proxy);
            }
        }

        public static ProxyOrbiter GetVanillaProxyOrbiter(Transform t)
        {
            if (_vanillaProxyOrbiter.TryGetValue(t, out ProxyOrbiter proxy))
            {
                return proxy;
            }
            else
            {
                return null;
            }
        }

        public static void RegisterProxy(NHProxy proxy)
        {
            _proxies.SafeAdd(proxy);
        }

        public static void UnregisterProxy(NHProxy proxy)
        {
            _proxies.Remove(proxy);
        }
    }
}
