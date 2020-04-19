using System.Text.RegularExpressions;

namespace CustomWindowsProperties
{
    static class Extensions
    {
        public static string First(this string s, int length)
        {
            if (s.Length < length)
                return s;
            else
                return s.Substring(0, length);
        }

        // Ensure that there are no illegal characters in a filename
        public static string FixFileName(string fileName)
        {
            return Regex.Replace(fileName, @"[\/?:*""><|]+", "_", RegexOptions.Compiled);
        }
    }
}
