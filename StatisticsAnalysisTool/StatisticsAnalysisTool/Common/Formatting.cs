using System;

namespace StatisticsAnalysisTool.Common
{
    using System.Globalization;

    public class Formatting
    {
        public static string CurrentDateTimeFormat(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime().ToString("G", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));

        public static string ToStringShort(double num)
        {
            if (num < 1000)
            {
                return num.ToString("N2", CultureInfo.CurrentCulture);
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