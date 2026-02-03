using System;
using Cyclophiops.Export;
using Cyclophiops.Regedit.Utils;

namespace Cyclophiops.Regedit
{
    internal class GetUserSoftwareDetail
    {
        public static bool Get()
        {
            try
            {
                OutputFile.LogInfo("开始读取已安装软件列表...");

                var softwareList = SoftwareReader.GetInstalledSoftware();

                if (softwareList.Count == 0)
                {
                    OutputFile.LogInfo("未找到任何已安装软件");
                    return true;
                }

                OutputFile.LogInfo($"找到 {softwareList.Count} 个已安装软件");

                var success = SoftwareReader.ExportToCsv(softwareList);

                if (success)
                {
                    OutputFile.LogInfo("软件列表导出成功");
                }

                return success;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("读取软件列表时发生异常", ex);
                return false;
            }
        }
    }
}
