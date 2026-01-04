using System;

namespace Polymarket.ClobClient.Utilities
{
    public static class MathUtils
    {
        public static decimal RoundDown(decimal value, int decimals)
        {
            var factor = (decimal)Math.Pow(10, decimals);
            return Math.Floor(value * factor) / factor;
        }

        public static decimal RoundUp(decimal value, int decimals)
        {
            var factor = (decimal)Math.Pow(10, decimals);
            return Math.Ceiling(value * factor) / factor;
        }

        public static decimal RoundNormal(decimal value, int decimals)
        {
            return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
        }

        public static int DecimalPlaces(decimal value)
        {
            // Convert to string and split.
            // A more robust way might be needed but this mimics simple usage.
            var s = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var parts = s.Split('.');
            return parts.Length > 1 ? parts[1].Length : 0;
        }
    }
}
