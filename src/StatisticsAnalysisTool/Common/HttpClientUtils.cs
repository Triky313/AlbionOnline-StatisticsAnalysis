using log4net;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

public static class HttpClientUtils
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public static async Task<bool> DownloadFileAsync(this HttpClient client, string url, string filePath, FileMode fileMode = FileMode.Create)
    {
        try
        {
            await using var stream = await client.GetStreamAsync(url);
            await using var fileStream = new FileStream(filePath, fileMode);
            await stream.CopyToAsync(fileStream);
            return true;
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            return false;
        }
    }
}