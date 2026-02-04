using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Cyclophiops.Export;

namespace Cyclophiops.Detail.Browser
{
    internal class BookmarksParser
    {
        public class BookmarkItem
        {
            public string Name { get; set; }

            public string Url { get; set; }

            public DateTime? DateAdded { get; set; }

            public string Folder { get; set; }
        }

        public static bool ExportToHtml()
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

                var json = File.ReadAllText(defaultBookmarks, Encoding.UTF8);
                var bookmarks = ParseBookmarks(json);

                var outputDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "log", "BrowserBookmarks");
                Directory.CreateDirectory(outputDir);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var htmlFile = Path.Combine(outputDir, $"Edge_Bookmarks_{timestamp}.html");
                var csvFile = Path.Combine(outputDir, $"Edge_Bookmarks_{timestamp}.csv");

                ExportToHtmlFile(bookmarks, htmlFile);
                ExportToCsvFile(bookmarks, csvFile);

                OutputFile.LogInfo($"Edge Bookmarks exported to HTML: {htmlFile}");
                OutputFile.LogInfo($"Edge Bookmarks exported to CSV: {csvFile}");
                OutputFile.LogInfo($"Total bookmarks: {bookmarks.Count}");

                return true;
            }
            catch (Exception ex)
            {
                OutputFile.LogError("Failed to export Edge Bookmarks", ex);
                return false;
            }
        }

        private static List<BookmarkItem> ParseBookmarks(string json)
        {
            var bookmarks = new List<BookmarkItem>();
            ParseBookmarksRecursive(json, bookmarks, string.Empty);
            return bookmarks;
        }

        private static void ParseBookmarksRecursive(string json, List<BookmarkItem> bookmarks, string currentFolder)
        {
            var urlPattern = @"""type"":\s*""url"",\s*""name"":\s*""([^""]*)"",\s*""url"":\s*""([^""]*)""";
            var folderPattern = @"""type"":\s*""folder"",\s*""name"":\s*""([^""]*)""";

            var urlMatches = Regex.Matches(json, urlPattern);
            foreach (Match match in urlMatches)
            {
                var name = UnescapeJson(match.Groups[1].Value);
                var url = UnescapeJson(match.Groups[2].Value);

                bookmarks.Add(new BookmarkItem
                {
                    Name = name,
                    Url = url,
                    Folder = currentFolder,
                });
            }

            var folderMatches = Regex.Matches(json, folderPattern);
            foreach (Match match in folderMatches)
            {
                var folderName = UnescapeJson(match.Groups[1].Value);
                if (!string.IsNullOrEmpty(folderName) && folderName != "Bookmarks Bar" && folderName != "Other Bookmarks")
                {
                }
            }
        }

        private static string UnescapeJson(string text)
        {
            return text.Replace("\\\"", "\"")
                       .Replace("\\\\", "\\")
                       .Replace("\\/", "/")
                       .Replace("\\n", "\n")
                       .Replace("\\r", "\r")
                       .Replace("\\t", "\t");
        }

        private static void ExportToHtmlFile(List<BookmarkItem> bookmarks, string filePath)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE NETSCAPE-Bookmark-file-1>");
            html.AppendLine("<!-- This is an automatically generated file.");
            html.AppendLine("     It will be read and overwritten.");
            html.AppendLine("     DO NOT EDIT! -->");
            html.AppendLine("<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=UTF-8\">");
            html.AppendLine("<TITLE>Bookmarks</TITLE>");
            html.AppendLine("<H1>Bookmarks</H1>");
            html.AppendLine("<DL><p>");

            foreach (var bookmark in bookmarks)
            {
                html.AppendLine($"    <DT><A HREF=\"{HtmlEscape(bookmark.Url)}\">{HtmlEscape(bookmark.Name)}</A>");
            }

            html.AppendLine("</DL><p>");

            File.WriteAllText(filePath, html.ToString(), Encoding.UTF8);
        }

        private static void ExportToCsvFile(List<BookmarkItem> bookmarks, string filePath)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Name,URL,Folder");

            foreach (var bookmark in bookmarks)
            {
                csv.AppendLine($"{CsvEscape(bookmark.Name)},{CsvEscape(bookmark.Url)},{CsvEscape(bookmark.Folder)}");
            }

            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }

        private static string HtmlEscape(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#39;");
        }

        private static string CsvEscape(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }
    }
}
