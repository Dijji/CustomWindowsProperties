using System;
using System.Collections.Generic;
using System.Linq;
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

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f) =>
                e.SelectMany(c => f(c).Flatten(f)).Concat(e);

        public static bool IsValidPropertyName(string name)
        {
            return name != null &&
                Regex.IsMatch(name, @"\A([A-Z]([A-Z]|[a-z]|[0-9])*\.)+([A-Z]([A-Z]|[a-z]|[0-9])*)\z");
        }

        // Ensure that there are no illegal characters in a filename
        public static string FixFileName(string fileName)
        {
            return Regex.Replace(fileName, @"[\/?:*""><|]+", "_", RegexOptions.Compiled);
        }
    }
}
