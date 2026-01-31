using System;
using System.IO;
using Cyclophiops.Export;

namespace Cyclophiops.Detail.Browser
{
    internal class GetHistory
    {
        public static bool Get()
        {
            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var edgeUserData = Path.Combine(localAppData, "Microsoft", "Edge", "User Data");
                var defaultHistory = Path.Combine(edgeUserData, "Default", "History");

                if (!File.Exists(defaultHistory))
                {
                    OutputFile.LogError("EdgeHistory文件不存在", new FileNotFoundException($"未找到文件: {defaultHistory}"));
                    return false;
                }

                var outputDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "log");
                Directory.CreateDirectory(outputDir);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var outputFile = Path.Combine(outputDir, $"Edge_History_{timestamp}");

                File.Copy(defaultHistory, outputFile, true);

                OutputFile.LogInfo($"EdgeHistory已导出到: {outputFile}");
                return true;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("导出EdgeHistory失败", ex);
                return false;
            }
        }
    }
}
