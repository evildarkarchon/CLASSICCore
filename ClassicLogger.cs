using NLog;

namespace CLASSICCore
{
    public static class ClassicLogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void LogInfo(string message)
        {
            Logger.Info(message);
        }

        public static void LogError(string message)
        {
            Logger.Error(message);
        }

        public static void LogDebug(string message)
        {
            Logger.Debug(message);
        }

        // Add other log methods as needed
    }
}