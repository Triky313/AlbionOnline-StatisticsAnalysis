using Serilog;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;

namespace StatisticsAnalysisTool.Common;

public static class SoundController
{
    public static List<FileInformation> Sounds { get; set; } = new();

    public static void InitializeSoundFilesFromDirectory()
    {
        if (Sounds?.Count > 0)
        {
            return;
        }

        var soundFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.SoundDirectoryName);

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
    
    public static void PlayAlertSound(string soundPath)
    {
        try
        {
            var player = new SoundPlayer(soundPath);
            player.Load();
            player.Play();
            player.Dispose();
        }
        catch (Exception e) when (e is InvalidOperationException or UriFormatException or FileNotFoundException or ArgumentException)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
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
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return string.Empty;
        }
    }
}