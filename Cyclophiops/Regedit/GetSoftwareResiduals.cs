using System;
using Cyclophiops.Export;
using Cyclophiops.Regedit.Utils;

namespace Cyclophiops.Regedit
{
    internal class GetSoftwareResiduals
    {
        public static bool Get()
        {
            try
            {
                OutputFile.LogInfo("开始扫描软件残骸...");

                var residuals = SoftwareResidualReader.ScanSoftwareResiduals();

                if (residuals.Count == 0)
                {
                    OutputFile.LogInfo("未找到任何软件残骸");
                    return true;
                }

                var success = SoftwareResidualReader.ExportToCsv(residuals);

                if (success)
                {
                    OutputFile.LogInfo("软件残骸扫描完成");
                }

                return success;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("扫描软件残骸时发生异常", ex);
                return false;
            }
        }
    }
}
