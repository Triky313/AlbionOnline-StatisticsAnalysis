using Serilog;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Common;

public static class SoundController
{
    private static readonly List<MediaPlayer> ActivePlayers = [];

    public static List<FileInformation> Sounds { get; set; } = new();

    public static void InitializeSoundFilesFromDirectory()
    {
        if (Sounds?.Count > 0)
        {
            return;
        }

        var soundFilePath = AppDataPaths.SoundDirectory;

        if (!Directory.Exists(soundFilePath))
        {
            return;
        }

        var files = DirectoryController.GetFiles(soundFilePath, "*.wav");

        if (files == null)
        {
            return;
        }

        Sounds ??= new List<FileInformation>();

        foreach (var file in files)
        {
            var fileInformation = new FileInformation(Path.GetFileNameWithoutExtension(file), file);
            Sounds.Add(fileInformation);
        }
    }
    
    public static void PlayAlertSound(string soundPath, double volumePercentage = 100)
    {
        if (string.IsNullOrWhiteSpace(soundPath))
        {
            return;
        }

        var normalizedVolume = double.IsFinite(volumePercentage)
            ? Math.Clamp(volumePercentage, 0, 100) / 100
            : 1;

        if (normalizedVolume <= 0)
        {
            return;
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher != null && !dispatcher.CheckAccess())
        {
            _ = dispatcher.InvokeAsync(() => PlayAlertSound(soundPath, volumePercentage));
            return;
        }

        PlayAlertSoundOnCurrentThread(soundPath, normalizedVolume);
    }

    private static void PlayAlertSoundOnCurrentThread(string soundPath, double volume)
    {
        var player = new MediaPlayer
        {
            Volume = volume
        };

        try
        {
            player.MediaEnded += (_, _) => ClosePlayer(player);
            player.MediaFailed += (_, eventArgs) => HandlePlaybackFailure(player, soundPath, eventArgs.ErrorException);
            ActivePlayers.Add(player);
            player.Open(new Uri(soundPath, UriKind.Absolute));
            player.Play();
        }
        catch (Exception e) when (e is InvalidOperationException or UriFormatException or FileNotFoundException or ArgumentException)
        {
            ClosePlayer(player);
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "Alert sound playback failed for {soundPath}", soundPath);
        }
    }

    private static void HandlePlaybackFailure(MediaPlayer player, string soundPath, Exception exception)
    {
        ClosePlayer(player);
        DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
        Log.Error(exception, "Alert sound playback failed for {soundPath}", soundPath);
    }

    private static void ClosePlayer(MediaPlayer player)
    {
        player.Close();
        ActivePlayers.Remove(player);
    }

    public static string GetCurrentSoundPath(string selectedAlertSound)
    {
        try
        {
            var currentSound = Sounds.FirstOrDefault(s => s.FileName == selectedAlertSound);
            return currentSound?.FilePath ?? string.Empty;
        }
        catch (Exception e) when (e is ArgumentException)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return string.Empty;
        }
    }
}
