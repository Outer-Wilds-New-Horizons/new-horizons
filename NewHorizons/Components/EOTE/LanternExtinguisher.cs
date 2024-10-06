using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.EOTE;

public class LanternExtinguisher : MonoBehaviour
{
    public void Update()
    {
        if (PlayerState.InDreamWorld() && PlayerState.IsCameraUnderwater())
        {
            var heldItem = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
            if (heldItem is DreamLanternItem lantern && lantern._lanternController._lit)
            {
                Locator.GetDreamWorldController().ExitDreamWorld(DreamWakeType.LanternSubmerged);
            }
        }
    }
}
