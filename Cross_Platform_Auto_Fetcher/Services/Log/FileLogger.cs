using System;
using System.IO;

namespace Cross_Platform_Auto_Fetcher.Services.Log
{
    public static class FileLogger
    {
        private static readonly string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_log.txt");
        private static readonly object _lock = new object();

        static FileLogger()
        {
            // Clear the log file on new run
            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
        }

        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logFilePath, message + "\n");
                }
            }
            catch (Exception)
            {
                // Don't crash the app if logging fails
            }
        }
    }
}

