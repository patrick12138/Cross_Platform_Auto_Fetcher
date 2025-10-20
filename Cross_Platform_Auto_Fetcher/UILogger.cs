using System;

namespace Cross_Platform_Auto_Fetcher
{
    public static class UILogger
    {
        public static event Action<string> OnLog;

        public static void Log(string message)
        {
            // Ensure the event is invoked on the UI thread
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                OnLog?.Invoke(message);
            });
        }
    }
}
