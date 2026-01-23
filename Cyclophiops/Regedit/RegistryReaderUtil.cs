using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Cyclophiops.Export;
using Microsoft.Win32;

namespace Cyclophiops.Regedit
{
    public class RegistryEnumerator
    {
        /// <summary>
        /// 枚举注册表子项.
        /// </summary>
        /// <returns></returns>
        public static RegistryEnumerat.RegistryEnumerateResult Enumerate(
            string path,
            RegistryHive hive = RegistryHive.LocalMachine,
            RegistryView view = RegistryView.Registry64,
            Func<string, bool> filter = null,
            bool recursive = false,
            int maxDepth = -1,
            bool includeEmpty = true)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var result = new RegistryEnumerat.RegistryEnumerateResult { Success = true };

            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(hive, view))
                using (var key = baseKey.OpenSubKey(path))
                {
                    if (key == null)
                    {
                        result.Success = false;
                        result.ErrorMessage = $"无法打开注册表路径: {path}";
                        return result;
                    }

                    EnumerateRecursive(key, filter, recursive, maxDepth, includeEmpty, result, 0, path);
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
            Func<string, bool> filter,
            bool recursive,
            int maxDepth,
            bool includeEmpty,
            RegistryEnumerat.RegistryEnumerateResult result,
            int depth,
            string currentPath)
        {
            if (maxDepth >= 0 && depth > maxDepth)
            {
                return;
            }

            try
            {
                var subKeyNames = key.GetSubKeyNames();
                result.TotalCount += subKeyNames.Length;

                foreach (var subKeyName in subKeyNames)
                {
                    if (filter != null && !filter(subKeyName))
                    {
                        continue;
                    }

                    ProcessSubKey(key, subKeyName, filter, recursive, maxDepth, includeEmpty, result, depth, currentPath);
                }
            }
            catch (SecurityException)
            {
                // 忽略无权限访问的项
            }
        }

        private static void ProcessSubKey(
            RegistryKey parentKey,
            string subKeyName,
            Func<string, bool> filter,
            bool recursive,
            int maxDepth,
            bool includeEmpty,
            RegistryEnumerat.RegistryEnumerateResult result,
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

                    if (!includeEmpty && subKeyCount == 0)
                    {
                        return;
                    }

                    result.Folders.Add(new RegistryEnumerat.RegistryEnumerateResult.FolderInfo
                    {
                        Name = subKeyName,
                        FullPath = fullPath,
                        Depth = depth,
                        SubKeyCount = subKeyCount,
                    });

                    if (recursive)
                    {
                        EnumerateRecursive(subKey, filter, recursive, maxDepth, includeEmpty, result, depth + 1, fullPath);
                    }
                }
            }
            catch (SecurityException)
            {
                // 忽略无权限访问的项
            }
        }

        private static string ExportTree(RegistryEnumerat.RegistryEnumerateResult result)
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

        private static bool IsLastAtDepth(List<RegistryEnumerat.RegistryEnumerateResult.FolderInfo> folders, int currentIndex)
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

        public static bool ExportToFile(RegistryEnumerat.RegistryEnumerateResult result)
        {
            try
            {
                if (result == null)
                {
                    throw new ArgumentNullException(nameof(result));
                }

                var filePath = OutputFile.EnsureOutputPath("registry_enumerate.txt");
                var sb = new StringBuilder();

                sb.AppendLine($"Registry Enumerate Export - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Total: {result.TotalCount}, Filtered: {result.FilteredCount}");
                sb.AppendLine(new string('=', 60));
                sb.AppendLine();
                sb.Append(ExportTree(result));

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                OutputFile.LogInfo($"注册表枚举已导出: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("导出注册表枚举失败", " ", ex);
                return false;
            }
        }
    }
}
