using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Cyclophiops.Regedit
{
    internal class InitialRun
    {
        public static void CreateLogFile(string filePath)
        {
                string curTime = $"{DateTime.Now:yyyy-MM-dd_HH:mm:ss}";
                var sb = new StringBuilder();
                sb.AppendLine($"CreateTime: {curTime}");
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
