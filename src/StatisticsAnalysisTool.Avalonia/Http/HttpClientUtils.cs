using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Avalonia.Http;

public class HttpClientUtils : IHttpClientUtils
{
    public async Task<bool> DownloadFileAsync(HttpClient client, string url, string filePath, string taskName)
    {
        try
        {
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var output = File.Create(filePath);

            var buffer = new byte[16 * 1024];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                await output.WriteAsync(buffer.AsMemory(0, bytesRead));
            }

            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return false;
        }
    }

    public async Task<bool> IsUrlAccessible(string url)
    {
        using HttpClient client = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url);

        HttpResponseMessage response = await client.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}