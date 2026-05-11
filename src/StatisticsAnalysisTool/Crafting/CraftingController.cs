using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingController
{
    private const string CraftingsFileName = "Craftings.json";

    private static string CraftingsFilePath => AppDataPaths.UserDataFile(CraftingsFileName);

    public async Task<List<SavedCrafting>> LoadAsync()
    {
        try
        {
            var craftings = await FileController.LoadAsync<List<SavedCrafting>>(CraftingsFilePath).ConfigureAwait(false);
            return craftings ?? [];
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "Craftings could not be loaded");
            return [];
        }
    }

    public async Task SaveAsync(IEnumerable<SavedCrafting> craftings)
    {
        try
        {
            var craftingsToSave = craftings?.OrderByDescending(x => x.LastChangedUtc).ToList() ?? [];
            await FileController.SaveAsync(craftingsToSave, CraftingsFilePath).ConfigureAwait(false);
            Log.Information("Craftings saved");
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "Craftings could not be saved");
        }
    }
}