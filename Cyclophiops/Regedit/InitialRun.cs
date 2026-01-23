using System;
using System.IO;
using System.Text;

namespace Cyclophiops.Regedit
{
    internal class InitialRun
    {
        public static void CreateLogFile(string filePath)
        {
            var curTime = $"{DateTime.Now:yyyy-MM-dd_HH:mm:ss}";
            var sb = new StringBuilder();
            sb.AppendLine($"CreateTime: {curTime}");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
