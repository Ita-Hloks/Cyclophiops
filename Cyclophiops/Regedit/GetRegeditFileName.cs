using Microsoft.Win32;
using System;
using Cyclophiops.Regedit;

namespace Cyclophiops.Regedit
{
    internal class GetRegeditFileName
    {
        public static bool Get()
        {
            try
            {
                var config = new RegistryEnumerateConfig(
                    @"",
                    "USERS-SOFTWARE 注册表树",
                    RegistryHive.Users,
                    RegistryView.Registry64,
                    null,
                    new EnumerateOptions { Recursive = true, MaxDepth = 3 }
                );

                RegistryConfig.LogInfo("开始枚举注册表...");
                var result = RegistryEnumerator.Enumerate(config);

                if (result == null)
                {
                    RegistryConfig.LogError("枚举结果为 null");
                    return false;
                }

                RegistryConfig.LogInfo($"枚举完成 - Success: {result.Success}, Total: {result.TotalCount}, Filtered: {result.FilteredCount}, Folders: {result.Folders.Count}");

                if (!result.Success)
                {
                    RegistryConfig.LogError($"枚举失败: {result.ErrorMessage}");
                    return false;
                }

                if (result.Folders.Count == 0)
                {
                    RegistryConfig.LogInfo("警告: 没有找到任何子项");
                    return true;
                }

                RegistryConfig.LogInfo("开始导出到文件...");
                bool exportResult = RegistryEnumerator.ExportToFile(result);

                if (exportResult)
                    RegistryConfig.LogInfo("导出成功");
                else
                    RegistryConfig.LogError("导出失败");

                return exportResult;
            }
            catch (Exception ex)
            {
                RegistryConfig.LogError("执行过程中发生异常", ex);
                return false;
            }
        }
    }
}
