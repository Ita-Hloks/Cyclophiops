using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cyclophiops.Regedit
{
    internal class GetRegeditValue
    {
        public static void Get()
        {
            string logName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
            string logPath = $@"D:/{logName}";
            try
            {
                var configs = new[]
                {
                    new RegistryReadConfig(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                        new[] { "EnableLUA", "ConsentPromptBehaviorAdmin", "ConsentPromptBehaviorUser", "PromptOnSecureDesktop" },
                        "UAC Settings"
                    ),
                    new RegistryReadConfig(
                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                        new[] { "ProductName", "CurrentBuild", "DisplayVersion", "RegisteredOwner", "ReleaseId", "BuildLabEx" },
                        "Windows Version Info"
                    ),
                    new RegistryReadConfig(
                        @"SYSTEM\CurrentControlSet\Control\TimeZoneInformation",
                        new[] { "TimeZoneKeyName", "Bias" },
                        "Time Zone Info"
                    )
                };
                RegistryReaderUtil.ReadMultipleRegistriesToFile(
                    configs,
                    logPath
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"错误: {ex.Message}");
            }
        }
    }
}
