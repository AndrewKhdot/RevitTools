using System;
using System.IO;

namespace RevitTools.Core.Services
{
    public static class LoggingService
    {
        private static readonly object _lock = new object();

        private static string LogFilePath
        {
            get
            {
                string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string dir = Path.Combine(baseDir, "RevitTools", "Logs");
                Directory.CreateDirectory(dir);

                return Path.Combine(dir, "plugin.log");
            }
        }

        public static void Log(string message)
        {
            lock (_lock)
            {
                File.AppendAllText(LogFilePath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}\n");
            }
        }

        public static void LogException(Exception ex)
        {
            Log($"ERROR: {ex.Message}\n{ex.StackTrace}");
        }
    }
}