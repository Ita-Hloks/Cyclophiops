using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management;
using System.Text;
using Cyclophiops.Export;

namespace Cyclophiops.Hardware
{
    public static class GetDeviceInfo
    {
        private const int WmiTimeoutSeconds = 10;

        public static bool Export(string userPath)
        {
            try
            {
                var filePath = OutputFile.EnsureOutputPath(
                    path: userPath,
                    defaultPathProvider: () => $"D:\\log\\{DateTime.Now:yyyy-MM-dd_HHmmss}_device_info.csv",
                    defaultExtension: ".csv");

                var csv = CollectAsCsv();
                File.WriteAllText(filePath, csv, Encoding.UTF8);
                OutputFile.LogInfo($"设备信息已导出: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("导出设备信息失败", " ", ex);
                return false;
            }
        }

        private static string CollectAsCsv()
        {
            var sb = new StringBuilder(16 * 1024);
            sb.AppendLine("Category,Key,Value");

            AppendSingle(sb, "Computer", "Win32_ComputerSystem",
                new[] { "Manufacturer", "Model", "SystemType", "Domain", "TotalPhysicalMemory" });

            AppendSingle(sb, "OS", "Win32_OperatingSystem",
                new[] { "Caption", "Version", "BuildNumber", "OSArchitecture", "LastBootUpTime" });

            AppendMulti(sb, "CPU", "Win32_Processor",
                new[] { "Name", "Manufacturer", "NumberOfCores", "NumberOfLogicalProcessors", "MaxClockSpeed", "ProcessorId" });

            AppendSingle(sb, "BIOS", "Win32_BIOS",
                new[] { "Manufacturer", "SMBIOSBIOSVersion", "SerialNumber", "ReleaseDate" });

            AppendSingle(sb, "BaseBoard", "Win32_BaseBoard",
                new[] { "Manufacturer", "Product", "SerialNumber", "Version" });

            AppendMulti(sb, "Memory", "Win32_PhysicalMemory",
                new[] { "Manufacturer", "PartNumber", "SerialNumber", "Capacity", "Speed", "MemoryType" });

            AppendMulti(sb, "Disk", "Win32_DiskDrive",
                new[] { "Model", "SerialNumber", "Size", "InterfaceType", "MediaType" });

            AppendMulti(sb, "GPU", "Win32_VideoController",
                new[] { "Name", "DriverVersion", "AdapterRAM" });

            AppendMulti(sb, "Network", "Win32_NetworkAdapterConfiguration",
                new[] { "Description", "MACAddress", "IPAddress", "IPSubnet", "DefaultIPGateway", "DNSServerSearchOrder" },
                "IPEnabled = True");

            return sb.ToString();
        }

        private static void AppendSingle(StringBuilder sb, string category, string wmiClass, string[] fields, string where = null)
        {
            try
            {
                using (var searcher = CreateSearcher(wmiClass, fields, where))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        AppendFields(sb, category, fields, mo, 0);
                        break;
                    }
                }
            }
            catch (ManagementException ex)
            {
                OutputFile.LogError($"WMI 查询失败 [{wmiClass}]", " ", ex);
                sb.AppendLine($"{category},ERROR,{ex.Message}");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                OutputFile.LogError($"WMI 查询超时或失败 [{wmiClass}]", " ", ex);
                sb.AppendLine($"{category},TIMEOUT,查询超时({WmiTimeoutSeconds}秒)");
            }
        }

        private static void AppendMulti(StringBuilder sb, string category, string wmiClass, string[] fields, string where = null)
        {
            try
            {
                var idx = 0;
                using (var searcher = CreateSearcher(wmiClass, fields, where))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        AppendFields(sb, category, fields, mo, idx);
                        idx++;
                    }
                }
            }
            catch (ManagementException ex)
            {
                OutputFile.LogError($"WMI 查询失败 [{wmiClass}]", " ", ex);
                sb.AppendLine($"{category},ERROR,{ex.Message}");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                OutputFile.LogError($"WMI 查询超时或失败 [{wmiClass}]", " ", ex);
                sb.AppendLine($"{category},TIMEOUT,查询超时({WmiTimeoutSeconds}秒)");
            }
        }

        private static ManagementObjectSearcher CreateSearcher(string wmiClass, string[] fields, string where)
        {
            var query = "SELECT " + string.Join(",", fields) + " FROM " + wmiClass;
            if (!string.IsNullOrWhiteSpace(where))
            {
                query += " WHERE " + where;
            }

            return new ManagementObjectSearcher(new ObjectQuery(query))
            {
                Options = new EnumerationOptions
                {
                    ReturnImmediately = true,
                    Rewindable = false,
                    Timeout = new TimeSpan(0, 0, WmiTimeoutSeconds),
                },
            };
        }

        private static void AppendFields(StringBuilder sb, string category, string[] fields, ManagementObject mo, int instanceIndex)
        {
            var suffix = instanceIndex > 0 ? "[" + instanceIndex + "]" : string.Empty;

            foreach (var f in fields)
            {
                var key = f + suffix;
                var value = GetWmiValueAsString(mo, f);
                AppendRow(sb, category, key, value);
            }
        }

        private static string GetWmiValueAsString(ManagementObject mo, string propName)
        {
            try
            {
                var val = mo[propName];
                if (val == null)
                {
                    return string.Empty;
                }

                var arr = val as Array;
                if (arr != null)
                {
                    var parts = new List<string>();
                    foreach (var item in arr)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        parts.Add(Convert.ToString(item, CultureInfo.InvariantCulture));
                    }

                    return string.Join(";", parts);
                }

                var s = Convert.ToString(val, CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(s) && LooksLikeDmtfDatetime(s))
                {
                    try
                    {
                        var dt = ManagementDateTimeConverter.ToDateTime(s);
                        return dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                    }
                }

                if (IsMemoryField(propName) && long.TryParse(s, out var bytes))
                {
                    return FormatMemorySize(bytes);
                }

                return s;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool IsMemoryField(string fieldName)
        {
            return fieldName == "TotalPhysicalMemory" ||
                   fieldName == "Capacity" ||
                   fieldName == "AdapterRAM" ||
                   fieldName == "Size";
        }

        private static string FormatMemorySize(long bytes)
        {
            return UnitConverter.FormatBytes(bytes, decimals: 2, showOriginal: true);
        }

        private static bool LooksLikeDmtfDatetime(string s)
        {
            return s.Length >= 14 && s.IndexOf('.') > 0;
        }

        private static void AppendRow(StringBuilder sb, string category, string key, string value)
        {
            sb.Append(EscapeCsv(category)).Append(",")
              .Append(EscapeCsv(key)).Append(",")
              .Append(EscapeCsv(value)).AppendLine();
        }

        private static string EscapeCsv(string s)
        {
            if (s == null)
            {
                return string.Empty;
            }

            var quote = false;
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c == ',' || c == '"' || c == '\r' || c == '\n')
                {
                    quote = true;
                    break;
                }
            }

            if (!quote)
            {
                return s;
            }

            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
    }
}
