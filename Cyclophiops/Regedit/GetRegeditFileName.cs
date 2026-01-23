using System;
using Cyclophiops.Export;
using Microsoft.Win32;

namespace Cyclophiops.Regedit
{
    internal class GetRegeditFileName
    {
        public static bool Get()
        {
            try
            {
                OutputFile.LogInfo("开始枚举注册表...");
                var result = RegistryEnumerator.Enumerate(
                    path: string.Empty,
                    hive: RegistryHive.Users,
                    view: RegistryView.Registry64,
                    filter: null,
                    recursive: true,
                    maxDepth: 3);

                if (result == null)
                {
                    OutputFile.LogError("枚举结果为 null");
                    return false;
                }

                OutputFile.LogInfo($"枚举完成 - Success: {result.Success}, Total: {result.TotalCount}, Filtered: {result.FilteredCount}, Folders: {result.Folders.Count}");

                if (!result.Success)
                {
                    OutputFile.LogError($"枚举失败: {result.ErrorMessage}");
                    return false;
                }

                if (result.Folders.Count == 0)
                {
                    OutputFile.LogInfo("警告: 没有找到任何子项");
                    return true;
                }

                OutputFile.LogInfo("开始导出到文件...");
                var exportResult = RegistryEnumerator.ExportToFile(result);

                if (exportResult)
                {
                    OutputFile.LogInfo("导出成功");
                }
                else
                {
                    OutputFile.LogError("导出失败");
                }

                return exportResult;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("执行过程中发生异常", " ", ex);
                return false;
            }
        }
    }
}
