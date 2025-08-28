using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Common;

public static class CraftingTabController
{
    public static List<CraftingNote> CraftingNotes;
    private static bool _isLoading;
    private static bool _isSaving;

    public static async Task AddNoteAsync(string uniqueItemName, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (CraftingNotes == null || CraftingNotes?.Count <= 0)
        {
            await LoadFromFileAsync();
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

    public static async Task<string> GetNoteAsync(string uniqueItemName)
    {
        if (CraftingNotes == null || CraftingNotes?.Count <= 0)
        {
            await LoadFromFileAsync();
        }

        var note = CraftingNotes?.FirstOrDefault(x => x.UniqueItemName.Equals(uniqueItemName));
        return note?.Note ?? string.Empty;
    }

    #region Load / Save local file data

    private static async Task LoadFromFileAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;

        CraftingNotes = await FileController.LoadAsync<List<CraftingNote>>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.CraftingNotesFileName));

        _isLoading = false;
    }

    public static void SaveInFile()
    {
        if (_isSaving)
        {
            return;
        }

        _isSaving = true;

        var localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.CraftingNotesFileName);

        try
        {
            var fileString = JsonSerializer.Serialize(CraftingNotes);
            File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }

        _isSaving = false;
    }

    #endregion
}