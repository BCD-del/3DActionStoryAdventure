// Copyright © Magnetic Arcade. All Rights Reserved.

namespace MA.Core
{
    /// <summary>String utilities.</summary>
    public static class StringUtility
    {
        /// <summary>Formats a number of bytes into a human readable string. (B, KB, MB, GB)
        /// <para>Example: 1024 -> 1 KB</para></summary>
        public static string FormatBytes(long bytes)
        {
            const int kb = 1024;
            const int mb = kb * 1024;
            const int gb = mb * 1024;

            return bytes switch
            {
                >= gb => $"{bytes / (double)gb:F2} GB",
                >= mb => $"{bytes / (double)mb:F2} MB",
                >= kb => $"{bytes / (double)kb:F2} KB",
                _     => $"{bytes} B"
            };
        }

        /// <summary>Formats a large number into a human readable string. (K, M, B)
        /// <para>Example: 1000 -> 1 K</para></summary>
        public static string FormatLargeNumber(long num)
        {
            const int k = 1000;
            const int m = k * 1000;
            const int b = m * 1000;

            return num switch
            {
                >= b => $"{num / (double)b:F2} B",
                >= m => $"{num / (double)m:F2} M",
                >= k => $"{num / (double)k:F2} K",
                _    => $"{num}"
            };
        }
    }
}