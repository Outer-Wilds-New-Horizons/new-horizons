using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NewHorizons.Utility.DebugUtilities
{
    class DebugNomaiTextPlacer : MonoBehaviour
    {
        public static bool active;
        public Action<DebugRaycastData> onRaycast;

        private DebugRaycaster _rc;

        private ScreenPrompt _placePrompt;

        private void Awake()
        {
            _rc = this.GetComponent<DebugRaycaster>();

            _placePrompt = new ScreenPrompt(TranslationHandler.GetTranslation("DEBUG_PLACE_TEXT", TranslationHandler.TextType.UI) + " <CMD>", ImageUtilities.GetButtonSprite(KeyCode.G));
            Locator.GetPromptManager().AddScreenPrompt(_placePrompt, PromptPosition.UpperRight, false);
        }

        private void OnDestroy()
        {
            Locator.GetPromptManager()?.RemoveScreenPrompt(_placePrompt, PromptPosition.UpperRight);
        }

        private void Update()
        {
            UpdatePromptVisibility();
            if (!Main.Debug) return;
            if (!active) return;

            if (Keyboard.current[Key.G].wasReleasedThisFrame)
            {
                DebugRaycastData data = _rc.Raycast();
                if (onRaycast != null) onRaycast.Invoke(data);
            }
        }

        public void UpdatePromptVisibility()
        {
            _placePrompt.SetVisibility(!OWTime.IsPaused() && Main.Debug && active);
        }
    }
}
