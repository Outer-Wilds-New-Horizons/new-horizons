using HarmonyLib;
using NewHorizons.Utility.Files;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NewHorizons.Handlers.TitleScreen
{
    [HarmonyPatch]
    public class TitleScreenColourHandler
    {
        public static void SetColour(Color colour)
        {
            var buttons = GameObject.FindObjectOfType<TitleScreenManager>()._mainMenu.GetComponentsInChildren<Text>();
            var footer = GameObject.Find("TitleMenu/TitleCanvas/FooterBlock").GetComponentsInChildren<Text>();
            foreach (var button in buttons.Concat(footer))
            {
                button.color = colour;
            }
            _mainMenuColour = colour;
            var logo = ImageUtilities.TintImage(ImageUtilities.GetTexture(Main.Instance, "Assets\\textures\\MENU_OuterWildsLogo_d.png"), (Color)_mainMenuColour);
            GameObject.FindObjectOfType<TitleAnimRenderer>()._logoMaterial.mainTexture = logo;
            GameObject.FindObjectOfType<TitleAnimRenderer>()._logoMaterialClone.mainTexture = logo;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStyleApplier), nameof(UIStyleApplier.ChangeColors))]
        public static bool UIStyleApplier_ChangeColors(UIStyleApplier __instance, UIElementState state)
        {
            if (SceneManager.GetActiveScene().name == "TitleScreen" && _mainMenuColour is Color colour && __instance.transform.parent.name == "MainMenuLayoutGroup")
            {
                // Wyrm didn't say to account for any of these states I win!
                switch (state)
                {
                    case UIElementState.INTERMEDIATELY_HIGHLIGHTED:
                    case UIElementState.HIGHLIGHTED:
                    case UIElementState.PRESSED:
                    case UIElementState.ROLLOVER_HIGHLIGHT:
                        Color.RGBToHSV(colour, out var h, out var s, out var v);
                        colour = Color.HSVToRGB(h, s * 0.2f, v * 1.2f);
                        break;
                    case UIElementState.DISABLED:
                        return true;
                    default:
                        break;
                }

                for (int i = 0; i < __instance._foregroundGraphics.Length; i++)
                {
                    __instance._foregroundGraphics[i].color = colour;
                }
                for (int j = 0; j < __instance._backgroundGraphics.Length; j++)
                {
                    __instance._backgroundGraphics[j].color = colour;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private static Color? _mainMenuColour;
    }
}
