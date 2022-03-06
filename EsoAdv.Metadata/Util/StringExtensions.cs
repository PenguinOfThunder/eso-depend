namespace EsoAdv.Metadata.Util
{
    using System.Globalization;
    public static class StringExtensions
    {
        /// <summary>Try to parse a string as an integer and return default value if invalid</summary>
        public static int? SafeParseInt(this string value, int? defaultValue = null) =>
         value != null && int.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int intval)
            ? intval
            : defaultValue;
    }
}