using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Handlers
{
    public static class InterferenceHandler
    {
        private static List<string> _playerInterference;
        private static List<string> _probeInterference;
        private static List<string> _shipInterference;

        public static void Init()
        {
            _playerInterference = new List<string>();
            _probeInterference = new List<string>();
            _shipInterference = new List<string>();
        }

        public static bool PlayerHasInterference() => _playerInterference.Any();
        public static bool ProbeHasInterference() => _probeInterference.Any();
        public static bool ShipHasInterference() => _shipInterference.Any();

        public static bool IsPlayerSameAsProbe() => _playerInterference.All(_probeInterference.Contains) && _playerInterference.Count == _probeInterference.Count;
        public static bool IsPlayerSameAsShip() => _playerInterference.All(_shipInterference.Contains) && _playerInterference.Count == _shipInterference.Count;

        public static void OnPlayerEnterInterferenceVolume(string id)
        {
            _playerInterference.SafeAdd(id);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnPlayerExitInterferenceVolume(string id)
        {
            _playerInterference.Remove(id);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnProbeEnterInterferenceVolume(string id)
        {
            _probeInterference.SafeAdd(id);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnProbeExitInterferenceVolume(string id)
        {
            _probeInterference.Remove(id);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnShipEnterInterferenceVolume(string id)
        {
            _shipInterference.SafeAdd(id);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }
        public static void OnShipExitInterferenceVolume(string id)
        {
            _shipInterference.Remove(id);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }
    }
}
