using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RjisFilter
{
    static class LinqExtensions
    {
        public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> source)
        {
            return new SortedSet<T>(source);
        }

        public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            return new SortedSet<T>(source, comparer);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(source, comparer);
        }

        public static void RemoveStrings(this List<string> list, string s)
        {
            list.RemoveAll(x => string.Equals(x, s, StringComparison.OrdinalIgnoreCase));
        }
        public static void RemoveRegex(this List<string> list, string pattern)
        {
            list.RemoveAll(x => Regex.Match(x, pattern, RegexOptions.IgnoreCase).Success);
        }
        public static void KeepRegex(this List<string> list, string pattern)
        {
            list.RemoveAll(x => !Regex.Match(x, pattern, RegexOptions.IgnoreCase).Success);
        }
    }
}
