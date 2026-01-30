using System;

namespace Cyclophiops.Export
{
    public static class UnitConverter
    {
        public static string FormatBytes(long bytes, int decimals = 2, bool showOriginal = true)
        {
            return FormatUnit(bytes, 1024, new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" }, decimals, showOriginal);
        }

        public static string FormatBytesAuto(long bytes)
        {
            return FormatBytes(bytes, decimals: -1, showOriginal: false);
        }

        public static string FormatSpeed(long bytesPerSecond, bool showOriginal = false)
        {
            var formatted = FormatBytes(bytesPerSecond, 2, false);
            return showOriginal
                ? string.Format("{0} ({1}/s)", bytesPerSecond, formatted)
                : formatted + "/s";
        }

        public static string FormatFrequency(long hz, int decimals = 2, bool showOriginal = true)
        {
            return FormatUnit(hz, 1000, new[] { "Hz", "kHz", "MHz", "GHz", "THz" }, decimals, showOriginal);
        }

        public static (double Value, string Unit) ParseBytes(long bytes)
        {
            return ParseUnit(bytes, 1024, new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" });
        }

        private static string FormatUnit(long value, int divisor, string[] units, int decimals, bool showOriginal)
        {
            if (value < 0)
            {
                return value.ToString();
            }

            if (value == 0)
            {
                return showOriginal ? "0 (0 " + units[0] + ")" : "0 " + units[0];
            }

            var unitIndex = 0;
            var size = (double)value;

            while (size >= divisor && unitIndex < units.Length - 1)
            {
                size /= divisor;
                unitIndex++;
            }

            var actualDecimals = decimals == -1 ? (unitIndex == 0 ? 0 : 2) : decimals;
            var roundedSize = Math.Round(size, actualDecimals);
            var formatted = roundedSize.ToString("F" + actualDecimals);

            if (showOriginal && value >= divisor)
            {
                return string.Format("{0} ({1} {2})", value, formatted, units[unitIndex]);
            }

            return string.Format("{0} {1}", formatted, units[unitIndex]);
        }

        private static (double Value, string Unit) ParseUnit(long value, int divisor, string[] units)
        {
            if (value <= 0)
            {
                return (value, units[0]);
            }

            var unitIndex = 0;
            var size = (double)value;

            while (size >= divisor && unitIndex < units.Length - 1)
            {
                size /= divisor;
                unitIndex++;
            }

            return (Math.Round(size, 2), units[unitIndex]);
        }
    }
}
