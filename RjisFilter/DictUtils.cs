using System.Collections.Generic;
using System.Linq;

namespace RjisFilter
{
    class DictUtils
    {
        public static void AddEntry<T, U>(Dictionary<T, List<U>> d, T key, U listEntry)
        {
            if (!d.TryGetValue(key, out var list))
            {
                list = new List<U>();
                d.Add(key, list);
            }
            list.Add(listEntry);
        }

        public static IEnumerable<string> GetResults(Dictionary<string, List<string>> dict, string key)
        {
            dict.TryGetValue(key, out var result);
            return result ?? Enumerable.Empty<string>();
        }

        public static string GetResult(Dictionary<string, string> dict, string key)
        {
            dict.TryGetValue(key, out var result);
            return result ?? string.Empty;
        }

    }
}
