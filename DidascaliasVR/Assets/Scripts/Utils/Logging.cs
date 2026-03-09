using UnityEngine;

namespace System.Runtime.CompilerServices
{
    class IsExternalInit { }
}

namespace Didascalia.Utils
{
    internal record LogString(string Value)
    {
        public static implicit operator LogString(string value) => new LogString(value);
        public static implicit operator string(LogString logString) => logString.Value;
    }

    internal static class LogStringExtensions
    {
        public static LogString Bold(this LogString str) => $"<b>{str.Value}</b>";
        public static LogString Italic(this LogString str) => $"<i>{str.Value}</i>";
        public static LogString Underline(this LogString str) => $"<u>{str.Value}</u>";
        public static LogString Strikethrough(this LogString str) => $"<s>{str.Value}</s>";
        public static LogString Size(this LogString str, float size) => $"<size={size}>{str.Value}</size>";

        public static LogString Color(this LogString str, Color color) =>
            $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{str.Value}</color>";
    }

    internal static class Log
    {
        public static void Message(string message, Object context)
        {
#if UNITY_EDITOR
            string logMessage =
                new LogString("[Didascalia]").Color(Color.mediumPurple)
                + ": " + message;
            Debug.Log(logMessage, context);
#else
            _ = context;
            Console.WriteLine("[Didascalia]: " + message);
#endif
        }
        public static void Warning(string message, Object context)
        {
#if UNITY_EDITOR
            string logMessage =
                new LogString("[Didascalia]").Color(Color.mediumPurple)
                + new LogString(" [warning]").Color(Color.yellow)
                + ": " + message;
            Debug.LogWarning(logMessage, context);
#else
            _ = context;
            Console.WriteLine("[Didascalia] [warning]: " + message);
#endif
        }
        public static void Info(string message, Object context)
        {
#if UNITY_EDITOR
            string logMessage =
                new LogString("[Didascalia]").Color(Color.mediumPurple)
                + new LogString(" [info]").Color(Color.cyan)
                + ": " + message;
            Debug.Log(logMessage, context);
#else
            _ = context;
            Console.WriteLine("[Didascalia] [info]: " + message);
#endif
        }
    }
}