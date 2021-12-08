using OWML.Common;
using System;
using System.ComponentModel;
using UnityEngine;

namespace NewHorizons.Utility
{
    public class Logger
    {
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
                catch(Exception)
                {
                    value = null;
                }
                
                Log($"{obj.name} {name}={value}");
            }
        }

        public static void LogPath(GameObject go)
        {
            if (go == null) Log("Can't print path: GameObject is null");
            else Log($"{GetPath(go.transform)}");
        }

        private static string GetPath(Transform current)
        {
            if (current.parent == null) return "/" + current.name;
            return GetPath(current.parent) + "/" + current.name;
        }

        public static void Log(string text, LogType type)
        {
            Main.helper.Console.WriteLine(Enum.GetName(typeof(LogType), type) + " : " + text, LogTypeToMessageType(type));
        }

        public static void Log(string text)
        {
            Log(text, LogType.Log);
        }
        public static void LogError(string text)
        {
            Log(text, LogType.Error);
        }
        public enum LogType
        {
            Log,
            Error,
            Warning,
            Todo
        }
        private static MessageType LogTypeToMessageType(LogType t) 
        {
            switch(t)
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
