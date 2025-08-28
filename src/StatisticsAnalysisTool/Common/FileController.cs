using Serilog;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

public static class FileController
{
    private static readonly SemaphoreSlim IoLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public static async Task<T> LoadAsync<T>(string path, Func<T, bool> validate = null) where T : new()
    {
        await IoLock.WaitAsync();

        try
        {
            EnsureDirectory(path);
            var tmp = GetTmpPath(path);

            if (File.Exists(tmp))
            {
                var (ok, value) = await TryLoad<T>(tmp);
                if (ok && PassesValidation(value, validate))
                {
                    File.Move(tmp, path, overwrite: true);
                    Log.Information("Recovered from tmp and promoted to {file}.", path);
                    return value!;
                }
                else
                {
                    SafeDelete(tmp);
                    Log.Warning("Tmp recovery failed for {file}, deleted tmp.", path);
                }
            }

            if (File.Exists(path))
            {
                var (ok, value) = await TryLoad<T>(path);
                if (ok && PassesValidation(value, validate))
                {
                    return value!;
                }
            }

            Log.Warning("Load failed for {file}. Returning default.", path);
            return new T();
        }
        finally
        {
            IoLock.Release();
        }
    }

    public static async Task<bool> SaveAsync<T>(T value, string path, Func<T, bool> validate = null)
    {
        await IoLock.WaitAsync();
        try
        {
            EnsureDirectory(path);
            if (!PassesValidation(value, validate))
            {
                Log.Warning("Rejected save for {file} (validation failed).", path);
                return false;
            }

            var json = JsonSerializer.Serialize(value, JsonOptions);
            var tmp = GetTmpPath(path);

            await File.WriteAllTextAsync(tmp, json, Encoding.UTF8);

            File.Move(tmp, path, overwrite: true);

            Log.Information("Saved {file} via tmp-swap.", path);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SaveAsync failed for {file}", path);
            return false;
        }
        finally
        {
            IoLock.Release();
        }
    }

    private static async Task<(bool ok, T value)> TryLoad<T>(string file)
    {
        try
        {
            var json = await File.ReadAllTextAsync(file, Encoding.UTF8);
            var obj = JsonSerializer.Deserialize<T>(json, JsonOptions);
            return (obj is not null, obj);
        }
        catch
        {
            return (false, default);
        }
    }

    private static string GetTmpPath(string path) => path + ".tmp";

    private static bool PassesValidation<T>(T value, Func<T, bool> validate) => validate == null || validate(value);

    private static void EnsureDirectory(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    private static void SafeDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // ignored
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
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}