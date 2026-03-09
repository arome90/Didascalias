using UnityEngine;

namespace Didascalia.Utils
{
    internal static class Error
    {
        // public static void DebugbreakFail(Object context)
        // {
        //     DebugbreakFailMessage("DebugbreakFail called", context);
        // }

        public static void DebugbreakFailMessage(string message, Object context)
        {
            string logMessage =
                new LogString("[Didascalia]").Color(Color.mediumPurple)
                + new LogString(" [error]").Color(Color.red)
                + ": " + message;
#if UNITY_EDITOR
            Debug.Assert(false, logMessage, context);
#else
            throw new System.Exception(logMessage);
#endif

        }

        public static void DebugbreakFailUnless(bool condition, string message, Object context)
        {
            if (!condition)
            {
                DebugbreakFailMessage(message, context);
            }
        }
        public static void DebugbreakFailIf(bool condition, string message, Object context)
        {
            if (condition)
            {
                DebugbreakFailMessage(message, context);
            }
        }
    }
}