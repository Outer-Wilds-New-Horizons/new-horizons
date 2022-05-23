#region

using HarmonyLib;
using UnityEngine;

#endregion

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class EyeOfTheUniversePatches
    {
        // Funny eye of the universe stuff
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DeathManager), nameof(DeathManager.KillPlayer))]
        public static bool DeathManager_KillPlayer()
        {
            return Main.Instance.CurrentStarSystem != "EyeOfTheUniverse";
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipThrusterController), nameof(ShipThrusterController.ReadTranslationalInput))]
        public static bool ShipThrusterController_ReadTranslationalInput(ShipThrusterController __instance,
            ref Vector3 __result)
        {
            if (Main.Instance.CurrentStarSystem != "EyeOfTheUniverse") return true;

            var value = OWInput.GetValue(InputLibrary.thrustX);
            var value2 = OWInput.GetValue(InputLibrary.thrustZ);
            var value3 = OWInput.GetValue(InputLibrary.thrustUp);
            var value4 = OWInput.GetValue(InputLibrary.thrustDown);
            if (!OWInput.IsInputMode(InputMode.ShipCockpit | InputMode.LandingCam))
            {
                __result = Vector3.zero;
                return false;
            }

            if (!__instance._shipResources.AreThrustersUsable())
            {
                __result = Vector3.zero;
                return false;
            }

            if (__instance._autopilot.IsFlyingToDestination())
            {
                __result = Vector3.zero;
                return false;
            }

            var vector = new Vector3(value, 0f, value2);
            if (vector.sqrMagnitude > 1f) vector.Normalize();
            vector.y = value3 - value4;
            if (__instance._requireIgnition && __instance._landingManager.IsLanded())
            {
                vector.x = 0f;
                vector.z = 0f;
                vector.y = Mathf.Clamp01(vector.y);
                if (!__instance._isIgniting && __instance._lastTranslationalInput.y <= 0f && vector.y > 0f)
                {
                    __instance._isIgniting = true;
                    __instance._ignitionTime = Time.time;
                    GlobalMessenger.FireEvent("StartShipIgnition");
                }

                if (__instance._isIgniting)
                {
                    if (vector.y <= 0f)
                    {
                        __instance._isIgniting = false;
                        GlobalMessenger.FireEvent("CancelShipIgnition");
                    }

                    if (Time.time < __instance._ignitionTime + __instance._ignitionDuration)
                    {
                        vector.y = 0f;
                    }
                    else
                    {
                        __instance._isIgniting = false;
                        __instance._requireIgnition = false;
                        GlobalMessenger.FireEvent("CompleteShipIgnition");
                        RumbleManager.PlayShipIgnition();
                    }
                }
            }

            var d = __instance._thrusterModel.GetMaxTranslationalThrust() /
                    __instance._thrusterModel.GetMaxTranslationalThrust();
            var vector2 = vector * d;
            if (__instance._limitOrbitSpeed && vector2.magnitude > 0f)
            {
                var vector3 = __instance._landingRF.GetOWRigidBody().GetWorldCenterOfMass() -
                              __instance._shipBody.GetWorldCenterOfMass();
                var vector4 = __instance._shipBody.GetVelocity() - __instance._landingRF.GetVelocity();
                var vector5 = vector4 - Vector3.Project(vector4, vector3);
                var vector6 = Quaternion.FromToRotation(-__instance._shipBody.transform.up, vector3) *
                              __instance._shipBody.transform.TransformDirection(vector2 *
                                  __instance._thrusterModel.GetMaxTranslationalThrust());
                var vector7 = Vector3.Project(vector6, vector3);
                var vector8 = vector6 - vector7;
                var a = vector5 + vector8 * Time.deltaTime;
                var magnitude = a.magnitude;
                var orbitSpeed = __instance._landingRF.GetOrbitSpeed(vector3.magnitude);
                if (magnitude > orbitSpeed)
                {
                    a = a.normalized * orbitSpeed;
                    vector8 = (a - vector5) / Time.deltaTime;
                    vector6 = vector7 + vector8;
                    vector2 = __instance._shipBody.transform.InverseTransformDirection(vector6 /
                        __instance._thrusterModel.GetMaxTranslationalThrust());
                }
            }

            __instance._lastTranslationalInput = vector;
            __result = vector2;

            return false;
        }
    }
}