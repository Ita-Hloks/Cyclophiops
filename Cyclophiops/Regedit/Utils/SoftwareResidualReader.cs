using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Cyclophiops.Export;
using Microsoft.Win32;

namespace Cyclophiops.Regedit.Utils
{
    public class SoftwareResidualReader
    {
        public class ResidualInfo
        {
            public string KeyName { get; set; }

            public string DisplayName { get; set; }

            public string Version { get; set; }

            public string Publisher { get; set; }

            public string InstallPath { get; set; }

            public int ValueCount { get; set; }

            public int SubKeyCount { get; set; }
        }

        private static readonly HashSet<string> _systemPathBlacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Microsoft",
            "Policies",
            "Classes",
            "RegisteredApplications",
            "Clients",
            "StartMenuInternet",
            "AppDataLow",
            "CloudStore",
            "ODBC",
            "Wow6432Node",
            "Adobe",
            "Macromedia",
            "AppXBackupContentType",
            "ControlSet001",
            "Installer",
        };

        private static readonly string[] _knownSystemPrefixes = new[]
        {
            "Microsoft.",
            "Windows.",
            "ms-",
            "AppX",
            "ActivatableClasses",
        };

        public static List<ResidualInfo> ScanSoftwareResiduals()
        {
            var results = new List<ResidualInfo>();

            try
            {
                var userSids = SidHelper.GetAllUserSids();
                OutputFile.LogInfo($"开始扫描软件残骸，找到 {userSids.Count} 个用户配置");

                foreach (var sid in userSids)
                {
                    ScanUserSoftware(sid, results);
                }

                OutputFile.LogInfo($"扫描完成，找到 {results.Count} 个软件痕迹");
            }
            catch (Exception ex)
            {
                OutputFile.LogError("扫描软件残骸时出错", ex);
            }

            return results;
        }

        private static void ScanUserSoftware(string sid, List<ResidualInfo> results)
        {
            try
            {
                var path = $@"{sid}\SOFTWARE";

                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64))
                using (var softwareKey = baseKey.OpenSubKey(path))
                {
                    if (softwareKey == null)
                    {
                        return;
                    }

                    var subKeyNames = softwareKey.GetSubKeyNames();

                    foreach (var subKeyName in subKeyNames)
                    {
                        if (ShouldSkipKey(subKeyName))
                        {
                            continue;
                        }

                        try
                        {
                            using (var key = softwareKey.OpenSubKey(subKeyName))
                            {
                                if (key == null)
                                {
                                    continue;
                                }

                                var residual = AnalyzeKey(key, subKeyName);
                                if (residual != null)
                                {
                                    results.Add(residual);
                                }
                            }
                        }
                        catch (SecurityException)
                        {
                        }
                        catch (Exception ex)
                        {
                            OutputFile.LogError($"读取键 {subKeyName} 时出错", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OutputFile.LogError($"扫描用户 {sid} 的 SOFTWARE 键时出错", ex);
            }
        }

        private static bool ShouldSkipKey(string keyName)
        {
            if (_systemPathBlacklist.Contains(keyName))
            {
                return true;
            }

            foreach (var prefix in _knownSystemPrefixes)
            {
                if (keyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static ResidualInfo AnalyzeKey(RegistryKey key, string keyName)
        {
            var valueNames = key.GetValueNames();
            var subKeyNames = key.GetSubKeyNames();

            var displayName = GetStringValue(key, "DisplayName", "ProductName", "Name");
            var version = GetStringValue(key, "Version", "DisplayVersion", "ProductVersion");
            var publisher = GetStringValue(key, "Publisher", "Manufacturer", "Company");
            var installPath = GetStringValue(key, "InstallPath", "InstallLocation", "Path", "InstallDir");

            return new ResidualInfo
            {
                KeyName = keyName,
                DisplayName = displayName ?? string.Empty,
                Version = version ?? string.Empty,
                Publisher = publisher ?? string.Empty,
                InstallPath = installPath ?? string.Empty,
                ValueCount = valueNames.Length,
                SubKeyCount = subKeyNames.Length,
            };
        }

        private static string GetStringValue(RegistryKey key, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                var value = key.GetValue(name) as string;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        public static bool ExportToCsv(List<ResidualInfo> residuals, string filePath = null)
        {
            try
            {
                if (residuals == null || residuals.Count == 0)
                {
                    OutputFile.LogInfo("没有软件残骸可导出");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    filePath = OutputFile.EnsureOutputPath(
                        $"SoftwareResiduals_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                        defaultExtension: ".csv");
                }

                var sb = new StringBuilder();

                sb.AppendLine("KeyName,DisplayName,Version,Publisher,InstallPath,ValueCount,SubKeyCount");

                foreach (var residual in residuals)
                {
                    sb.AppendLine(string.Join(",",
                        EscapeCsvField(residual.KeyName),
                        EscapeCsvField(residual.DisplayName),
                        EscapeCsvField(residual.Version),
                        EscapeCsvField(residual.Publisher),
                        EscapeCsvField(residual.InstallPath),
                        residual.ValueCount.ToString(),
                        residual.SubKeyCount.ToString()));
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                OutputFile.LogInfo($"软件残骸已导出到: {filePath} (共 {residuals.Count} 项)");

                return true;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("导出软件残骸到 CSV 失败", ex);
                return false;
            }
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }
    }
}
