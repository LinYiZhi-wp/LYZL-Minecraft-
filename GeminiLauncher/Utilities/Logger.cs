using System;
using System.IO;

namespace GeminiLauncher.Utilities
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GeminiLauncher.log");
        private static readonly object _lock = new object();
        private const long MaxLogSizeBytes = 5 * 1024 * 1024;

        public static void Initialize()
        {
            try
            {
                lock (_lock)
                {
                    if (File.Exists(LogPath))
                    {
                        var info = new FileInfo(LogPath);
                        if (info.Length > MaxLogSizeBytes)
                        {
                            string archivePath = LogPath.Replace(".log", $".{DateTime.Now:yyyyMMdd_HHmmss}.log");
                            File.Move(LogPath, archivePath);

                            int maxArchives = 3;
                            var archives = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "GeminiLauncher.*.log")
                                .OrderByDescending(f => f).Skip(maxArchives).ToList();
                            foreach (var old in archives)
                            {
                                try { File.Delete(old); } catch { }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public static void Log(string message, string type = "INFO")
        {
            try
            {
                lock (_lock)
                {
                    string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {message}{Environment.NewLine}";
                    File.AppendAllText(LogPath, logLine);
                    // Also write to console for easier debugging
                    Console.WriteLine(logLine.Trim());
                }
            }
            catch { /* Best effort logging */ }
        }

        public static void LogError(Exception ex, string context)
        {
            Log($"[{context}] {ex.Message}\nStackTrace: {ex.StackTrace}", "ERROR");
            if (ex.InnerException != null)
            {
                Log($"InnerException: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}", "ERROR-INNER");
            }
        }

        public static void LogInfo(string message)
        {
            Log(message, "INFO");
        }

        public static void LogWarning(string message)
        {
            Log(message, "WARNING");
        }

        public static void LogDebug(string message)
        {
            Log(message, "DEBUG");
        }

        public static void LogCritical(string message)
        {
            Log(message, "CRITICAL");
        }

        public static void LogGameOutput(string message)
        {
            Log(message, "GAME-OUTPUT");
        }

        public static void LogGameError(string message)
        {
            Log(message, "GAME-ERROR");
        }

        public static void ClearLog()
        {
            try
            {
                lock (_lock)
                {
                    if (File.Exists(LogPath))
                    {
                        File.WriteAllText(LogPath, $"--- Log Cleared at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---\n");
                    }
                }
            }
            catch { /* Best effort */ }
        }

        public static string GetLogPath()
        {
            return LogPath;
        }
    }
}
