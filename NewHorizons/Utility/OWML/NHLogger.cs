using OWML.Common;
using System;

namespace NewHorizons.Utility.OWML
{
    public static class NHLogger
    {

        private static LogType _logLevel = LogType.Error;

        public static void UpdateLogLevel(LogType newLevel)
        {
            _logLevel = newLevel;
        }

        private static void Log(object text, LogType type)
        {
            if (type < _logLevel) return;
            Main.Instance.ModHelper.Console.WriteLine($"{Enum.GetName(typeof(LogType), type)} : {text}", LogTypeToMessageType(type));
        }

        public static void LogVerbose(params object[] obj) => LogVerbose(string.Join(", ", obj));
        public static void LogVerbose(object text) => Log(text, LogType.Verbose);

        public static void Log(object text) => Log(text, LogType.Log);
        public static void Log(params object[] obj) => Log(string.Join(", ", obj));

        public static void LogWarning(object text) => Log(text, LogType.Warning);
        public static void LogWarning(params object[] obj) => LogWarning(string.Join(", ", obj));

        public static void LogError(object text) => Log(text, LogType.Error);
        public static void LogError(params object[] obj) => LogError(string.Join(", ", obj));

        public enum LogType
        {
            Verbose,
            Log,
            Warning,
            Error,
        }

        private static MessageType LogTypeToMessageType(LogType t) =>
            t switch
            {
                LogType.Error => MessageType.Error,
                LogType.Warning => MessageType.Warning,
                _ => MessageType.Info
            };
    }
}
