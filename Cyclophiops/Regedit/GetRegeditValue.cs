using System;
using Cyclophiops.Export;

namespace Cyclophiops.Regedit
{
    internal class GetRegeditValue
    {
        public static void Get()
        {
            _ = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
            try
            {
                var configs = new[]
                {
                    new RegistryReadConfig(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                        new[] { "EnableLUA", "ConsentPromptBehaviorAdmin", "ConsentPromptBehaviorUser", "PromptOnSecureDesktop" },
                        "UAC Settings"),
                    new RegistryReadConfig(
                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                        new[] { "ProductName", "CurrentBuild", "DisplayVersion", "RegisteredOwner", "ReleaseId", "BuildLabEx" },
                        "Windows Version Info"),
                    new RegistryReadConfig(
                        @"SYSTEM\CurrentControlSet\Control\TimeZoneInformation",
                        new[] { "TimeZoneKeyName", "Bias" },
                        "Time Zone Info"),
                };
                RegistryReaderUtil.ReadMultipleRegistriesToFile(
                    configs);
            }
            catch (Exception ex)
            {
                OutputFile.LogError("注册表获取发送异常", " ", ex);
            }
        }
    }
}
