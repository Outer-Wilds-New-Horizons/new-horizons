using NewHorizons.External.Configs;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.StarSystem
{
    public class SkyboxBuilder
    {
        public static void Make(StarSystemConfig.SkyboxConfig info, IModBehaviour mod)
        {
            Logger.Log("Building Skybox");
            var skyBoxMaterial = AssetBundleUtilities.Load<Material>(info.assetBundle, info.path, mod);
            RenderSettings.skybox = skyBoxMaterial;
            foreach (var camera in Resources.FindObjectsOfTypeAll<OWCamera>())
            {
                camera.clearFlags = CameraClearFlags.Skybox;
            }
        }
    }
}