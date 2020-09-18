using System;

namespace StatisticsAnalysisTool.Common
{
    using System.Globalization;

    public class Formatting
    {
        public static string CurrentDateTimeFormat(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime().ToString("G", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));
    }
}