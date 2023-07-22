using NewHorizons.Components;
using System.Collections.Generic;

namespace NewHorizons.Handlers
{
    public static class ProxyHandler
    {
        private static List<NHProxy> _proxies = new List<NHProxy>();

        public static NHProxy GetProxy(string astroName)
        {
            foreach (NHProxy proxy in _proxies)
            {
                if (proxy.astroName.Equals(astroName))
                    return proxy;
            }
            return null;
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
