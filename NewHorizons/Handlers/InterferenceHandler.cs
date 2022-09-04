using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Handlers
{
    using InterferenceVolume = NewHorizons.Components.InterferenceVolume;

    public static class InterferenceHandler
    {
        private static List<InterferenceVolume> _playerInterference;
        private static List<InterferenceVolume> _probeInterference;
        private static List<InterferenceVolume> _shipInterference;

        public static void Init()
        {
            _playerInterference = new List<InterferenceVolume>();
            _probeInterference = new List<InterferenceVolume>();
            _shipInterference = new List<InterferenceVolume>();
        }

        public static bool PlayerHasInterference() => _playerInterference.Any(volume => volume != null);
        public static bool ProbeHasInterference() => _probeInterference.Any(volume => volume != null);
        public static bool ShipHasInterference() => _shipInterference.Any(volume => volume != null);

        public static bool IsPlayerSameAsProbe()
        {
            _playerInterference.RemoveAll(volume => volume == null);
            return _playerInterference.All(_probeInterference.Contains) && _playerInterference.Count == _probeInterference.Count;
        }

        public static bool IsPlayerSameAsShip()
        {
            _playerInterference.RemoveAll(volume => volume == null);
            return _playerInterference.All(_shipInterference.Contains) && _playerInterference.Count == _shipInterference.Count;
        }

        public static void OnPlayerEnterInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            _playerInterference.SafeAdd(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnPlayerExitInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            _playerInterference.Remove(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnProbeEnterInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            _probeInterference.SafeAdd(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnProbeExitInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            _probeInterference.Remove(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnShipEnterInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            _shipInterference.SafeAdd(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }
        public static void OnShipExitInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            _shipInterference.Remove(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }
    }
}
