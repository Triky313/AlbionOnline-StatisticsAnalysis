using StatisticsAnalysisTool.Crafting;
using StatisticsAnalysisTool.GameFileData.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameFileData;

public static class HideoutData
{
    public const string SourceFileName = "hideouts.json";
    public const string ModifiedFileName = "hideouts-modified.json";

    public static List<HideoutPowerLevelObject> PowerLevels { get; private set; } = [];

    public static async Task<bool> LoadDataAsync()
    {
        var data = await GameData.LoadDataAsync<HideoutPowerLevelObject, HideoutsRootObject>(
            SourceFileName,
            ModifiedFileName,
            new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            }
        ).ConfigureAwait(false);

        PowerLevels = data;
        return data.Count >= 0;
    }

    public static CraftingHideoutBonusOption[] GetHideoutBonusOptions()
    {
        var options = new List<CraftingHideoutBonusOption>
        {
            new()
            {
                Name = "None",
                Level = 0
            }
        }
        ;

        options.AddRange(PowerLevels
            .Select(CreateOption)
            .Where(x => x.Level is >= 1 and <= 9)
            .OrderBy(x => x.Level));

        return options.ToArray();
    }

    internal static IDisposable UsePowerLevelsForTests(List<HideoutPowerLevelObject> powerLevels)
    {
        var previousPowerLevels = PowerLevels;
        PowerLevels = powerLevels ?? [];
        return new HideoutDataScope(previousPowerLevels);
    }

    private static CraftingHideoutBonusOption CreateOption(HideoutPowerLevelObject powerLevel)
    {
        var level = ParseInt(powerLevel.Level);

        return new CraftingHideoutBonusOption
        {
            Name = "Level " + level,
            Level = level,
            GeneralistBonusPercent = ParsePercent(powerLevel.GeneralistCraftingBonus),
            SpecialistBonusPercent = ParsePercent(powerLevel.SpecialistCraftingBonus)
        }
        ;
    }

    private static int ParseInt(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return 0;
        }

        return result;
    }

    private static decimal ParsePercent(string value)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            return 0m;
        }

        return result * 100m;
    }

    private sealed class HideoutDataScope(List<HideoutPowerLevelObject> previousPowerLevels) : IDisposable
    {
        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            PowerLevels = previousPowerLevels;
            _isDisposed = true;
        }
    }
}