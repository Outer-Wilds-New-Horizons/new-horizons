using NewHorizons.Handlers;
using UnityEngine;

namespace NewHorizons.Streaming;

internal static class VanillaStreamingFix
{
    internal static void UnparentSectorStreaming(Sector rootSector, AstroObject.Name streamingGroupName)
        => UnparentSectorStreaming(rootSector, rootSector.gameObject, streamingGroupName, Sector.Name.Unnamed);

    internal static void UnparentSectorStreaming(Sector rootSector, GameObject startingObject, AstroObject.Name streamingGroupName, Sector.Name originalParentSectorName)
    {
        // Set originalParentSectorName to unnamed to alter all sectors
        foreach (var component in startingObject.GetComponentsInChildren<Component>(true))
        {
            if (component is ISectorGroup sectorGroup)
            {
                if (sectorGroup.GetSector()?.GetName() == originalParentSectorName || originalParentSectorName == Sector.Name.Unnamed)
                {
                    sectorGroup.SetSector(rootSector);
                }
            }

            if (component is SectoredMonoBehaviour behaviour)
            {
                if (behaviour.GetSector()?.GetName() == originalParentSectorName || originalParentSectorName == Sector.Name.Unnamed)
                {
                    behaviour.SetSector(rootSector);
                }
            }
        }
        var sectorStreamingObj = new GameObject("Sector_Streaming");
        sectorStreamingObj.transform.SetParent(startingObject.transform, false);

        var sectorStreaming = sectorStreamingObj.AddComponent<SectorStreaming>();
        sectorStreaming._streamingGroup = StreamingHandler.GetStreamingGroup(streamingGroupName);
        sectorStreaming.SetSector(rootSector);
    }
}
