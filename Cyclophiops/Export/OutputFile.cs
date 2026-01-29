using System;
using System.IO;
using System.Text;

namespace Cyclophiops.Export
{
    public class OutputFile
    {
        private static readonly object _logLock = new object();
        private static string _dailyLogPath;

        public static string EnsureOutputPath(string path = "")
        {
            if (string.IsNullOrWhiteSpace(path) || path == " ")
            {
                path = GetDailyLogPath();
            }

            path = path.Replace("/", "\\");

            if (path.StartsWith("@\\"))
            {
                path = path.Substring(2);
            }

            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), path);
            }

            path = Path.GetFullPath(path);

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path + ".txt";
            }

            return path;
        }

        public static string GetDailyLogPath()
        {
            if (_dailyLogPath == null || !IsSameDay(_dailyLogPath))
            {
                _dailyLogPath = $"D:\\log\\{DateTime.Now:yyyy-MM-dd}.log";
            }

            return _dailyLogPath;
        }

        private static bool IsSameDay(string logPath)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(logPath);
                var fileDate = DateTime.ParseExact(fileName, "yyyy-MM-dd", null);
                return fileDate.Date == DateTime.Now.Date;
            }
            catch
            {
                return false;
            }
        }

        public static void LogInfo(string message, string logDirectory = "") => WriteLog("INFO", message, logDirectory);

        public static void LogError(string message, string logDirectory = "", Exception ex = null) => WriteLog("ERROR", message, logDirectory, ex);

        private static void WriteLog(string level, string message, string logDirectory, Exception ex = null)
        {
            lock (_logLock)
            {
                try
                {
                    var logPath = EnsureOutputPath(logDirectory);
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var log = $"[{timestamp}] |{level}| {message}";
                    if (ex != null)
                    {
                        log += $"\n{ex}";
                    }

                    log += "\n";
                    File.AppendAllText(logPath, log, Encoding.UTF8);
                }
                catch
                {
                }
            }
        }
    }
}
