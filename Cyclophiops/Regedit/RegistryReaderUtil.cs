using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Microsoft.Win32;

namespace Cyclophiops.Regedit
{
    // ==================== 全局配置 ====================
    public static class RegistryConfig
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
        private static readonly string LogFile = Path.Combine(LogDirectory, "registry.log");

        private static void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        private static void WriteLog(string level, string message, Exception ex = null)
        {
            try
            {
                EnsureLogDirectoryExists();
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var log = $"[{timestamp}] |{level}| {message}";
                if (ex != null)
                {
                    log += $"\n{ex}";
                }

                log += "\n";
                File.AppendAllText(LogFile, log, Encoding.UTF8);
            }
            catch
            {
            }
        }

        public static void LogInfo(string message) => WriteLog("INFO", message);

        public static void LogError(string message, Exception ex = null) => WriteLog("ERROR", message, ex);

        internal static string GetExportPath(string baseName)
        {
            EnsureLogDirectoryExists();
            var fileName = $"{baseName}_{DateTime.Now:yyyy-MM-dd_HHmmss}.txt";
            return Path.Combine(LogDirectory, fileName);
        }
    }

    // ==================== 配置类 ====================
    public class RegistryReadConfig
    {
        public readonly string Path;
        public readonly string[] ValueNames;
        public readonly string Title;
        public readonly RegistryHive Hive;
        public readonly RegistryView View;

        public RegistryReadConfig(string path, string[] valueNames, string title = null,
            RegistryHive hive = RegistryHive.LocalMachine, RegistryView view = RegistryView.Registry64)
        {
            Path = path;
            ValueNames = valueNames;
            Title = title;
            Hive = hive;
            View = view;
        }
    }

    public class RegistryEnumerateConfig
    {
        public readonly string Path;
        public readonly string Title;
        public readonly RegistryHive Hive;
        public readonly RegistryView View;
        public readonly Func<string, bool> Filter;
        public readonly EnumerateOptions Options;

        public RegistryEnumerateConfig(string path, string title = null,
            RegistryHive hive = RegistryHive.LocalMachine, RegistryView view = RegistryView.Registry64,
            Func<string, bool> filter = null, EnumerateOptions options = null)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Title = title;
            Hive = hive;
            View = view;
            Filter = filter;
            Options = options ?? new EnumerateOptions();
        }
    }

    public class EnumerateOptions
    {
        public bool Recursive = false;
        public int MaxDepth = -1;
        public bool IncludeEmpty = true;
    }

    public class RegistryFolderInfo
    {
        public string Name;
        public string FullPath;
        public int Depth;
        public int SubKeyCount;
    }

    public class RegistryEnumerateResult
    {
        public List<RegistryFolderInfo> Folders = new List<RegistryFolderInfo>();
        public int TotalCount;
        public int FilteredCount;
        public bool Success;
        public string ErrorMessage;
    }

    // ==================== 过滤器 ====================
    public static class RegistryFilters
    {
        public static Func<string, bool> StartsWith(string prefix)
            => name => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

        public static Func<string, bool> EndsWith(string suffix)
            => name => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);

        public static Func<string, bool> Contains(string text)
            => name => name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;

        public static Func<string, bool> Regex(string pattern)
        {
            var regex = new System.Text.RegularExpressions.Regex(
                pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return name => regex.IsMatch(name);
        }

        public static Func<string, bool> Exclude(params string[] excludeNames)
        {
            var set = new HashSet<string>(excludeNames, StringComparer.OrdinalIgnoreCase);
            return name => !set.Contains(name);
        }

        public static Func<string, bool> Include(params string[] includeNames)
        {
            var set = new HashSet<string>(includeNames, StringComparer.OrdinalIgnoreCase);
            return name => set.Contains(name);
        }

        public static Func<string, bool> IsGuid()
            => name => Guid.TryParse(name, out _);

        public static Func<string, bool> And(params Func<string, bool>[] filters)
            => name => filters.All(f => f(name));

        public static Func<string, bool> Or(params Func<string, bool>[] filters)
            => name => filters.Any(f => f(name));

        public static Func<string, bool> Not(Func<string, bool> filter)
            => name => !filter(name);
    }

    // ==================== 读取工具类 ====================
    public class RegistryReaderUtil
    {
        public static bool ReadMultipleRegistriesToFile(RegistryReadConfig[] configs)
        {
            try
            {
                var filePath = RegistryConfig.GetExportPath("registry_export");
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
                RegistryConfig.LogError("批量读取注册表失败", ex);
                return false;
            }
        }

        private static void AppendRegistryConfig(StringBuilder sb, RegistryReadConfig config)
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

        /// <summary>
        /// 确保文件所在目录存在.
        /// </summary>
        private static void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }

    // ==================== 枚举工具类 ====================
    public class RegistryEnumerator
    {
        /// <summary>
        /// 枚举注册表子项.
        /// </summary>
        /// <returns></returns>
        public static RegistryEnumerateResult Enumerate(RegistryEnumerateConfig config)
        {
            var result = new RegistryEnumerateResult { Success = true };

            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(config.Hive, config.View))
                using (var key = baseKey.OpenSubKey(config.Path))
                {
                    if (key == null)
                    {
                        result.Success = false;
                        result.ErrorMessage = $"无法打开注册表路径: {config.Path}";
                        return result;
                    }

                    EnumerateRecursive(key, config, result, 0, config.Path);
                }

                result.FilteredCount = result.Folders.Count;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private static void EnumerateRecursive(
            RegistryKey key,
            RegistryEnumerateConfig config,
            RegistryEnumerateResult result,
            int depth,
            string currentPath)
        {
            if (config.Options.MaxDepth >= 0 && depth > config.Options.MaxDepth)
            {
                return;
            }

            try
            {
                var subKeyNames = key.GetSubKeyNames();
                result.TotalCount += subKeyNames.Length;

                foreach (var subKeyName in subKeyNames)
                {
                    if (!ShouldIncludeKey(config, subKeyName))
                    {
                        continue;
                    }

                    ProcessSubKey(key, subKeyName, config, result, depth, currentPath);
                }
            }
            catch (SecurityException)
            {
                // 忽略无权限访问的项
            }
        }

        private static bool ShouldIncludeKey(RegistryEnumerateConfig config, string keyName)
        {
            return config.Filter == null || config.Filter(keyName);
        }

        private static void ProcessSubKey(
            RegistryKey parentKey,
            string subKeyName,
            RegistryEnumerateConfig config,
            RegistryEnumerateResult result,
            int depth,
            string currentPath)
        {
            var fullPath = currentPath + "\\" + subKeyName;

            try
            {
                using (var subKey = parentKey.OpenSubKey(subKeyName))
                {
                    if (subKey == null)
                    {
                        return;
                    }

                    var subKeyCount = subKey.GetSubKeyNames().Length;

                    if (!config.Options.IncludeEmpty && subKeyCount == 0)
                    {
                        return;
                    }

                    result.Folders.Add(new RegistryFolderInfo
                    {
                        Name = subKeyName,
                        FullPath = fullPath,
                        Depth = depth,
                        SubKeyCount = subKeyCount,
                    });

                    if (config.Options.Recursive)
                    {
                        EnumerateRecursive(subKey, config, result, depth + 1, fullPath);
                    }
                }
            }
            catch (SecurityException)
            {
                // 忽略无权限访问的项
            }
        }

        private static string ExportTree(RegistryEnumerateResult result)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < result.Folders.Count; i++)
            {
                var folder = result.Folders[i];
                var indent = new string(' ', folder.Depth * 4);

                // 判断是否是最后一个同深度的项
                var isLast = IsLastAtDepth(result.Folders, i);

                var prefix = isLast ? "└─ " : "├─ ";
                sb.AppendLine($"{indent}{prefix}{folder.Name}");
            }

            return sb.ToString();
        }

        private static bool IsLastAtDepth(List<RegistryFolderInfo> folders, int currentIndex)
        {
            if (currentIndex >= folders.Count - 1)
            {
                return true;
            }

            var currentDepth = folders[currentIndex].Depth;

            // 检查后续项是否有相同深度的
            for (var i = currentIndex + 1; i < folders.Count; i++)
            {
                if (folders[i].Depth < currentDepth)
                {
                    return true;
                }

                if (folders[i].Depth == currentDepth)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ExportToFile(RegistryEnumerateResult result)
        {
            try
            {
                if (result == null)
                {
                    throw new ArgumentNullException(nameof(result));
                }

                var filePath = RegistryConfig.GetExportPath("registry_enumerate");
                var sb = new StringBuilder();

                sb.AppendLine($"Registry Enumerate Export - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Total: {result.TotalCount}, Filtered: {result.FilteredCount}");
                sb.AppendLine(new string('=', 60));
                sb.AppendLine();
                sb.Append(ExportTree(result));

                EnsureDirectoryExists(filePath);
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                RegistryConfig.LogInfo($"注册表枚举已导出: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                RegistryConfig.LogError("导出注册表枚举失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 确保文件所在目录存在
        /// </summary>
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
