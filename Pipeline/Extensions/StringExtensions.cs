namespace Pipeline.Extensions {
    public static class StringExtensions {
        public static string Left(this string s, int length) {
            return s.Length > length ? s.Substring(0, length) : s;
        }

        public static string Right(this string s, int length) {
            return s.Length > length ? s.Substring(s.Length - length, length) : s;
        }

        public static bool IsNumeric(this string theValue) {
            double retNum;
            return double.TryParse(theValue, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
        }
    }
}