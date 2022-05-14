using NewHorizons.External.Configs;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewHorizons.Builder.StarSystem;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Object = UnityEngine.Object;
using NewHorizons.Components;

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

            if(system.Config.enableTimeLoop)
            {
                var timeLoopController = new GameObject("TimeLoopController");
                timeLoopController.AddComponent<TimeLoopController>();
            }
        }
    }
}
