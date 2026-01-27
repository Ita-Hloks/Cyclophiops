using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Cyclophiops.Export
{
    public class OutputFile
    {
        public static string EnsureOutputPath(string path = "")
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                var random = new Random();
                var randomString = new string(Enumerable.Range(0, 10)
                    .Select(_ => (char)random.Next('a', 'z' + 1))
                    .ToArray());

                path = $"D:/log/{DateTime.Now:yyyy-MM-dd_HHmmss}_log.txt";
            }

            // 相对路径转绝对路径
            if (path.StartsWith("@/") || path.StartsWith(@"@\"))
            {
                path = path.Substring(2); // 去掉 "@/" 或 "@\"
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

            // 确保路径指向文件
            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path + ".txt";
            }

            return path;
        }

        // Log

        public static void LogInfo(string message, string logDirectory = "") => WriteLog("INFO", message, logDirectory);

        public static void LogError(string message, string logDirectory = "", Exception ex = null) => WriteLog("ERROR", message, logDirectory, ex);

        private static void WriteLog(string level, string message, string logDirectory, Exception ex = null)
        {
            try
            {
                EnsureOutputPath(logDirectory);
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var log = $"[{timestamp}] |{level}| {message}";
                if (ex != null)
                {
                    log += $"\n{ex}";
                }

                log += "\n";
                File.AppendAllText(logDirectory, log, Encoding.UTF8);
            }
            catch
            {
            }
        }
    }
}
