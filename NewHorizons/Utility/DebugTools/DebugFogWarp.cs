using UnityEngine;

namespace NewHorizons.Utility.DebugTools
{
    /// <summary>
    /// Adapted from Survivors https://github.com/Hawkbat/ow-mod-jam-2/blob/main/EscapePodFour.cs#L197
    /// </summary>
    [RequireComponent(typeof(SphericalFogWarpVolume))]
    public class DebugFogWarp : MonoBehaviour
    {
        public SphericalFogWarpVolume fogWarpVolume;
        public void OnGUI()
        {
            if (Main.Debug && Main.VisualizeBrambleVolumeNames && fogWarpVolume != null)
            {
                DrawWorldLabel(fogWarpVolume, fogWarpVolume.name);
                if (fogWarpVolume._exits != null)
                {
                    foreach (var e in fogWarpVolume._exits)
                    {
                        if (e != null)
                        {
                            DrawWorldLabel(fogWarpVolume.GetExitPosition(e), e.name);
                        }
                    }
                }
            }
        }

        public void DrawWorldLabel(Component component, string text)
        {
            DrawWorldLabel(component.transform.position, text);
        }

        public void DrawWorldLabel(Vector3 worldPos, string text)
        {
            var c = Locator.GetPlayerCamera();
            var d = Vector3.Distance(c.transform.position, worldPos);
            if (d > 1000f) return;
            GUI.Label(new Rect(WorldToGui(worldPos), new Vector2(500f, 20f)), text);
        }

        public Vector2 WorldToGui(Vector3 wp)
        {
            var c = Locator.GetPlayerCamera();
            var sp = c.WorldToScreenPoint(wp);
            if (sp.z < 0) return new Vector2(Screen.width, Screen.height);
            var gp = new Vector2(sp.x, Screen.height - sp.y);
            return gp;
        }
    }
}
