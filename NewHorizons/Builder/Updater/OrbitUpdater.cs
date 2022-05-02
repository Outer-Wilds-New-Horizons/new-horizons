using NewHorizons.Builder.Orbital;
using NewHorizons.Utility;
using NewHorizons.Utility.CommonResources;
using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Game.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Updater
{
    public static class OrbitUpdater
    {
        public static void Update(NewHorizonsBody body, GameObject go)
        {
            var mapping = Planet.defaultMapping;
            var heavenlyBody = CommonResourcesUtilities.HeavenlyBodyFromAstroObject(go.GetComponent<AstroObject>());

            Logger.Log($"Updating position of {body.Config.Name}/{heavenlyBody}");

            if (heavenlyBody != HeavenlyBody.None)
            {
                var original = mapping[heavenlyBody];

                var coords = body.Config.Orbit.GetKeplerCoords();

                var parent = original.state.parent;
                if (body.Config.Orbit.PrimaryBody != null)
                {
                    var parentAO = AstroObjectLocator.GetAstroObject(body.Config.Orbit.PrimaryBody);
                    var newParent = CommonResourcesUtilities.HeavenlyBodyFromAstroObject(parentAO);
                    if (newParent != HeavenlyBody.None)
                    {
                        Logger.LogWarning($"Sorry, can't change primary body for planets yet. You tried making {body.Config.Name} orbit {newParent}");
                        /*
                        parent = newParent;
                        // Have to change the gravity stuff
                        go.GetComponentInChildren<ConstantForceDetector>()._detectableFields = new ForceVolume[] { parentAO.GetGravityVolume() };
                        go.GetComponent<AstroObject>()._primaryBody = parentAO;
                        */
                    }
                    else Logger.LogError($"Couldn't find new parent {body.Config.Orbit.PrimaryBody}");
                }

                var planetoid = new Planet.Plantoid(
                    original.size,
                    original.gravity,
                    mapping[heavenlyBody].state.orbit.orientation.rotation,
                    InitialMotionBuilder.SiderealPeriodToAngularSpeed(body.Config.Orbit.SiderealPeriod),
                    parent,
                    coords
                );

                mapping[heavenlyBody] = planetoid;

                Planet.defaultMapping = mapping;
                Planet.mapping = mapping;
            }
            else
            {
                Logger.LogError($"Couldn't find heavenlyBody for {body.Config.Name}");
            }
        }
    }
}
