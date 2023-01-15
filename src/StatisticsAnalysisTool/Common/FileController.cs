using log4net;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

public static class FileController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public static async Task<T> LoadAsync<T>(string localFilePath) where T : new()
    {
        if (!File.Exists(localFilePath))
        {
            return new T();
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
            };
            var localFileString = await File.ReadAllTextAsync(localFilePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(localFileString, options) ?? new T();
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            return new T();
        }
    }

    public static async Task SaveAsync<T>(T value, string localFilePath)
    {
        try
        {
            var option = new JsonSerializerOptions
            {
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
            };
            var fileString = await value.SerializeJsonStringAsync(option);
            var task = File.WriteAllTextAsync(localFilePath, fileString, Encoding.UTF8);
            task.Wait();
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}