using HarmonyLib;
using OWML.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Patches;

/// <summary>
/// From QSB
/// 
/// By delaying rigidbody stuff here we make copying objects with orbs work properly
/// Should also improve rafts
/// </summary>
[HarmonyPatch(typeof(OWRigidbody))]
public static class OWRigidbodyPatches
{
    private static readonly Dictionary<OWRigidbody, Transform> _setParentQueue = new();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(OWRigidbody.Awake))]
    private static bool Awake(OWRigidbody __instance)
    {
        if (Main.Instance.ModHelper.Interaction.ModExists("Raicuparta.QuantumSpaceBuddies"))
        {
            // QSB already does all this, so don't run it again if it's installed.
            return true;
        }

        __instance._transform = __instance.transform;

        if (!__instance._scaleRoot)
        {
            __instance._scaleRoot = __instance._transform;
        }

        CenterOfTheUniverse.TrackRigidbody(__instance);
        __instance._offsetApplier = __instance.gameObject.GetAddComponent<CenterOfTheUniverseOffsetApplier>();
        __instance._offsetApplier.Init(__instance);
        if (__instance._simulateInSector)
        {
            __instance._simulateInSector.OnSectorOccupantsUpdated += __instance.OnSectorOccupantsUpdated;
        }

        __instance._origParent = __instance._transform.parent;
        __instance._origParentBody = __instance._origParent ? __instance._origParent.GetAttachedOWRigidbody() : null;
        if (__instance._transform.parent)
        {
            _setParentQueue[__instance] = null;
        }

        __instance._rigidbody = __instance.GetRequiredComponent<Rigidbody>();
        __instance._rigidbody.interpolation = RigidbodyInterpolation.None;
        if (!__instance._autoGenerateCenterOfMass)
        {
            __instance._rigidbody.centerOfMass = __instance._centerOfMass;
        }

        if (__instance.IsSimulatedKinematic())
        {
            __instance.EnableKinematicSimulation();
        }

        __instance._origCenterOfMass = __instance.RunningKinematicSimulation() ? __instance._kinematicRigidbody.centerOfMass : __instance._rigidbody.centerOfMass;
        __instance._referenceFrame = new ReferenceFrame(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(OWRigidbody.Start))]
    private static void Start(OWRigidbody __instance)
    {
        if (Main.Instance.ModHelper.Interaction.ModExists("Raicuparta.QuantumSpaceBuddies"))
        {
            // QSB already does all this, so don't run it again if it's installed.
            return;
        }

        if (_setParentQueue.TryGetValue(__instance, out var parent))
        {
            __instance._transform.parent = parent;
            _setParentQueue.Remove(__instance);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(OWRigidbody.OnDestroy))]
    private static void OnDestroy(OWRigidbody __instance)
    {
        if (Main.Instance.ModHelper.Interaction.ModExists("Raicuparta.QuantumSpaceBuddies"))
        {
            // QSB already does all this, so don't run it again if it's installed.
            return;
        }

        _setParentQueue.Remove(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(OWRigidbody.Suspend), typeof(Transform), typeof(OWRigidbody))]
    private static bool Suspend(OWRigidbody __instance, Transform suspensionParent, OWRigidbody suspensionBody)
    {
        if (Main.Instance.ModHelper.Interaction.ModExists("Raicuparta.QuantumSpaceBuddies"))
        {
            // QSB already does all this, so don't run it again if it's installed.
            return true;
        }

        if (!__instance._suspended || __instance._unsuspendNextUpdate)
        {
            __instance._suspensionBody = suspensionBody;
            var direction = __instance.GetVelocity() - suspensionBody.GetPointVelocity(__instance._transform.position);
            __instance._cachedRelativeVelocity = suspensionBody.transform.InverseTransformDirection(direction);
            __instance._cachedAngularVelocity = __instance.RunningKinematicSimulation() ? __instance._kinematicRigidbody.angularVelocity : __instance._rigidbody.angularVelocity;
            __instance.enabled = false;
            __instance._offsetApplier.enabled = false;
            if (__instance.RunningKinematicSimulation())
            {
                __instance._kinematicRigidbody.enabled = false;
            }
            else
            {
                __instance.MakeKinematic();
            }

            if (_setParentQueue.ContainsKey(__instance))
            {
                _setParentQueue[__instance] = suspensionParent;
            }
            else
            {
                __instance._transform.parent = suspensionParent;
            }

            __instance._suspended = true;
            __instance._unsuspendNextUpdate = false;
            if (!Physics.autoSyncTransforms)
            {
                Physics.SyncTransforms();
            }

            if (__instance._childColliders == null)
            {
                __instance._childColliders = __instance.GetComponentsInChildren<Collider>();
                foreach (var childCollider in __instance._childColliders)
                {
                    childCollider.gameObject.GetAddComponent<OWCollider>().ListenForParentBodySuspension();
                }
            }

            __instance.RaiseEvent(nameof(__instance.OnSuspendOWRigidbody), __instance);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(OWRigidbody.ChangeSuspensionBody))]
    private static bool ChangeSuspensionBody(OWRigidbody __instance, OWRigidbody newSuspensionBody)
    {
        if (Main.Instance.ModHelper.Interaction.ModExists("Raicuparta.QuantumSpaceBuddies"))
        {
            // QSB already does all this, so don't run it again if it's installed.
            return true;
        }

        if (__instance._suspended)
        {
            __instance._cachedRelativeVelocity = Vector3.zero;
            __instance._suspensionBody = newSuspensionBody;
            if (_setParentQueue.ContainsKey(__instance))
            {
                _setParentQueue[__instance] = newSuspensionBody.transform;
            }
            else
            {
                __instance._transform.parent = newSuspensionBody.transform;
            }
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(OWRigidbody.UnsuspendImmediate))]
    private static bool UnsuspendImmediate(OWRigidbody __instance, bool restoreCachedVelocity)
    {
        if (Main.Instance.ModHelper.Interaction.ModExists("Raicuparta.QuantumSpaceBuddies"))
        {
            // QSB already does all this, so don't run it again if it's installed.
            return true;
        }

        if (__instance._suspended)
        {
            if (__instance.RunningKinematicSimulation())
            {
                __instance._kinematicRigidbody.enabled = true;
            }
            else
            {
                __instance.MakeNonKinematic();
            }

            __instance.enabled = true;
            if (_setParentQueue.ContainsKey(__instance))
            {
                _setParentQueue[__instance] = null;
            }
            else
            {
                __instance._transform.parent = null;
            }

            if (!Physics.autoSyncTransforms)
            {
                Physics.SyncTransforms();
            }

            var cachedVelocity = restoreCachedVelocity ? __instance._suspensionBody.transform.TransformDirection(__instance._cachedRelativeVelocity) : Vector3.zero;
            __instance.SetVelocity(__instance._suspensionBody.GetPointVelocity(__instance._transform.position) + cachedVelocity);
            __instance.SetAngularVelocity(restoreCachedVelocity ? __instance._cachedAngularVelocity : Vector3.zero);
            __instance._suspended = false;
            __instance._suspensionBody = null;
            __instance.RaiseEvent(nameof(__instance.OnUnsuspendOWRigidbody), __instance);
        }

        return false;
    }
}
