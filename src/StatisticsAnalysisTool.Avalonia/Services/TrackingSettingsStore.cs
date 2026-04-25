using Serilog;
using StatisticsAnalysisTool.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Avalonia.Services;

public sealed class TrackingSettingsStore
{
    private const string SettingsFileName = "Settings.json";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private JsonObject? _settingsRoot;

    public string SettingsFilePath { get; } = ResolveSettingsFilePath();

    public async Task<TrackingSettingsDocument> LoadAsync()
    {
        if (!File.Exists(SettingsFilePath))
        {
            _settingsRoot = new JsonObject();
            return new TrackingSettingsDocument();
        }

        try
        {
            string json = await File.ReadAllTextAsync(SettingsFilePath).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
            {
                _settingsRoot = new JsonObject();
                return new TrackingSettingsDocument();
            }

            JsonObject? root = JsonNode.Parse(json) as JsonObject;
            _settingsRoot = root ?? new JsonObject();

            TrackingSettingsDocument? document = _settingsRoot.Deserialize<TrackingSettingsDocument>(JsonSerializerOptions);
            return document ?? new TrackingSettingsDocument();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Settings file could not be loaded from {SettingsFilePath}", SettingsFilePath);
            _settingsRoot = new JsonObject();
            return new TrackingSettingsDocument();
        }
    }

    public async Task SaveAsync(TrackingSettingsDocument settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        JsonObject root = _settingsRoot is null
            ? new JsonObject()
            : (JsonObject)_settingsRoot.DeepClone();

        root[nameof(TrackingSettingsDocument.PacketProvider)] = (int)settings.PacketProvider;
        root[nameof(TrackingSettingsDocument.PacketFilter)] = settings.PacketFilter;
        root[nameof(TrackingSettingsDocument.NetworkDevice)] = settings.NetworkDevice;
        root[nameof(TrackingSettingsDocument.NetworkDevices)] = JsonSerializer.SerializeToNode(settings.NetworkDevices, JsonSerializerOptions);

        string? directoryPath = Path.GetDirectoryName(SettingsFilePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await File.WriteAllTextAsync(SettingsFilePath, root.ToJsonString(JsonSerializerOptions)).ConfigureAwait(false);
        _settingsRoot = root;
    }

    private static string ResolveSettingsFilePath()
    {
        string localSettingsPath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        if (File.Exists(localSettingsPath))
        {
            return localSettingsPath;
        }

        string? sharedSettingsPath = FindExistingWpfSettingsFile();
        return string.IsNullOrWhiteSpace(sharedSettingsPath) ? localSettingsPath : sharedSettingsPath;
    }

    private static string? FindExistingWpfSettingsFile()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string solutionFilePath = Path.Combine(currentDirectory.FullName, "StatisticsAnalysisTool.sln");
            if (File.Exists(solutionFilePath))
            {
                foreach (string configuration in GetConfigurations())
                {
                    foreach (string targetFramework in GetTargetFrameworks())
                    {
                        string candidatePath = Path.Combine(currentDirectory.FullName, "StatisticsAnalysisTool", "bin", configuration, targetFramework, SettingsFileName);
                        if (File.Exists(candidatePath))
                        {
                            return candidatePath;
                        }
                    }
                }
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    private static IReadOnlyList<string> GetConfigurations()
    {
        return ["Debug", "Release"];
    }

    private static IReadOnlyList<string> GetTargetFrameworks()
    {
        return ["net10.0-windows", "net10.0-windows7.0", "net9.0-windows", "net9.0-windows7.0"];
    }
}
