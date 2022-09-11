using System;
using System.Globalization;

namespace StatisticsAnalysisTool.Common
{
    public class Formatting
    {
        public static string CurrentDateTimeFormat(DateTime value)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime()
                .ToString("G", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));
        }
    }
}