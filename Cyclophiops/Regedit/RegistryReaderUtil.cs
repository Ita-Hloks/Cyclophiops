using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;

namespace Cyclophiops.Regedit
{
    public class RegistryReaderUtil
    {
        /// <summary>
        /// 读取注册表并写入文件（简化版）
        /// </summary>
        /// <param name="registryPath">注册表路径，如 @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"</param>
        /// <param name="valueNames">要读取的值名称数组</param>
        /// <param name="filePath">输出文件路径</param>
        /// <param name="title">文件标题（可选）</param>
        /// <param name="hive">注册表根键，默认为 LocalMachine</param>
        /// <param name="view">注册表视图，默认为 64 位</param>
        /// <returns>是否成功</returns>
        public static bool ReadRegistryToFile(
            string registryPath,
            string[] valueNames,
            string filePath,
            string title = "Registry Info",
            RegistryHive hive = RegistryHive.LocalMachine,
            RegistryView view = RegistryView.Registry64)
        {
            try
            {
                var key = RegistryKey
                    .OpenBaseKey(hive, view)
                    .OpenSubKey(registryPath, writable: false);

                if (key == null)
                    return false;

                var sb = new StringBuilder();
                sb.AppendLine($"==== {title} ====");
                sb.AppendLine($"Path: {registryPath}");
                sb.AppendLine();

                foreach (var name in valueNames)
                {
                    object value = key.GetValue(name);
                    sb.AppendLine($"{name} = {value ?? "NULL"}");
                }

                sb.AppendLine();

                File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                // 等待修改
                File.AppendAllText("registry_error.log",
                    $"[{DateTime.Now}] {ex}\n\n");
                return false;
            }
        }

        /// <summary>
        /// 批量读取多个注册表路径（高级版）
        /// </summary>
        public static bool ReadMultipleRegistriesToFile(
            RegistryReadConfig[] configs,
            string filePath,
            bool append = false)
        {
            try
            {
                var sb = new StringBuilder();

                if (!append)
                {
                    sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine();
                }

                foreach (var config in configs)
                {
                    var key = RegistryKey
                        .OpenBaseKey(config.Hive, config.View)
                        .OpenSubKey(config.Path, writable: false);

                    if (key == null)
                    {
                        sb.AppendLine($"[FAILED] {config.Title ?? config.Path}");
                        sb.AppendLine("Reason: Key not found");
                        sb.AppendLine();
                        continue;
                    }

                    sb.AppendLine($"===== {config.Title ?? config.Path} =====");
                    sb.AppendLine();

                    foreach (var name in config.ValueNames)
                    {
                        object value = key.GetValue(name);
                        sb.AppendLine($"{name} = {value ?? "NULL"}");
                    }

                    sb.AppendLine();
                }

                if (append)
                    File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
                else
                    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText("registry_error.log",
                    $"[{DateTime.Now}] {ex}\n\n");
                return false;
            }
        }

        /// <summary>
        /// 读取注册表子项下的所有值
        /// </summary>
        public static bool ReadAllValuesToFile(
            string registryPath,
            string filePath,
            string title = "Registry Info",
            RegistryHive hive = RegistryHive.LocalMachine,
            RegistryView view = RegistryView.Registry64)
        {
            try
            {
                var key = RegistryKey
                    .OpenBaseKey(hive, view)
                    .OpenSubKey(registryPath, writable: false);

                if (key == null)
                    return false;

                var valueNames = key.GetValueNames();
                return ReadRegistryToFile(registryPath, valueNames, filePath, title, hive, view);
            }
            catch (Exception ex)
            {
                File.AppendAllText("registry_error.log",
                    $"[{DateTime.Now}] {ex}\n\n");
                return false;
            }
        }
    }

    /// <summary>
    /// 注册表读取配置类
    /// </summary>
    public class RegistryReadConfig
    {
        public string Path { get; set; }
        public string[] ValueNames { get; set; }
        public string Title { get; set; }
        public RegistryHive Hive { get; set; } = RegistryHive.LocalMachine;
        public RegistryView View { get; set; } = RegistryView.Registry64;

        public RegistryReadConfig(string path, string[] valueNames, string title = null)
        {
            Path = path;
            ValueNames = valueNames;
            Title = title;
        }
    }
}
