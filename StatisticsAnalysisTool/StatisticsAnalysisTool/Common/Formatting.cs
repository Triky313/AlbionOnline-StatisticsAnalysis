using System;

namespace StatisticsAnalysisTool.Common
{
    using System.Globalization;

    public class Formatting
    {
        public static string CurrentDateTimeFormat(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime().ToString("G", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));

        public static string DateTimeToLastUpdateTime(DateTime dateTime)
        {
            var startTime = dateTime;
            var endTime = DateTime.UtcNow;
            var minutes = (endTime - startTime).TotalMinutes;
            var hours = (endTime - startTime).TotalHours;
            var days = (endTime - startTime).TotalDays;

            if (minutes <= 120)
            {
                return $"{minutes:N0} {LanguageController.Translation("MINUTES")}";
            }

            if (hours <= 48)
            {
                return $"{hours:N0} {LanguageController.Translation("HOURS")}";
            }

            if (days <= 365)
            {
                return $"{days:N0} {LanguageController.Translation("DAYS")}";
            }

            return $"{LanguageController.Translation("OVER_A_YEAR")}";
        }

        public static string ToStringShort(double num)
        {
            if (num < -10000000)
            {
                num /= 10000;
                return (num / 100f).ToString("#.00'M'", CultureInfo.CurrentCulture);
            }

            if (num < -1000000)
            {
                num /= 100;
                return (num / 10f).ToString("#.00'K'", CultureInfo.CurrentCulture);
            }

            if (num < -10000)
            {
                num /= 10;
                return (num / 100f).ToString("#.00'K'", CultureInfo.CurrentCulture);
            }

            if (num < 1000)
            {
                return num.ToString("N0", CultureInfo.CurrentCulture);
            }

            if (num < 10000)
            {
                num /= 10;
                return (num / 100f).ToString("#.00'K'", CultureInfo.CurrentCulture);
            }

            if (num < 1000000)
            {
                num /= 100;
                return (num / 10f).ToString("#.00'K'", CultureInfo.CurrentCulture);
            }

            if (num < 10000000)
            {
                num /= 10000;
                return (num / 100f).ToString("#.00'M'", CultureInfo.CurrentCulture);
            }

            num /= 100000;
            return (num / 10f).ToString("#.00'M'", CultureInfo.CurrentCulture);
        }
    }
}