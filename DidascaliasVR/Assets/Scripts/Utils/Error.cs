namespace Didascalia.Utils
{
    internal static class Error
    {
        // public static void DebugbreakFail(Object context)
        // {
        //     DebugbreakFailMessage("DebugbreakFail called", context);
        // }

        public static void DebugbreakFailMessage(string message, UnityEngine.Object context)
        {
#if UNITY_EDITOR
            string logMessage =
                new LogString("[Didascalia]").Color(UnityEngine.Color.mediumPurple)
                + new LogString(" [error]").Color(UnityEngine.Color.red)
                + ": " + message;
            UnityEngine.Debug.Assert(false, logMessage, context);
            UnityEngine.Debug.Break();
#else
            _ = context;
            System.Diagnostics.Trace.Assert(false, "[Didascalia] [error]: " + message);
            System.Environment.Exit(1);
#endif

        }

        public static void DebugbreakFailUnless(bool condition, string message, UnityEngine.Object context)
        {
            if (!condition)
            {
                DebugbreakFailMessage(message, context);
            }
        }
        public static void DebugbreakFailIf(bool condition, string message, UnityEngine.Object context)
        {
            if (condition)
            {
                DebugbreakFailMessage(message, context);
            }
        }

        
        public static void DebugbreakFailUnimplemented(string message, UnityEngine.Object context)
        {
#if UNITY_EDITOR
            string logMessage =
                new LogString("[Didascalia]").Color(UnityEngine.Color.mediumPurple)
                + new LogString(" [unimplemented]").Color(UnityEngine.Color.yellow)
                + ": " + message;
            UnityEngine.Debug.Assert(false, logMessage, context);
            UnityEngine.Debug.Break();
#else
            _ = context;
            System.Diagnostics.Trace.Assert(false, "[Didascalia] [unimplemented]: " + message);
            System.Environment.Exit(1);
#endif
        }
    }
}