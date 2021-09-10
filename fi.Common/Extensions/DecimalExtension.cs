using System;
using System.Globalization;

namespace fi.Common
{
    public static class DecimalExtension
    {
        public static decimal Percent(this decimal baseValue, decimal value) => value.Equals(0) ? 0 : Math.Floor((baseValue - value) / value * 100);
        public static string ToPointString(this decimal point, string stringFormat = null) => point.ToString(stringFormat, new NumberFormatInfo { NumberDecimalSeparator = "." });
        public static string ToPointString(this decimal? point, string stringFormat = null)
        {
            if (!point.HasValue)
                return string.Empty;

            return ToPointString(point.Value, stringFormat);
        }
    }
}
