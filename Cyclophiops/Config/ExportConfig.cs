using System;
using System.Configuration;
using System.IO;

namespace Cyclophiops.Config
{
    public static class ExportConfig
    {
        private static readonly string _defaultPath = AppDomain.CurrentDomain.BaseDirectory;
        private const string ExportPathKey = "ExportPath";

        public static string GetExportPath()
        {
            try
            {
                var config = ConfigurationManager.AppSettings[ExportPathKey];
                if (!string.IsNullOrEmpty(config) && Directory.Exists(config))
                {
                    return config;
                }
            }
            catch
            {
            }

            return _defaultPath;
        }

        public static void SetExportPath(string path)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;

                if (string.IsNullOrEmpty(path))
                {
                    path = _defaultPath;
                }

                if (settings[ExportPathKey] == null)
                {
                    settings.Add(ExportPathKey, path);
                }
                else
                {
                    settings[ExportPathKey].Value = path;
                }

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot save config: " + ex.Message, ex);
            }
        }
    }
}
