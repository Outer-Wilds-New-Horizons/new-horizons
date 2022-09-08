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
        Logger.LogVerbose($"putting groups on {go} (linked to {sector})");
        if (!sector)
        {
            Logger.LogWarning("tried to put groups on a null sector");
            return;
        }
        if (go.activeInHierarchy)
        {
            Logger.LogWarning("tried to put groups on an active gameobject");
            return;
        }

        go.GetAddComponent<SectorCullGroup>()._sector = sector;
        go.GetAddComponent<SectorCollisionGroup>()._sector = sector;
        go.GetAddComponent<SectorLightsCullGroup>()._sector = sector;
    }
}