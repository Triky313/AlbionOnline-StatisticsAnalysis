using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StatisticsAnalysisTool.Models;

public static class MobDisplayNameFormatter
{
    private static readonly Regex WordBoundaryRegex = new("(?<=[a-z])(?=[A-Z])", RegexOptions.Compiled);

    public static string Humanize(string value, bool preserveEmpty = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return preserveEmpty ? "—" : string.Empty;
        }

        var normalized = value
            .Replace("_", " ", StringComparison.Ordinal)
            .Replace("miniboss", "mini boss", StringComparison.OrdinalIgnoreCase)
            .Replace("hidemob", "hide mob", StringComparison.OrdinalIgnoreCase);
        normalized = WordBoundaryRegex.Replace(normalized, " ");
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normalized.ToLower(CultureInfo.CurrentCulture));
    }
}