using System;
using Cyclophiops.Export;
using Cyclophiops.Regedit.Utils;

namespace Cyclophiops.Regedit
{
    internal class GetRegeditValue
    {
        public static bool Get()
        {
            try
            {
                var configs = new[]
                {
                    new ReadRegeditValue.Config(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                        new[] { "EnableLUA", "ConsentPromptBehaviorAdmin", "ConsentPromptBehaviorUser", "PromptOnSecureDesktop" },
                        "UAC Settings"),
                    new ReadRegeditValue.Config(
                        @"SOFTWARE\Microsoft\Cryptography",
                        new[] { "MachineGuid" },
                        "MachineGuid"),
                    new ReadRegeditValue.Config(
                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                        new[] { "ProductName", "CurrentBuild", "DisplayVersion", "RegisteredOwner", "ReleaseId", "BuildLabEx" },
                        "Windows Version Info"),
                    new ReadRegeditValue.Config(
                        @"SYSTEM\CurrentControlSet\Control\TimeZoneInformation",
                        new[] { "TimeZoneKeyName", "Bias" },
                        "Time Zone Info"),
                };
                ReadRegeditValue.ReadMulRegeditToFile(
                    configs);
            }
            catch (Exception ex)
            {
                OutputFile.LogError("注册表获取发送异常", " ", ex);
                return false;
            }

            return true;
        }
    }
}
