using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General;

public static class GroupBuilder
{
    /// <summary>
    /// puts groups on objects.
    /// run this before the gameobject is active.
    /// </summary>
    public static void Make(GameObject go, Sector sector)
    {
        if (!sector)
        {
            Logger.LogWarning($"tried to put groups on {go.name} when sector is null");
            return;
        }
        if (go.activeInHierarchy)
        {
            Logger.LogWarning($"tried to put groups on an active gameobject {go.name}");
            return;
        }

        go.GetAddComponent<SectorCullGroup>()._sector = sector;
        go.GetAddComponent<SectorCollisionGroup>()._sector = sector;
        go.GetAddComponent<SectorLightsCullGroup>()._sector = sector;
    }
}