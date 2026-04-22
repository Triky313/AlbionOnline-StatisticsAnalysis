using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Reflection;
using System.Text;
using System.Windows;

namespace StatisticsAnalysisTool.HintBar;

internal static class HintBarClipboard
{
    public static void Copy(string barType, string message, Exception exception = null)
    {
        try
        {
            Clipboard.SetText(CreateClipboardText(barType, message, exception));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to copy hint bar diagnostic data to clipboard");
        }
    }

    private static string CreateClipboardText(string barType, string message, Exception exception)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
        var textBuilder = new StringBuilder();

        textBuilder.AppendLine("Statistics Analysis Tool diagnostic data");
        textBuilder.AppendLine($"Message type: {barType}");
        textBuilder.AppendLine($"Message: {message}");
        AppendException(textBuilder, exception);
        textBuilder.AppendLine($"Version: {version}");
        textBuilder.AppendLine();
        textBuilder.Append(SystemInfo.CreateReport());

        return textBuilder.ToString();
    }

    private static void AppendException(StringBuilder textBuilder, Exception exception)
    {
        if (exception == null)
        {
            return;
        }

        textBuilder.AppendLine();
        textBuilder.AppendLine("Exception:");
        textBuilder.AppendLine($"Type: {exception.GetType().FullName}");
        textBuilder.AppendLine($"Message: {exception.Message}");
        textBuilder.AppendLine("Details:");
        textBuilder.AppendLine(exception.ToString());
        textBuilder.AppendLine();
    }
}