using System.Text;
using System.Text.Json;

namespace StatisticAnalysisTool.Extractor;

public static class ExtractorUtilities
{
    public static void WriteItem(Stream stream, IdContainer idContainer, bool first = false)
    {
        var output = new StringBuilder();

        if (!first)
        {
            output.AppendLine(",");
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        if (idContainer is ItemContainer itemContainer)
        {
            output.Append(JsonSerializer.Serialize(itemContainer, options));
        }
        else
        {
            output.Append(JsonSerializer.Serialize(idContainer, options));
        }

        WriteString(stream, output.ToString());
        output.Clear();
    }

    public static void WriteString(Stream stream, string val)
    {
        var buffer = Encoding.UTF8.GetBytes(val);
        stream.Write(buffer, 0, buffer.Length);
    }
}