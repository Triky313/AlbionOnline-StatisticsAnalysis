using StatisticsAnalysisTool.ViewModels;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Serilog;

namespace StatisticsAnalysisTool.Common;

public static class HttpClientUtils
{
    public static async Task<bool> DownloadFileAsync(this HttpClient client, string url, string filePath, string taskName)
    {
        try
        {
            var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
            if (mainWindowViewModel != null)
            {
                mainWindowViewModel.ToolTaskCurrentTaskName = taskName;
                mainWindowViewModel.ToolTaskFrontViewVisibility = Visibility.Visible;
            }

            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            var totalBytes = response.Content.Headers.ContentLength;

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var output = File.Create(filePath);

            var buffer = new byte[16 * 1024];
            int bytesRead;
            var bytesProcessed = 0;

            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                await output.WriteAsync(buffer.AsMemory(0, bytesRead));

                bytesProcessed += bytesRead;

                var percentage = bytesProcessed * 100.0 / totalBytes;
                if (mainWindowViewModel != null)
                {
                    mainWindowViewModel.ToolTaskProgressBarValue = percentage ?? 0;
                }
            }

            if (mainWindowViewModel != null)
            {
                mainWindowViewModel.ToolTaskFrontViewVisibility = Visibility.Collapsed;
                mainWindowViewModel.ToolTaskCurrentTaskName = string.Empty;
                mainWindowViewModel.ToolTaskProgressBarValue = 0;
            }

            return true;
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return false;
        }
    }

    public static async Task<bool> IsUrlAccessible(string url)
    {
        using HttpClient client = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url);

        HttpResponseMessage response = await client.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}