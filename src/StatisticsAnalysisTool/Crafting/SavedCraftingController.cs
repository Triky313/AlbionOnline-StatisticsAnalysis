using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Crafting;

public class SavedCraftingController
{
    private const string CraftingsFileName = "Craftings.json";
    private const string LegacyCraftingNotesFileName = "CraftingNotes.json";

    private static string CraftingsFilePath => AppDataPaths.UserDataFile(CraftingsFileName);
    private static string LegacyCraftingNotesFilePath => AppDataPaths.UserDataFile(LegacyCraftingNotesFileName);

    public async Task<List<SavedCrafting>> LoadAsync()
    {
        try
        {
            var craftings = await FileController.LoadAsync<List<SavedCrafting>>(CraftingsFilePath).ConfigureAwait(false);
            craftings ??= [];

            await MigrateLegacyNotesAsync(craftings).ConfigureAwait(false);
            return craftings;
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "Craftings could not be loaded");
            return [];
        }
    }

    public async Task<bool> SaveAsync(IEnumerable<SavedCrafting> craftings)
    {
        if (!AppDataPaths.TryEnsureUserDataDirectory())
        {
            Log.Debug("Skipped craftings save because no Albion server is active.");
            return false;
        }

        try
        {
            var craftingsToSave = craftings?.OrderByDescending(x => x.LastChangedUtc).ToList() ?? [];
            var saved = await FileController.SaveAsync(craftingsToSave, CraftingsFilePath).ConfigureAwait(false);
            if (saved)
            {
                Log.Information("Craftings saved");
            }

            return saved;
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "Craftings could not be saved");
            return false;
        }
    }

    private async Task MigrateLegacyNotesAsync(List<SavedCrafting> craftings)
    {
        if (!AppDataPaths.IsUserDataAvailable || !File.Exists(LegacyCraftingNotesFilePath))
        {
            return;
        }

        var legacyNotes = await FileController.LoadAsync<List<LegacyCraftingNote>>(LegacyCraftingNotesFilePath).ConfigureAwait(false);
        var migrated = ApplyLegacyNotes(craftings, legacyNotes);
        var canDeleteLegacyFile = true;

        if (migrated)
        {
            canDeleteLegacyFile = await SaveAsync(craftings).ConfigureAwait(false);
        }

        if (canDeleteLegacyFile)
        {
            DeleteLegacyCraftingNotesFiles();
        }
    }

    private static bool ApplyLegacyNotes(List<SavedCrafting> craftings, List<LegacyCraftingNote> legacyNotes)
    {
        if (craftings == null || legacyNotes == null || craftings.Count == 0 || legacyNotes.Count == 0)
        {
            return false;
        }

        var notesByItem = legacyNotes
            .Where(x => !string.IsNullOrWhiteSpace(x.UniqueItemName) && !string.IsNullOrWhiteSpace(x.Note))
            .GroupBy(x => x.UniqueItemName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Last().Note, StringComparer.OrdinalIgnoreCase);

        var migrated = false;

        foreach (var crafting in craftings.Where(x => x != null && string.IsNullOrWhiteSpace(x.Notes)))
        {
            if (string.IsNullOrWhiteSpace(crafting.ItemUniqueName) || !notesByItem.TryGetValue(crafting.ItemUniqueName, out var note))
            {
                continue;
            }

            crafting.Notes = note;
            migrated = true;
        }

        return migrated;
    }

    private static void DeleteLegacyCraftingNotesFiles()
    {
        DeleteLegacyCraftingNotesFile(LegacyCraftingNotesFilePath);
        DeleteLegacyCraftingNotesFile(LegacyCraftingNotesFilePath + ".tmp");
    }

    private static void DeleteLegacyCraftingNotesFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            File.Delete(filePath);
            Log.Information("Deleted legacy crafting notes file {File}", filePath);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "Legacy crafting notes file could not be deleted");
        }
    }

    private sealed class LegacyCraftingNote
    {
        public string UniqueItemName { get; set; }
        public string Note { get; set; }
    }
}
