using System.Globalization;

namespace EsoAdv.Metadata.Util;

public static class StringExtensions
{
    /// <summary>Try to parse a string as an integer and return default value if invalid</summary>
    public static int? SafeParseInt(this string value, int? defaultValue = null)
    {
        return value != null &&
               int.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var intval)
            ? intval
            : defaultValue;
    }
}