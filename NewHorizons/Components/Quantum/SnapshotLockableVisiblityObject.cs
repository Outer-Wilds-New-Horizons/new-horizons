using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components.Quantum;

/// <summary>
/// A quantum object that does nothing but track if its been photographed
/// </summary>
internal class SnapshotLockableVisibilityObject : SocketedQuantumObject
{
    public GameObject emptySocketObject;

    public override bool ChangeQuantumState(bool skipInstantVisibilityCheck) => true;
}


[HarmonyPatch(typeof(SocketedQuantumObject))]
public static class SocketedQuantumObjectPatches
{
    private static bool _skipPatch;
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SocketedQuantumObject), nameof(SocketedQuantumObject.MoveToSocket))]
    private static bool SocketedQuantumObject_MoveToSocket(SocketedQuantumObject __instance, QuantumSocket socket)
    {
        // Double check this is a normal allowed move, then do rotation stuff
        if (socket.GetVisibilityObject() is SnapshotLockableVisibilityObject qObj1 && (!qObj1.IsLocked() || _skipPatch))
        {
            if (qObj1._gravityVolume != null && qObj1._alignWithGravity)
            {
                Vector3 toDirection = qObj1.emptySocketObject.transform.position - qObj1._gravityVolume.GetCenterPosition();
                Quaternion lhs = Quaternion.FromToRotation(qObj1.emptySocketObject.transform.up, toDirection);
                qObj1.emptySocketObject.transform.rotation = lhs * qObj1.emptySocketObject.transform.rotation;
            }
            if (qObj1._randomYRotation)
            {
                float d = Random.Range(0f, 360f);
                qObj1.emptySocketObject.transform.localRotation *= Quaternion.Euler(Vector3.up * d);
            }
        }

        if (_skipPatch)
        {
            return true;
        }

        if (socket.GetVisibilityObject() is SnapshotLockableVisibilityObject qObj)
        {
            // Do not allow it to switch to a socket that is photographed
            // Instead try to swap with an occupied socket
            if (qObj.IsLockedByProbeSnapshot())
            {
                _skipPatch = true;
                // Try to swap
                var swapSocket = __instance._socketList.FirstOrDefault(x => x._quantumObject != null && x._quantumObject != __instance && !x._quantumObject.IsLocked());
                if (swapSocket != null)
                {
                    var originalSocket = __instance.GetCurrentSocket();
                    var otherQObj = swapSocket._quantumObject as SocketedQuantumObject;
                    otherQObj.MoveToSocket(socket);
                    __instance.MoveToSocket(swapSocket);
                    otherQObj.MoveToSocket(originalSocket);
                }
                else
                {
                    __instance.MoveToSocket(__instance.GetCurrentSocket());
                }

                _skipPatch = false;
                return false;
            }
        }

        return true;
    }
}
