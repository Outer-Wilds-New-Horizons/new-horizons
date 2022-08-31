using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Handlers
{
    public static class InterferenceHandler
    {
        public static bool _playerInterference;
        public static bool _probeInterference;

        public static bool PlayerHasInterference() => _playerInterference;
        public static bool ProbeHasInterference() => _probeInterference;

        public static void OnPlayerEnterInterferenceVolume() => _playerInterference = true;
        public static void OnPlayerExitInterferenceVolume() => _playerInterference = false;

        public static void OnProbeEnterInterferenceVolume() => _probeInterference = true;
        public static void OnProbeExitInterferenceVolume() => _probeInterference = false;
    }
}
