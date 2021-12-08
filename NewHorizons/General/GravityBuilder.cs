using OWML.Utils;
using System.Reflection;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class GravityBuilder
    {
        public static GravityVolume Make(GameObject body, float surfaceAccel, float upperSurface, float lowerSurface)
        {
            GameObject gravityGO = new GameObject("GravityWell");
            gravityGO.transform.parent = body.transform;
            gravityGO.layer = 17;
            gravityGO.SetActive(false);

            GravityVolume GV = gravityGO.AddComponent<GravityVolume>();
            GV.SetValue("_cutoffAcceleration", 0.1f);
            GV.SetValue("_falloffType", GV.GetType().GetNestedType("FalloffType", BindingFlags.NonPublic).GetField("linear").GetValue(GV));
            GV.SetValue("_alignmentRadius", 1.5f * upperSurface);
            //Utility.AddDebugShape.AddSphere(gravityGO, 1.5f * upperSurface, new Color32(255, 0, 0, 128));
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
            SC.radius = 4 * upperSurface;

            OWCollider OWC = gravityGO.AddComponent<OWCollider>();
            OWC.SetLODActivationMask(DynamicOccupant.Player);

            gravityGO.SetActive(true);

            Logger.Log("Finished building gravity", Logger.LogType.Log);
            return GV;
        }
    }
}
