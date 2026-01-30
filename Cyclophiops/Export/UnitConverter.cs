using System;

namespace Cyclophiops.Export
{
    public static class UnitConverter
    {
        public static string FormatBytes(long bytes, int decimals = 2, bool showOriginal = true)
        {
            if (bytes < 0)
            {
                return bytes.ToString();
            }

            if (bytes == 0)
            {
                return showOriginal ? "0 (0 B)" : "0 B";
            }

            var units = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            var unitIndex = 0;
            var size = (double)bytes;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            var roundedSize = Math.Round(size, decimals);
            var formatted = roundedSize.ToString("F" + decimals);

            if (showOriginal && bytes >= 1024)
            {
                return string.Format("{0} ({1} {2})", bytes, formatted, units[unitIndex]);
            }

            return string.Format("{0} {1}", formatted, units[unitIndex]);
        }

        public static string FormatBytesAuto(long bytes)
        {
            if (bytes < 0)
            {
                return bytes.ToString();
            }

            if (bytes == 0)
            {
                return "0 B";
            }

            var units = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            var unitIndex = 0;
            var size = (double)bytes;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            var dec = unitIndex == 0 ? 0 : 2;
            var roundedSize = Math.Round(size, dec);
            return roundedSize.ToString("F" + dec) + " " + units[unitIndex];
        }

        public static string FormatSpeed(long bytesPerSecond, bool showOriginal = false)
        {
            var formatted = FormatBytes(bytesPerSecond, 2, false);
            if (showOriginal)
            {
                return string.Format("{0} ({1}/s)", bytesPerSecond, formatted);
            }

            return formatted + "/s";
        }

        public static string FormatFrequency(long hz, int decimals = 2, bool showOriginal = true)
        {
            if (hz < 0)
            {
                return hz.ToString();
            }

            if (hz == 0)
            {
                return showOriginal ? "0 (0 Hz)" : "0 Hz";
            }

            var units = new[] { "Hz", "KHz", "MHz", "GHz", "THz" };
            var unitIndex = 0;
            var freq = (double)hz;

            while (freq >= 1000 && unitIndex < units.Length - 1)
            {
                freq /= 1000;
                unitIndex++;
            }

            var roundedFreq = Math.Round(freq, decimals);
            var formatted = roundedFreq.ToString("F" + decimals);

            if (showOriginal && hz >= 1000)
            {
                return string.Format("{0} ({1} {2})", hz, formatted, units[unitIndex]);
            }

            return string.Format("{0} {1}", formatted, units[unitIndex]);
        }

        public static (double Value, string Unit) ParseBytes(long bytes)
        {
            if (bytes == 0)
            {
                return (0, "B");
            }

            var units = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            var unitIndex = 0;
            var size = (double)bytes;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return (Math.Round(size, 2), units[unitIndex]);
        }
    }
}
