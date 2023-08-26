using NewHorizons.Handlers;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Streaming;

internal static class VanillaStreamingFix
{
    internal static void UnparentSectorStreaming(Sector rootSector, AstroObject.Name streamingGroupName)
    {
        foreach (var component in rootSector.GetComponentsInChildren<Component>(true))
        {
            if (component is ISectorGroup sectorGroup)
            {
                sectorGroup.SetSector(rootSector);
            }

            if (component is SectoredMonoBehaviour behaviour)
            {
                behaviour.SetSector(rootSector);
            }
        }
        var sectorStreamingObj = new GameObject("Sector_Streaming");
        sectorStreamingObj.transform.SetParent(rootSector.transform, false);

        var sectorStreaming = sectorStreamingObj.AddComponent<SectorStreaming>();
        sectorStreaming._streamingGroup = StreamingHandler.GetStreamingGroup(streamingGroupName);
        sectorStreaming.SetSector(rootSector);
    }
}
