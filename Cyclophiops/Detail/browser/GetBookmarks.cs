using System;
using System.IO;
using Cyclophiops.Export;

namespace Cyclophiops.Detail.Browser
{
    internal class GetBookmarks
    {
        public static bool Get()
        {
            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var edgeUserData = Path.Combine(localAppData, "Microsoft", "Edge", "User Data");
                var defaultBookmarks = Path.Combine(edgeUserData, "Default", "Bookmarks");

                if (!File.Exists(defaultBookmarks))
                {
                    OutputFile.LogError("Edge Bookmarks file not found", new FileNotFoundException($"File not found: {defaultBookmarks}"));
                    return false;
                }

                var outputDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "log");
                Directory.CreateDirectory(outputDir);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var outputFile = Path.Combine(outputDir, $"Edge_Bookmarks_{timestamp}.json");

                File.Copy(defaultBookmarks, outputFile, true);

                OutputFile.LogInfo($"Edge Bookmarks exported to: {outputFile}");
                return true;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("Failed to export Edge Bookmarks", ex);
                return false;
            }
        }
    }
}
