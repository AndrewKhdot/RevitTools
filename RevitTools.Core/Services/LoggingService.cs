using System;
using System.IO;

namespace RevitTools.Core.Services
{
    public static class LoggingService
    {
        private static readonly object _lock = new object();
        public static event Action<string> OnLog;
        public static event Action OnClear; 


        private static string LogDirectory
        {
            get
            {
                string baseDir = Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData);

                return Path.Combine(baseDir, "RevitTools", "Logs");
            }
        }

        private static string LogFilePath =>
            Path.Combine(LogDirectory, "plugin.log");


        public static void Log(string message)
        {
            
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";

            lock (_lock)
            {
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }
            OnLog?.Invoke(line);
        }

        public static void LogException(Exception ex)
        {
            Log($"ERROR: {ex.Message}\n{ex.StackTrace}");
        }

        
        public static void Clear()
        {
            lock (_lock)
            {
                if (File.Exists(LogFilePath))
                    File.Delete(LogFilePath);
            }

            OnClear?.Invoke();
        }


        
        public static string GetLogDirectory()
        {
            Directory.CreateDirectory(LogDirectory);
            return LogDirectory;
        }

    }
}