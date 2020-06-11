using OWML.ModHelper.Events;
using System.Reflection;
using UnityEngine;

namespace Marshmallow.General
{
    static class MakeGravityWell
    {
        public static GravityVolume Make(GameObject body, float surfaceAccel, float upperSurface, float lowerSurface)
        {
            GameObject gravityGO = new GameObject();
            gravityGO.transform.parent = body.transform;
            gravityGO.name = "GravityWell";
            gravityGO.layer = 17;
            gravityGO.SetActive(false);

            GravityVolume GV = gravityGO.AddComponent<GravityVolume>();
            GV.SetValue("_cutoffAcceleration", 0.1f);
            GV.SetValue("_falloffType", GV.GetType().GetNestedType("FalloffType", BindingFlags.NonPublic).GetField("linear").GetValue(GV));
            GV.SetValue("_alignmentRadius", 600f);
            GV.SetValue("_upperSurfaceRadius", upperSurface);
            GV.SetValue("_lowerSurfaceRadius", lowerSurface);
            GV.SetValue("_layer", 3);
            GV.SetValue("_priority", 0);
            GV.SetValue("_alignmentPriority", 0);
            GV.SetValue("_surfaceAcceleration", surfaceAccel);
            GV.SetValue("_inheritable", false);
            GV.SetValue("_isPlanetGravityVolume", true);
            GV.SetValue("_cutoffRadius", 55f);

            SphereCollider SC = gravityGO.AddComponent<SphereCollider>();
            SC.isTrigger = true;
            SC.radius = 4000;

            OWCollider OWC = gravityGO.AddComponent<OWCollider>();
            OWC.SetLODActivationMask(DynamicOccupant.Player);

            gravityGO.SetActive(true);

            return GV;
        }
    }
}
