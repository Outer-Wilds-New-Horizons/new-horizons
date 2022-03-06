using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Handlers
{
    public static class OWAssetHandler
    {
        private static Dictionary<Material, string> _materialCache;

        public static void Init()
        {
            _materialCache = new Dictionary<Material, string>();
        }

        public static void LoadObject(GameObject obj)
        {
            var assetBundles = new List<string>();

            var tables = Resources.FindObjectsOfTypeAll<StreamingMaterialTable>();
            foreach (var streamingHandle in obj.GetComponentsInChildren<StreamingMeshHandle>())
            {
                var assetBundle = streamingHandle.assetBundle;
                if (!assetBundles.Contains(assetBundle))
                {
                    assetBundles.Add(assetBundle);
                }
                if (streamingHandle is StreamingRenderMeshHandle || streamingHandle is StreamingSkinnedMeshHandle)
                {
                    var materials = streamingHandle.GetComponent<Renderer>().sharedMaterials;
                    
                    if (materials.Length == 0) continue;

                    if (_materialCache == null) _materialCache = new Dictionary<Material, string>();

                    // Gonna assume that if theres more than one material its probably in the same asset bundle anyway right
                    if (_materialCache.TryGetValue(materials[0], out assetBundle))
                    {
                        assetBundles.Add(assetBundle);
                    }
                    else
                    {
                        foreach (var table in tables)
                        {
                            foreach (var x in table._materialPropertyLookups)
                            {
                                if (materials.Contains(x.material))
                                {
                                    _materialCache.SafeAdd(x.material, table.assetBundle);
                                    assetBundles.SafeAdd(table.assetBundle);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var assetBundle in assetBundles)
            {
                StreamingManager.LoadStreamingAssets(assetBundle);
            }
        }
    }
}
