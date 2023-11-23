using System.Net.Http;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Avalonia.Http;

public interface IHttpClientUtils
{
    Task<bool> DownloadFileAsync(HttpClient client, string url, string filePath, string taskName);
    Task<bool> IsUrlAccessible(string url);
}