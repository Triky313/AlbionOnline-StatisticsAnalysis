using Serilog;
using System;
using System.Globalization;

namespace StatisticsAnalysisTool.Localization;

internal static class LocalizedTextFormatter
{
    internal static string Format(string key, string translation, string fallback, CultureInfo culture, params object[] arguments)
    {
        string[] templates = [translation, fallback];
        foreach (var template in templates)
        {
            if (string.IsNullOrEmpty(template))
            {
                continue;
            }

            try
            {
                return string.Format(culture, template, arguments);
            }
            catch (FormatException exception)
            {
                Log.Warning(exception, "Invalid translation format for {TranslationKey}. Template: {Template}", key, template);
            }
        }

        return key;
    }
}
