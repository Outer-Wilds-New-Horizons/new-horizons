#region

using System;
using System.ComponentModel;
using OWML.Common;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace NewHorizons.Utility
{
    public static class Logger
    {
        private static LogType _logLevel = LogType.Error;

        public static void UpdateLogLevel(LogType newLevel)
        {
            _logLevel = newLevel;
        }

        public static void LogProperties(Object obj)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                var name = descriptor?.Name;
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
            else Log($"{SearchUtilities.GetPath(go.transform)}");
        }

        public static void Log(string text, LogType type)
        {
            if ((int) type < (int) _logLevel) return;
            Main.Instance.ModHelper.Console.WriteLine(Enum.GetName(typeof(LogType), type) + " : " + text,
                LogTypeToMessageType(type));
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
            Log,
            Warning,
            Error
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