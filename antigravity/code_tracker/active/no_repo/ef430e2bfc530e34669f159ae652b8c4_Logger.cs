Ùusing System;
using System.IO;

namespace GeminiLauncher.Utilities
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GeminiLauncher.log");
        private static readonly object _lock = new object();

        public static void Log(string message, string type = "INFO")
        {
            try
            {
                lock (_lock)
                {
                    string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {message}{Environment.NewLine}";
                    File.AppendAllText(LogPath, logLine);
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
    }
}
Ù*cascade082Dfile:///c:/Users/Linyizhi/.gemini/GeminiLauncher/Utilities/Logger.cs