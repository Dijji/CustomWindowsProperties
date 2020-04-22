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

        public static bool IsValidPropertyName(string name)
        {
            return Regex.IsMatch(name, @"\A([A-Z]([A-Z]|[a-z]|[0-9])*\.)+([A-Z]([A-Z]|[a-z]|[0-9])*)\z");
        }

        // Ensure that there are no illegal characters in a filename
        public static string FixFileName(string fileName)
        {
            return Regex.Replace(fileName, @"[\/?:*""><|]+", "_", RegexOptions.Compiled);
        }
    }
}
