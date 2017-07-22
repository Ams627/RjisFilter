using System;

namespace RjisFilter.Model
{
    static class RjisUtils
    {
        public static (bool, DateTime) GetRjisDate(string s)
        {
            bool result = DateTime.TryParseExact(s,
                "ddMMyyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var date);
            return (result, date);
        }
    }
}
