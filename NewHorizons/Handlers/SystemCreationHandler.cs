using NewHorizons.Builder.StarSystem;
using NewHorizons.Components;
using NewHorizons.Utility;
using UnityEngine;
using Object = UnityEngine.Object;
namespace NewHorizons.Handlers
{
    public static class SystemCreationHandler
    {
        public static void LoadSystem(NewHorizonsSystem system)
        {
            var skybox = GameObject.Find("Skybox/Starfield");

            if (system.Config.skybox?.destroyStarField ?? false)
            {
                Object.Destroy(skybox);
            }

            if (system.Config.skybox?.assetBundle != null && system.Config.skybox?.path != null)
            {
                SkyboxBuilder.Make(system.Config.skybox, system.Mod);
            }

            if (system.Config.enableTimeLoop)
            {
                var timeLoopController = new GameObject("TimeLoopController");
                timeLoopController.AddComponent<TimeLoopController>();
            }
        }
    }
}
