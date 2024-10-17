using HarmonyLib;
using NewHorizons.Components.EOTE;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(LanternZoomPoint))]
    public static class LanternZoomPointPatches
    {
        // Patching all methods that assume the player is holding an artifact (_playerLantern) to add null checks so they'll work outside of the dream world

        [HarmonyPrefix]
        [HarmonyPatch(nameof(LanternZoomPoint.Update))]
        public static bool LanternZoomPoint_Update(LanternZoomPoint __instance)
        {
            if (PlayerState.InDreamWorld()) return true;
            if (!__instance.enabled)
            {
                return false;
            }
            if (__instance._state != LanternZoomPoint.State.RetroZoom)
            {
                if (__instance._playerLantern != null)
                {
                    __instance._playerLantern.GetLanternController().MoveTowardFocus(1f, 2f);
                }
            }
            if (__instance._state == LanternZoomPoint.State.LookAt && Time.time > __instance._stateChangeTime + 0.4f)
            {
                __instance.ChangeState(LanternZoomPoint.State.ZoomIn);
                __instance.StartZoomIn();
            }
            else if (__instance._state == LanternZoomPoint.State.ZoomIn)
            {
                __instance.UpdateZoomIn();
            }
            if (__instance._state == LanternZoomPoint.State.RetroZoom)
            {
                __instance.UpdateRetroZoom();
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(LanternZoomPoint.UpdateRetroZoom))]
        public static bool LanternZoomPoint_UpdateRetroZoom(LanternZoomPoint __instance)
        {
            if (PlayerState.InDreamWorld()) return true;
            float num = Mathf.InverseLerp(__instance._stateChangeTime, __instance._stateChangeTime + 1.2f, Time.time);
            float focus = Mathf.Pow(Mathf.SmoothStep(0f, 1f, 1f - num), 0.2f);
            if (__instance._playerLantern != null)
            {
                __instance._playerLantern.GetLanternController().SetFocus(focus);
            }
            float t = __instance._retroZoomCurve.Evaluate(num);
            float targetFieldOfView = Mathf.Lerp(__instance._startFOV, Locator.GetPlayerCameraController().GetOrigFieldOfView(), t);
            Locator.GetPlayerCameraController().SetTargetFieldOfView(targetFieldOfView);
            float d = __instance._imageHalfWidth / Mathf.Tan(Locator.GetPlayerCamera().fieldOfView * 0.017453292f * 0.5f);
            Vector3 vector = __instance._startLocalPos - __instance._endLocalPos;
            __instance._attachPoint.transform.localPosition = __instance._endLocalPos + vector.normalized * d;
            if (num >= 1f)
            {
                __instance.FinishRetroZoom();
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(LanternZoomPoint.FinishRetroZoom))]
        public static bool LanternZoomPoint_FinishRetroZoom(LanternZoomPoint __instance)
        {
            if (PlayerState.InDreamWorld()) return true;
            __instance.ChangeState(LanternZoomPoint.State.Idle);
            __instance.enabled = false;
            __instance._attachPoint.DetachPlayer();
            GlobalMessenger.FireEvent("PlayerRepositioned");
            if (__instance._playerLantern != null)
            {
                __instance._playerLantern.ForceUnfocus();
                __instance._playerLantern.enabled = true;
                __instance._playerLantern = null;
            }
            OWInput.ChangeInputMode(InputMode.Character);
            __instance._lightController.FadeTo(0f, 1f);
            Locator.GetPlayerController().SetColliderActivation(true);
            Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().BreakLock();
            Locator.GetDreamWorldController().SetActiveZoomPoint(null);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(LanternZoomPoint.OnDetectLight))]
        public static bool LanternZoomPoint_OnDetectLight(LanternZoomPoint __instance)
        {
            if (PlayerState.InDreamWorld()) return true;
            if (__instance._state == LanternZoomPoint.State.Idle && !PlayerState.IsAttached() && Time.time > __instance._stateChangeTime + 1f && Vector3.Distance(__instance.transform.position, Locator.GetPlayerCamera().transform.position) > __instance._minActivationDistance)
            {
                __instance._playerLantern = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() as DreamLanternItem;
                Locator.GetDreamWorldController().SetActiveZoomPoint(__instance);
                __instance._attachPoint.transform.position = Locator.GetPlayerTransform().position;
                __instance._attachPoint.transform.rotation = Locator.GetPlayerTransform().rotation;
                __instance._attachPoint.AttachPlayer();
                Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(__instance.transform, 5f, false, 1f);
                OWInput.ChangeInputMode(InputMode.None);
                if (__instance._playerLantern != null)
                {
                    __instance._playerLantern.enabled = false;
                }
                __instance.ChangeState(LanternZoomPoint.State.LookAt);
                __instance.enabled = true;
            }
            return false;
        }
    }
}
