using log4net;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace StatisticsAnalysisTool.Common;

public static class CraftingTabController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public static List<CraftingNote> CraftingNotes;
    private static bool _isLoading;
    private static bool _isSaving;

    public static void Add(string uniqueItemName, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (CraftingNotes == null || CraftingNotes?.Count <= 0)
        {
            LoadFromFile();
        }

        var note = CraftingNotes?.FirstOrDefault(x => x.UniqueItemName.Equals(uniqueItemName));
        if (note != null)
        {
            note.Note = text;
        }
        else
        {
            CraftingNotes?.Add(new CraftingNote() { UniqueItemName = uniqueItemName, Note = text });
        }
    }

    public static string GetNote(string uniqueItemName)
    {
        if (CraftingNotes == null || CraftingNotes?.Count <= 0)
        {
            LoadFromFile();
        }

        var note = CraftingNotes?.FirstOrDefault(x => x.UniqueItemName.Equals(uniqueItemName));
        return note?.Note ?? string.Empty;
    }

    #region Load / Save local file data

    private static void LoadFromFile()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.CraftingNotesFileName}";

        if (File.Exists(localFilePath))
        {
            try
            {
                var localFileString = File.ReadAllText(localFilePath, Encoding.UTF8);
                var result = JsonSerializer.Deserialize<List<CraftingNote>>(localFileString) ?? new List<CraftingNote>();
                CraftingNotes = new List<CraftingNote>(result);
                return;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                CraftingNotes = new List<CraftingNote>();
                return;
            }
        }

        CraftingNotes = new List<CraftingNote>();
        _isLoading = false;
    }

    public static void SaveInFile()
    {
        if (_isSaving)
        {
            return;
        }

        _isSaving = true;
        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.CraftingNotesFileName}";

        try
        {
            var fileString = JsonSerializer.Serialize(CraftingNotes);
            File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }

        _isSaving = false;
    }

    #endregion
}