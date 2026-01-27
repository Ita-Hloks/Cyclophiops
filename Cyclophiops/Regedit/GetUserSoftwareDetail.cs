using System;
using System.Collections.Generic;
using Cyclophiops.Export;
using Cyclophiops.Regedit.Utils;
using Microsoft.Win32;

namespace Cyclophiops.Regedit
{
    internal class GetUserSoftwareDetail
    {
        public static bool Get()
        {
            try
            {
                OutputFile.LogInfo("开始枚举用户 SID...");

                var userSids = SidHelper.GetAllUserSids();
                OutputFile.LogInfo($"找到 {userSids.Count} 个用户 SID");

                var allResults = new List<ReadRegeditList.EnumerateResult>();

                foreach (var sid in userSids)
                {
                    OutputFile.LogInfo($"正在处理 SID: {sid}");

                    var result = ReadRegeditList.Enumerate(
                        path: $"{sid}\\SOFTWARE",
                        hive: RegistryHive.Users,
                        view: RegistryView.Registry64,
                        filter: null,
                        maxDepth: 2);

                    if (result.Success && result.Folders.Count > 0)
                    {
                        OutputFile.LogInfo($"  找到 {result.FilteredCount} 个项");
                        allResults.Add(result);
                    }
                }

                if (allResults.Count == 0)
                {
                    OutputFile.LogInfo("警告: 没有找到任何子项");
                    return true;
                }

                OutputFile.LogInfo($"总共处理了 {allResults.Count} 个用户配置");

                foreach (var result in allResults)
                {
                    ReadRegeditList.ExportToFile(result);
                }

                OutputFile.LogInfo("导出成功");
                return true;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("执行过程中发生异常", " ", ex);
                return false;
            }
        }
    }
}
