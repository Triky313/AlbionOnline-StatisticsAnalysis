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

        public static string DateTimeToLastUpdateTime(DateTime dateTime)
        {
            var startTime = dateTime;
            var endTime = DateTime.UtcNow;
            var minutes = (endTime - startTime).TotalMinutes;
            var hours = (endTime - startTime).TotalHours;
            var days = (endTime - startTime).TotalDays;

            if (minutes <= 120) return $"{minutes:N0} {LanguageController.Translation("MINUTES")}";

            if (hours <= 48) return $"{hours:N0} {LanguageController.Translation("HOURS")}";

            if (days <= 365) return $"{days:N0} {LanguageController.Translation("DAYS")}";

            return $"{LanguageController.Translation("OVER_A_YEAR")}";
        }
    }
}