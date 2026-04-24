using Serilog;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

public static class FileController
{
    private const int FileBufferSize = 65536;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks = new(StringComparer.OrdinalIgnoreCase);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public static async Task<T> LoadAsync<T>(string path, Func<T, bool> validate = null) where T : new()
    {
        var fileLock = GetFileLock(path);
        await fileLock.WaitAsync().ConfigureAwait(false);

        try
        {
            EnsureDirectory(path);
            var tmp = GetTmpPath(path);

            if (File.Exists(tmp))
            {
                var (ok, value) = await TryLoad<T>(tmp).ConfigureAwait(false);
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
                var (ok, value) = await TryLoad<T>(path).ConfigureAwait(false);
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
            fileLock.Release();
        }
    }

    public static async Task<bool> SaveAsync<T>(T value, string path, Func<T, bool> validate = null)
    {
        var fileLock = GetFileLock(path);
        await fileLock.WaitAsync().ConfigureAwait(false);

        try
        {
            EnsureDirectory(path);
            if (!PassesValidation(value, validate))
            {
                Log.Warning("Rejected save for {file} (validation failed).", path);
                return false;
            }

            var tmp = GetTmpPath(path);

            await using (var stream = CreateWriteStream(tmp))
            {
                await JsonSerializer.SerializeAsync(stream, value, JsonOptions).ConfigureAwait(false);
            }

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
            fileLock.Release();
        }
    }

    private static async Task<(bool ok, T value)> TryLoad<T>(string file)
    {
        try
        {
            await using var stream = CreateReadStream(file);
            var obj = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions).ConfigureAwait(false);
            return (obj is not null, obj);
        }
        catch
        {
            return (false, default);
        }
    }

    private static SemaphoreSlim GetFileLock(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return FileLocks.GetOrAdd(fullPath, _ => new SemaphoreSlim(1, 1));
    }

    private static FileStream CreateReadStream(string path)
    {
        return new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            FileBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
    }

    private static FileStream CreateWriteStream(string path)
    {
        return new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            FileBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
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