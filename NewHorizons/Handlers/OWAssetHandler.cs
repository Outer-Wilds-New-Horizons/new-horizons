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
        public static void Init()
        {

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
                    foreach (var table in tables)
                    {
                        foreach(var x in table._materialPropertyLookups)
                        {
                            if(materials.Contains(x.material))
                            {
                                assetBundles.SafeAdd(table.assetBundle);
                            }
                        }                           
                    }
                }
            }

            foreach (var assetBundle in assetBundles)
            {
                Logger.Log($"Loading {assetBundles.Count} : {assetBundle}");
                StreamingManager.LoadStreamingAssets(assetBundle);
            }
        }
    }
}
