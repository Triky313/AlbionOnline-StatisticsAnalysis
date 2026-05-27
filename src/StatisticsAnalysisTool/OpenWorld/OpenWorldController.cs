using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.DamageMeter;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.OpenWorld;

public class OpenWorldController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
{
    private const string MobKillsFileName = "OpenWorldMobKills.json";
    private const string UnknownMobUniqueName = "UNKNOWN_MOB";
    private static readonly TimeSpan PendingMobKillRetention = TimeSpan.FromSeconds(2);

    private readonly ConcurrentDictionary<long, byte> _recordedKilledMobs = new();
    private readonly ConcurrentDictionary<long, DateTime> _pendingKilledMobs = new();
    private readonly ConcurrentDictionary<long, byte> _localPlayerDamagedMobs = new();

    public void TrackLocalPlayerMobDamage(long mobObjectId, long causerId, double healthChange)
    {
        if (!SettingsController.CurrentSettings.IsOpenWorldTrackingActive || healthChange >= 0 || _localPlayerDamagedMobs.ContainsKey(mobObjectId))
        {
            return;
        }

        if (!IsLocalPlayer(causerId))
        {
            return;
        }

        _localPlayerDamagedMobs.TryAdd(mobObjectId, 0);
    }

    public async Task TryAddMobKillAsync(long mobObjectId, CombatMobCacheEntry mob, double healthChange, bool hasNewHealthValue)
    {
        if (!SettingsController.CurrentSettings.IsOpenWorldTrackingActive)
        {
            return;
        }

        if (healthChange >= 0 || hasNewHealthValue)
        {
            return;
        }

        if (mob == null)
        {
            if (_localPlayerDamagedMobs.ContainsKey(mobObjectId))
            {
                AddPendingMobKill(mobObjectId);
            }

            return;
        }

        await TryAddMobKillAsync(mobObjectId, mob);
    }

    public async Task<bool> TryAddPendingMobKillAsync(long mobObjectId, CombatMobCacheEntry mob)
    {
        if (!SettingsController.CurrentSettings.IsOpenWorldTrackingActive || mob == null)
        {
            return false;
        }

        RemoveExpiredPendingMobKills();

        if (!_pendingKilledMobs.TryGetValue(mobObjectId, out var pendingKillTimestamp)
            || DateTime.UtcNow - pendingKillTimestamp > PendingMobKillRetention)
        {
            _pendingKilledMobs.TryRemove(mobObjectId, out _);
            return false;
        }

        if (!_pendingKilledMobs.TryRemove(mobObjectId, out _))
        {
            return false;
        }

        return await TryAddMobKillAsync(mobObjectId, mob);
    }

    public void ResetRecordedMobKill(long mobObjectId)
    {
        _recordedKilledMobs.TryRemove(mobObjectId, out _);
        _localPlayerDamagedMobs.TryRemove(mobObjectId, out _);
    }

    private async Task<bool> TryAddMobKillAsync(long mobObjectId, CombatMobCacheEntry mob)
    {
        if (mob?.MobData == null || string.IsNullOrWhiteSpace(mob.MobData.UniqueName))
        {
            return false;
        }

        if (string.Equals(mob.MobData.UniqueName, UnknownMobUniqueName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!_localPlayerDamagedMobs.ContainsKey(mobObjectId))
        {
            return false;
        }

        if (!_recordedKilledMobs.TryAdd(mobObjectId, 0))
        {
            return false;
        }

        _localPlayerDamagedMobs.TryRemove(mobObjectId, out _);

        await RemoveEntriesByAutoDeleteDateAsync();

        var mobUniqueName = mob.MobData.UniqueName ?? mob.UniqueName ?? string.Empty;
        var mobName = MobsData.GetLocalizedMobName(mob.MobData);
        var mobKill = new OpenWorldMobKill
        {
            TimestampUtc = DateTime.UtcNow.Ticks,
            MobUniqueName = mobUniqueName,
            MobName = string.IsNullOrWhiteSpace(mobName) ? mobUniqueName : mobName,
            Avatar = MobsData.GetAvatarFileName(mob.MobData),
            Faction = MobsData.GetFaction(mob.MobData)
        };

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            mainWindowViewModel.OpenWorldBindings.MobKills.Add(mobKill);
            mainWindowViewModel.OpenWorldBindings.UpdateStats();
        });

        return true;
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
        if (!AppDataPaths.TryEnsureUserDataDirectory())
        {
            return;
        }

        var mobKills = mainWindowViewModel.OpenWorldBindings.MobKills.ToList();
        await FileController.SaveAsync(mobKills, AppDataPaths.UserDataFile(MobKillsFileName));
        Log.Information("Open World mob kills saved");
    }

    public async Task SaveOnClusterChangedAsync()
    {
        await SaveInFileAsync();
        _recordedKilledMobs.Clear();
        _pendingKilledMobs.Clear();
        _localPlayerDamagedMobs.Clear();
    }

    public async Task ResetStatsAsync()
    {
        _recordedKilledMobs.Clear();
        _pendingKilledMobs.Clear();
        _localPlayerDamagedMobs.Clear();

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            mainWindowViewModel.OpenWorldBindings.ResetStats();
        });

        await SaveInFileAsync();
    }

    private void AddPendingMobKill(long mobObjectId)
    {
        RemoveExpiredPendingMobKills();
        _pendingKilledMobs[mobObjectId] = DateTime.UtcNow;
        Log.Debug("Open World mob kill pending until mob data is available | ObjectId={ObjectId}", mobObjectId);
    }

    private void RemoveExpiredPendingMobKills()
    {
        var currentUtc = DateTime.UtcNow;
        foreach (var pendingKilledMob in _pendingKilledMobs.ToArray())
        {
            if (currentUtc - pendingKilledMob.Value > PendingMobKillRetention)
            {
                _pendingKilledMobs.TryRemove(pendingKilledMob.Key, out _);
            }
        }
    }

    private bool IsLocalPlayer(long objectId)
    {
        if (trackingController.EntityController.LocalUserData.UserObjectId == objectId)
        {
            return true;
        }

        var localEntity = trackingController.EntityController.GetLocalEntity();
        return localEntity?.Value?.ObjectId == objectId;
    }
}
