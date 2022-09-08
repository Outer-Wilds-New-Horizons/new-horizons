using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
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
                Logger.LogWarning($"tried to put groups on {go.name} when sector is null");
                return;
            }
            if (go.activeInHierarchy)
            {
                Logger.LogWarning($"tried to put groups on an active gameobject {go.name}");
                return;
            }
            Logger.LogVerbose($"putting groups on {go.name}");

            go.GetAddComponent<SectorCullGroup>()._sector = sector;
            go.GetAddComponent<SectorCollisionGroup>()._sector = sector;
            go.GetAddComponent<SectorLightsCullGroup>()._sector = sector;
        }
    }
}