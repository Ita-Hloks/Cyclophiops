using System;
using System.IO;
using System.Text;
using Cyclophiops.Export;
using Microsoft.Win32;

namespace Cyclophiops.Regedit.Utils
{
    internal class ReadRegeditValue
    {
        public class Config
        {
            public string Path { get; private set; }

            public string[] ValueNames { get; private set; }

            public string Title { get; private set; }

            public RegistryHive Hive { get; private set; }

            public RegistryView View { get; private set; }

            public Config(string path, string[] valueNames, string title = null,
                RegistryHive hive = RegistryHive.LocalMachine, RegistryView view = RegistryView.Registry64)
            {
                Path = path;
                ValueNames = valueNames;
                Title = title;
                Hive = hive;
                View = view;
            }
        }

        public static bool ReadMulRegeditToFile(Config[] configs)
        {
            try
            {
                var filePath = OutputFile.EnsureOutputPath($"{DateTime.Now:yyyy-MM-dd HHmmss}registry_export.txt");
                var sb = new StringBuilder();
                sb.AppendLine($"Registry Export - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine();

                foreach (var config in configs)
                {
                    AppendRegistryConfig(sb, config);
                }

                EnsureDirectoryExists(filePath);
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("批量读取注册表失败", " ", ex);
                return false;
            }
        }

        private static void AppendRegistryConfig(StringBuilder sb, Config config)
        {
            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(config.Hive, config.View))
                using (var key = baseKey.OpenSubKey(config.Path))
                {
                    if (key == null)
                    {
                        sb.AppendLine($"[FAILED] {config.Title ?? config.Path}");
                        sb.AppendLine($"Reason: Key not found - {config.Path}");
                        sb.AppendLine();
                        return;
                    }

                    sb.AppendLine($"===== {config.Title ?? config.Path} =====");
                    sb.AppendLine();

                    foreach (var name in config.ValueNames)
                    {
                        var value = key.GetValue(name);
                        sb.AppendLine($"{name} = {value ?? "NULL"}");
                    }

                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[ERROR] {config.Title ?? config.Path}");
                sb.AppendLine($"Reason: {ex.Message}");
                sb.AppendLine();
            }
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
