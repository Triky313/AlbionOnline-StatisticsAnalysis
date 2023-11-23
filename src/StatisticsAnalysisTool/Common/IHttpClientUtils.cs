using System.Net.Http;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

public interface IHttpClientUtils
{
    Task<bool> DownloadFileAsync(HttpClient client, string url, string filePath, string taskName);
    Task<bool> IsUrlAccessible(string url);
}