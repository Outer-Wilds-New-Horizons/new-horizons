using NewHorizons.Components.EOTE;
using NewHorizons.OtherMods.VoiceActing;
using NewHorizons.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class CloakHandler
    {
        private static HashSet<CloakFieldController> _cloaks;
        public static HashSet<CloakFieldController> Cloaks => _cloaks;

        private static bool _flagStrangerDisabled;
        public static bool FlagStrangerDisabled
        {
            get => _flagStrangerDisabled;
            set
            {
                _flagStrangerDisabled = value;
                if (value && _strangerCloak != null)
                {
                    DeregisterCloak(_strangerCloak);
                }
            }
        }
        public static bool VisibleStrangerInstalled => Main.Instance.ModHelper.Interaction.ModExists("xen.Decloaked");

        private static CloakFieldController _strangerCloak;
        private static CloakLocatorController _cloakLocator;

        public static void Init()
        {
            _cloaks = new();
            FlagStrangerDisabled = false;
            _strangerCloak = null;
            _cloakLocator = null;
        }

        public static void OnSystemReady()
        {
            // If NH is disabling the stranger it will not be gone yet, however other mods might have gotten rid of it
            var stranger = Locator.GetAstroObject(AstroObject.Name.RingWorld)?.gameObject;
            if (stranger != null && stranger.activeInHierarchy && !FlagStrangerDisabled && !VisibleStrangerInstalled)
            {
                _strangerCloak = stranger.GetComponentInChildren<CloakFieldController>();
                RegisterCloak(_strangerCloak);
            }

            _cloakLocator = Locator.GetRootTransform().gameObject.AddComponent<CloakLocatorController>();
            foreach (var cloak in _cloaks)
            {
                cloak.enabled = false;
                cloak.UpdateCloakVisualsState();
            }

            Refresh();
        }

        public static void RegisterCloak(CloakFieldController cloak)
        {
            _cloaks.Add(cloak);
        }

        public static void DeregisterCloak(CloakFieldController cloak)
        {
            if (_cloaks.Contains(cloak))
            {
                cloak.enabled = false;
                cloak.UpdateCloakVisualsState();
                _cloaks.Remove(cloak);

                Refresh();
            }
        }

        private static void Refresh()
        {
            // Make sure we aren't using the disabled cloak
            Locator.RegisterCloakFieldController(_cloaks.FirstOrDefault());

            if (!_cloaks.Any())
            {
                // For some reason ship/scout HUD markers break if this isn't set to the Stranger when it is disabled #647
                Locator.RegisterCloakFieldController(GameObject.FindObjectOfType<CloakFieldController>());
                Shader.DisableKeyword("_CLOAKINGFIELDENABLED");
                _cloakLocator.SetCurrentCloak(null);
                _cloakLocator.enabled = false;
            }
        }
    }
}
