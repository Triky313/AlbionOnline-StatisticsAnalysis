namespace StatisticsAnalysisTool.Common
{
    using System.Globalization;

    public class Formatting
    {
        public static string NumberValueWithPointSeparation(int value) => value.ToString("N0", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));

        public static string NumberValueWithPointSeparation(long value) => value.ToString("N0", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));

        public static string NumberValueWithPointSeparation(ulong value) => value.ToString("N0", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));

        public static object NumberValueWithPointSeparation(int? value) => (value != null) ? NumberValueWithPointSeparation(value) : 0 ;

        public static object NumberValueWithPointSeparation(long? value) => (value != null) ? NumberValueWithPointSeparation(value) : 0 ;

        public static object NumberValueWithPointSeparation(ulong? value) => (value != null) ? NumberValueWithPointSeparation(value) : 0 ;
    }
}