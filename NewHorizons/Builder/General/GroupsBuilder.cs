using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Builder.General;

public static class GroupsBuilder
{
    /// <summary>
    /// puts groups on an object, activated by sector.
    /// run this before the gameobject is active.
    /// </summary>
    public static void Make(GameObject go, Sector sector)
    {
        if (!sector)
        {
            NHLogger.LogWarning($"tried to put groups on {go.name} when sector is null");
            return;
        }
        if (go.activeInHierarchy)
        {
            NHLogger.LogWarning($"tried to put groups on an active gameobject {go.name}");
            return;
        }

        go.GetAddComponent<SectorCullGroup>()._sector = sector;
        go.GetAddComponent<SectorCollisionGroup>()._sector = sector;
        go.GetAddComponent<SectorLightsCullGroup>()._sector = sector;

        // SectorCollisionGroup is unique among the sector groups because it only attaches its event listener on Start() instead of Awake(), so if the detail gets immediately deactivated then it never gets attached. To avoid this, we'll attach the event listener manually (even if it means getting attached twice).
        sector.OnSectorOccupantsUpdated += go.GetComponent<SectorCollisionGroup>().OnSectorOccupantsUpdated;
    }
}