using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.DamageMeter;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.OpenWorld;

public class OpenWorldController(MainWindowViewModel mainWindowViewModel)
{
    private const string MobKillsFileName = "OpenWorldMobKills.json";
    private static readonly HashSet<string> MammothMobUniqueNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "T8_MOB_HIDE_STEPPE_MAMMOTH", 
        "T8_MOB_DYNAMIC_HIDE_STEPPE_MAMMOTH", 
        "T8_MOB_HIDE_STEPPE_ANCIENTMAMMOTH", 
        "T8_MOB_TREASURE_ANCIENTMAMMOTH", 
        "T6_MOB_GUARDIAN_STEPPE_MAMMOTH", 
        "T6_MOB_MINIGUARDIAN_STEPPE_MAMMOTH"
    };

    private readonly ConcurrentDictionary<long, byte> _recordedKilledMobs = new();

    public async Task TryAddMobKillAsync(CombatMobCacheEntry mob, double healthChange, bool hasNewHealthValue)
    {
        if (!SettingsController.CurrentSettings.IsOpenWorldTrackingActive)
        {
            return;
        }

        if (healthChange >= 0 || hasNewHealthValue)
        {
            return;
        }

        if (mob?.MobData == null || !IsTrackedMammoth(mob.MobData.UniqueName))
        {
            return;
        }

        if (!_recordedKilledMobs.TryAdd(mob.MobObjectId, 0))
        {
            return;
        }

        await RemoveEntriesByAutoDeleteDateAsync();

        var mobUniqueName = mob.MobData.UniqueName ?? mob.UniqueName ?? string.Empty;
        var mobName = MobsData.GetLocalizedMobName(mob.MobData);
        var mobKill = new OpenWorldMobKill
        {
            TimestampUtc = DateTime.UtcNow.Ticks,
            MobUniqueName = mobUniqueName,
            MobName = string.IsNullOrWhiteSpace(mobName) ? mobUniqueName : mobName,
            Avatar = MobsData.GetAvatarFileName(mob.MobData)
        };

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            mainWindowViewModel.OpenWorldBindings.MobKills.Add(mobKill);
            mainWindowViewModel.OpenWorldBindings.UpdateStats();
        });
    }

    public async Task RemoveEntriesByAutoDeleteDateAsync()
    {
        var deleteBefore = SettingsController.CurrentSettings.OpenWorldAutoDeleteStats switch
        {
            OpenWorldAutoDeleteStats.DeleteAfter7Days => DateTime.UtcNow.AddDays(-7),
            OpenWorldAutoDeleteStats.DeleteAfter30Days => DateTime.UtcNow.AddDays(-30),
            OpenWorldAutoDeleteStats.DeleteAfter365Days => DateTime.UtcNow.AddDays(-365),
            _ => DateTime.MinValue
        };

        if (deleteBefore == DateTime.MinValue)
        {
            return;
        }

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var entriesToDelete = mainWindowViewModel.OpenWorldBindings.MobKills
                .Where(x => x.TimestampDateTimeUtc < deleteBefore)
                .ToList();

            foreach (var entry in entriesToDelete)
            {
                mainWindowViewModel.OpenWorldBindings.MobKills.Remove(entry);
            }

            if (entriesToDelete.Count > 0)
            {
                mainWindowViewModel.OpenWorldBindings.UpdateStats();
            }
        });
    }

    public async Task LoadFromFileAsync()
    {
        var mobKills = await FileController.LoadAsync<List<OpenWorldMobKill>>(AppDataPaths.UserDataFile(MobKillsFileName));
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            mainWindowViewModel.OpenWorldBindings.MobKills.Clear();
            foreach (var mobKill in mobKills)
            {
                mainWindowViewModel.OpenWorldBindings.MobKills.Add(mobKill);
            }

            mainWindowViewModel.OpenWorldBindings.UpdateStats();
        }, DispatcherPriority.Loaded);
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(AppDataPaths.UserDataDirectory);
        var mobKills = mainWindowViewModel.OpenWorldBindings.MobKills.ToList();
        await FileController.SaveAsync(mobKills, AppDataPaths.UserDataFile(MobKillsFileName));
        Log.Information("Open World mob kills saved");
    }

    public async Task SaveOnClusterChangedAsync()
    {
        await SaveInFileAsync();
        _recordedKilledMobs.Clear();
    }

    private static bool IsTrackedMammoth(string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName))
        {
            return false;
        }

        var normalizedUniqueName = uniqueName.StartsWith("@MOB_", StringComparison.OrdinalIgnoreCase) ? uniqueName[5..] : uniqueName;

        return MammothMobUniqueNames.Contains(normalizedUniqueName);
    }
}