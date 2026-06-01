using System;
using System.IO;
using System.Text;

namespace StatisticsAnalysisTool.Common;

public static class ExportFileWriter
{
    private static readonly Encoding Utf8WithBom =
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    public static void WriteText(string fileName, string? content)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name must not be empty.", nameof(fileName));
        }

        File.WriteAllText(fileName, content ?? string.Empty, Utf8WithBom);
    }
}
