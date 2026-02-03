using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Cyclophiops.Export;
using Microsoft.Win32;

namespace Cyclophiops.Regedit.Utils
{
    public class SoftwareReader
    {
        public class SoftwareInfo
        {
            public string Name { get; set; }

            public string DisplayName { get; set; }

            public string Version { get; set; }

            public string Publisher { get; set; }

            public string InstallDate { get; set; }

            public string InstallLocation { get; set; }

            public string RegistryPath { get; set; }
        }

        public static List<SoftwareInfo> GetInstalledSoftware()
        {
            var results = new List<SoftwareInfo>();

            ReadFromUninstallKeys(results);
            ReadFromUserProfiles(results);

            return results;
        }

        private static void ReadFromUninstallKeys(List<SoftwareInfo> results)
        {
            var uninstallPaths = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
            };

            foreach (var path in uninstallPaths)
            {
                ReadSoftwareFromPath(
                    path,
                    RegistryHive.LocalMachine,
                    RegistryView.Registry64,
                    results);
            }
        }

        private static void ReadFromUserProfiles(List<SoftwareInfo> results)
        {
            try
            {
                var userSids = SidHelper.GetAllUserSids();

                foreach (var sid in userSids)
                {
                    var path = $@"{sid}\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

                    ReadSoftwareFromPath(
                        path,
                        RegistryHive.Users,
                        RegistryView.Registry64,
                        results);
                }
            }
            catch (Exception ex)
            {
                OutputFile.LogError("读取用户配置文件软件时出错", ex);
            }
        }

        private static void ReadSoftwareFromPath(
            string path,
            RegistryHive hive,
            RegistryView view,
            List<SoftwareInfo> results)
        {
            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(hive, view))
                using (var uninstallKey = baseKey.OpenSubKey(path))
                {
                    if (uninstallKey == null)
                    {
                        return;
                    }

                    var subKeyNames = uninstallKey.GetSubKeyNames();

                    foreach (var subKeyName in subKeyNames)
                    {
                        try
                        {
                            using (var appKey = uninstallKey.OpenSubKey(subKeyName))
                            {
                                if (appKey == null)
                                {
                                    continue;
                                }

                                var software = ReadSoftwareInfo(appKey, subKeyName, path + "\\" + subKeyName);

                                if (software != null && !string.IsNullOrWhiteSpace(software.DisplayName))
                                {
                                    results.Add(software);
                                }
                            }
                        }
                        catch (SecurityException)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OutputFile.LogError($"读取注册表路径 {path} 时出错", ex);
            }
        }

        private static SoftwareInfo ReadSoftwareInfo(RegistryKey key, string name, string registryPath)
        {
            var displayName = key.GetValue("DisplayName") as string;

            if (string.IsNullOrWhiteSpace(displayName))
            {
                return null;
            }

            var systemComponent = key.GetValue("SystemComponent");
            if (systemComponent != null && systemComponent.ToString() == "1")
            {
                return null;
            }

            return new SoftwareInfo
            {
                Name = name,
                DisplayName = displayName,
                Version = key.GetValue("DisplayVersion") as string ?? string.Empty,
                Publisher = key.GetValue("Publisher") as string ?? string.Empty,
                InstallDate = key.GetValue("InstallDate") as string ?? string.Empty,
                InstallLocation = key.GetValue("InstallLocation") as string ?? string.Empty,
                RegistryPath = registryPath,
            };
        }

        public static bool ExportToCsv(List<SoftwareInfo> softwareList, string filePath = null)
        {
            try
            {
                if (softwareList == null || softwareList.Count == 0)
                {
                    OutputFile.LogInfo("没有软件信息可导出");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    filePath = OutputFile.EnsureOutputPath(
                        $"InstalledSoftware_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                        defaultExtension: ".csv");
                }

                var sb = new StringBuilder();

                sb.AppendLine("DisplayName,Version,Publisher,InstallDate,InstallLocation,RegistryPath");

                foreach (var software in softwareList)
                {
                    sb.AppendLine(string.Join(",",
                        EscapeCsvField(software.DisplayName),
                        EscapeCsvField(software.Version),
                        EscapeCsvField(software.Publisher),
                        EscapeCsvField(software.InstallDate),
                        EscapeCsvField(software.InstallLocation),
                        EscapeCsvField(software.RegistryPath)));
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                OutputFile.LogInfo($"软件列表已导出到: {filePath} (共 {softwareList.Count} 项)");
                return true;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("导出软件列表到 CSV 失败", ex);
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
