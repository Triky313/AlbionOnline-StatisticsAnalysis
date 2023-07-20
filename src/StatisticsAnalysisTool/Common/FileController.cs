using log4net;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
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

            await using var writer = new StreamWriter(localFilePath, false, Encoding.UTF8);
            await writer.WriteAsync(fileString);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public static void TransferFileIfExistFromOldPathToUserDataDirectory(string oldFilePath)
    {
        var newFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Path.GetFileName(oldFilePath));
        
        try
        {
            FileInfo oldFileInfo = new FileInfo(oldFilePath);
            FileInfo newFileInfo = new FileInfo(newFilePath);

            if (!oldFileInfo.Exists)
            {
                return;
            }

            if (newFileInfo.Exists && oldFileInfo.Exists && newFileInfo.LastWriteTimeUtc > oldFileInfo.LastWriteTimeUtc)
            {
                File.Delete(oldFilePath);
                return;
            }

            File.Copy(oldFilePath, newFilePath, true);
            File.Delete(oldFilePath);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public static void DeleteFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            File.Delete(filePath);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}