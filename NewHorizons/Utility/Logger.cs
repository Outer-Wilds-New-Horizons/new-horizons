using OWML.Common;
using System;
using System.ComponentModel;
using UnityEngine;
namespace NewHorizons.Utility
{
    public static class Logger
    {

        private static LogType _logLevel = LogType.Error;

        public static void UpdateLogLevel(LogType newLevel)
        {
            _logLevel = newLevel;
        }

        public static void LogProperties(UnityEngine.Object obj)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                string name = descriptor?.Name;
                object value;
                try
                {
                    value = descriptor.GetValue(obj);
                }
                catch (Exception)
                {
                    value = null;
                }

                Log($"{obj.name} {name}={value}");
            }
        }

        public static void LogPath(GameObject go)
        {
            if (go == null) Log("Can't print path: GameObject is null");
            else Log($"{go.transform.GetPath()}");
        }

        public static void Log(string text, LogType type)
        {
            if ((int)type < (int)_logLevel) return;
            Main.Instance.ModHelper.Console.WriteLine(Enum.GetName(typeof(LogType), type) + " : " + text, LogTypeToMessageType(type));
        }

        public static void LogVerbose(string text)
        {
            Log(text, LogType.Verbose);
        }
        public static void Log(string text)
        {
            Log(text, LogType.Log);
        }
        public static void LogError(string text)
        {
            Log(text, LogType.Error);
        }
        public static void LogWarning(string text)
        {
            Log(text, LogType.Warning);
        }
        public enum LogType
        {
            Todo,
            Verbose,
            Log,
            Warning,
            Error,
        }
        private static MessageType LogTypeToMessageType(LogType t)
        {
            switch (t)
            {
                case LogType.Error:
                    return MessageType.Error;
                case LogType.Warning:
                    return MessageType.Warning;
                default:
                    return MessageType.Info;
            }
        }
    }
}
