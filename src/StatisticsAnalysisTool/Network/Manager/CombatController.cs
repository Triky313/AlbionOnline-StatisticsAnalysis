using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.DamageMeter;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager;

public class CombatController
{
    private const int DamageStatsUiUpdateIntervalInMilliseconds = 1000;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly TrackingController _trackingController;
    private readonly DamageStatsTracker _damageStatsTracker = new();
    private readonly ConcurrentDictionary<DashboardContentType, DamageStatsTracker> _damageStatsTrackersByContent = new();
    private readonly object _damageStatsUiUpdateLock = new();
    private bool _combatModeWasCombatOver;
    private bool _isDamageStatsUiUpdateActive;
    private DateTime _lastDamageStatsUiUpdate;
    private int _damageStatsVersion;
    public CombatEventTracker CombatEventTracker { get; }

    public CombatController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
        CombatEventTracker = new CombatEventTracker(trackingController);

        OnChangeCombatMode += ResetDamageMeterBeforeCombatStart;
        OnChangeCombatMode += AddCombatTime;
        OnChangeCombatMode += SetLastCombatMode;
        OnChangeCombatMode += CombatEventTracker.OnCombatStateUpdate;
        OnDamageUpdate += UpdateDamageMeterUiAsync;
        _mainWindowViewModel.DamageMeterBindings.DamageMeterContentFilterChanged += OnDamageMeterContentFilterChanged;
        _mainWindowViewModel.DamageMeterBindings.DamageMeterDisplayChanged += OnDamageMeterDisplayChanged;
        _mainWindowViewModel.DamageMeterBindings.DamageMeterSnapshotProvider = CreateDamageMeterSnapshot;

#if DEBUG
        RunDamageMeterDebugAsync(0, 0);
#endif
    }

    #region Damage Meter methods

    public event Action<ObservableCollection<DamageMeterFragment>, List<KeyValuePair<Guid, PlayerGameObject>>> OnDamageUpdate;

    public Task AddDamage(long affectedId, long causerId, double healthChange, double newHealthValue, int causingSpellIndex)
    {
        var healthChangeType = GetHealthChangeType(healthChange);
        if (!SettingsController.CurrentSettings.IsDamageMeterTrackingActive || (affectedId == causerId && healthChangeType == HealthChangeType.Damage))
        {
            return Task.CompletedTask;
        }

        var causerGameObject = _trackingController.EntityController?.GetEntity(causerId);
        var causerGameObjectValue = causerGameObject?.Value;

        var affectedGameObject = _trackingController.EntityController?.GetEntity(affectedId);

        if (_mainWindowViewModel.DamageMeterBindings.OnlyDamageToPlayersCounts && affectedGameObject?.Value is not { ObjectType: GameObjectType.Player })
        {
            return Task.CompletedTask;
        }

        if (causerGameObject?.Value is not { ObjectType: GameObjectType.Player } || !_trackingController.EntityController!.IsEntityInParty(causerGameObject.Value.Key))
        {
            return Task.CompletedTask;
        }

        var contentType = GetCurrentContentType();
        var contentStats = causerGameObjectValue.GetOrCreateDamageMeterContentStats(contentType);
        var contentDamageStatsTracker = _damageStatsTrackersByContent.GetOrAdd(contentType, _ => new DamageStatsTracker());

        if (healthChangeType == HealthChangeType.Damage)
        {
            var damageChangeValue = (int) Math.Round(healthChange.ToPositiveFromNegativeOrZero(), MidpointRounding.AwayFromZero);
            if (damageChangeValue <= 0)
            {
                return Task.CompletedTask;
            }

            RecordLastContributionWeapon(causerGameObjectValue, contentStats);
            causerGameObjectValue.Damage += damageChangeValue;
            AddOrUpdateSpell(causingSpellIndex, causerGameObjectValue, causerGameObjectValue.Spells, healthChangeType, damageChangeValue);
            lock (contentStats.SyncRoot)
            {
                contentStats.Damage += damageChangeValue;
                AddOrUpdateSpell(causingSpellIndex, causerGameObjectValue, contentStats.Spells, healthChangeType, damageChangeValue);
            }

            CombatEventTracker.AddHealthContribution(CombatEventValueType.Damage, causerId, affectedId, damageChangeValue, causingSpellIndex, contentType);
            var isMobTarget = affectedGameObject?.Value is not { ObjectType: GameObjectType.Player };
            _damageStatsTracker.RecordDamage(causerGameObject.Value.Key, causerGameObjectValue.Name, affectedId, damageChangeValue, newHealthValue, isMobTarget);
            contentDamageStatsTracker.RecordDamage(causerGameObject.Value.Key, causerGameObjectValue.Name, affectedId, damageChangeValue, newHealthValue, isMobTarget);
        }

        if (healthChangeType == HealthChangeType.Heal)
        {
            var healChangeValue = healthChange;
            if (healChangeValue <= 0)
            {
                return Task.CompletedTask;
            }

            var positiveHealChangeValue = (int) Math.Round(healChangeValue, MidpointRounding.AwayFromZero);
            RecordLastContributionWeapon(causerGameObjectValue, contentStats);
            if (!IsMaxHealthReached(affectedId, newHealthValue))
            {
                causerGameObjectValue.Heal += positiveHealChangeValue;
                AddOrUpdateSpell(causingSpellIndex, causerGameObjectValue, causerGameObjectValue.Spells, healthChangeType, positiveHealChangeValue);
                lock (contentStats.SyncRoot)
                {
                    contentStats.Heal += positiveHealChangeValue;
                    AddOrUpdateSpell(causingSpellIndex, causerGameObjectValue, contentStats.Spells, healthChangeType, positiveHealChangeValue);
                }

                CombatEventTracker.AddHealthContribution(CombatEventValueType.Heal, causerId, affectedId, positiveHealChangeValue, causingSpellIndex, contentType);
                contentDamageStatsTracker.RecordHeal(causerGameObject.Value.Key, causerGameObjectValue.Name, positiveHealChangeValue);
                _damageStatsTracker.RecordHeal(causerGameObject.Value.Key, causerGameObjectValue.Name, positiveHealChangeValue);
            }
            else
            {
                causerGameObjectValue.Overhealed += positiveHealChangeValue;
                lock (contentStats.SyncRoot)
                {
                    contentStats.Overhealed += positiveHealChangeValue;
                }

                contentDamageStatsTracker.RecordOverheal(causerGameObject.Value.Key, causerGameObjectValue.Name, positiveHealChangeValue);
                _damageStatsTracker.RecordOverheal(causerGameObject.Value.Key, causerGameObjectValue.Name, positiveHealChangeValue);
            }
        }

        causerGameObjectValue.CombatStart ??= DateTime.UtcNow;
        lock (contentStats.SyncRoot)
        {
            contentStats.CombatStart ??= DateTime.UtcNow;
        }

        UpdateDamageMeterUiForSelectedContent();
        UpdateDamageStatsUiIfAllowed();
        return Task.CompletedTask;
    }

    public Task AddTakenDamage(long affectedId, long causerId, double healthChange, double newHealthValue, int causingSpellIndex)
    {
        var healthChangeType = GetHealthChangeType(healthChange);
        if (!SettingsController.CurrentSettings.IsDamageMeterTrackingActive || (affectedId == causerId && healthChangeType == HealthChangeType.Damage))
        {
            return Task.CompletedTask;
        }

        var gameObject = _trackingController?.EntityController?.GetEntity(affectedId);
        var gameObjectValue = gameObject?.Value;

        if (gameObject?.Value is not { ObjectType: GameObjectType.Player } || !_trackingController.EntityController.IsEntityInParty(gameObject.Value.Key))
        {
            return Task.CompletedTask;
        }

        var contentType = GetCurrentContentType();
        var contentStats = gameObjectValue.GetOrCreateDamageMeterContentStats(contentType);

        if (healthChangeType == HealthChangeType.Damage)
        {
            var damageChangeValue = (int) Math.Round(healthChange.ToPositiveFromNegativeOrZero(), MidpointRounding.AwayFromZero);
            if (damageChangeValue <= 0)
            {
                return Task.CompletedTask;
            }

            gameObjectValue.TakenDamage += damageChangeValue;
            lock (contentStats.SyncRoot)
            {
                contentStats.TakenDamage += damageChangeValue;
            }

            CombatEventTracker.AddHealthContribution(CombatEventValueType.TakenDamage, causerId, affectedId, damageChangeValue, causingSpellIndex, contentType);
        }

        UpdateDamageMeterUiForSelectedContent();

        UpdateDamageStatsUiIfAllowed();
        return Task.CompletedTask;
    }

    private static bool _isUiUpdateActive;

    private DashboardContentType GetCurrentContentType()
    {
        var currentCluster = ClusterController.CurrentCluster;
        return DashboardContentTypeResolver.Resolve(
            currentCluster.MapType,
            _trackingController.StatisticController.ResolveDungeonMode(currentCluster.MapType),
            currentCluster.ClusterMode);
    }

    private void UpdateDamageMeterUiForSelectedContent()
    {
        if (!IsUiUpdateRequired())
        {
            return;
        }

        var bindings = _mainWindowViewModel.DamageMeterBindings;
        var selectedContentType = bindings.DamageMeterContentFilterSelection?.ContentType;

        if (bindings.DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Mob)
        {
            _ = UpdateMobDamageMeterUiAsync(selectedContentType);
            return;
        }

        OnDamageUpdate?.Invoke(
            bindings.DamageMeter,
            _trackingController.EntityController.GetAllEntitiesWithDamageOrHealAndInParty(selectedContentType));
    }

    public async void UpdateDamageMeterUiAsync(ObservableCollection<DamageMeterFragment> damageMeter, List<KeyValuePair<Guid, PlayerGameObject>> entities)
    {
        if (!IsUiUpdateAllowed())
        {
            return;
        }

        _isUiUpdateActive = true;

        try
        {
            await UpdateDamageMeterUiCoreAsync(damageMeter, entities);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Damage Meter UI update failed");
        }
        finally
        {
            _isUiUpdateActive = false;
        }
    }

    private async Task UpdateMobDamageMeterUiAsync(DashboardContentType? contentType)
    {
        if (!IsUiUpdateAllowed())
        {
            return;
        }

        _isUiUpdateActive = true;

        try
        {
            var mobStats = CombatEventTracker.GetMobDamageStats(contentType);
            var playersByGuid = _trackingController.EntityController
                .GetAllEntities()
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Key, x => x.Value);
            var fragments = MobDamageMeterFragmentFactory.Create(
                mobStats,
                (playerGuid, spellIndex) => ResolvePlayerSpellItemIndex(playersByGuid, playerGuid, spellIndex));

            await Application.Current.Dispatcher.InvokeAsync(
                () => _mainWindowViewModel.DamageMeterBindings.SetMobDamageMeter(fragments));
        }
        catch (Exception e)
        {
            Log.Warning(e, "Mob Damage Meter UI update failed");
        }
        finally
        {
            _isUiUpdateActive = false;
        }
    }

    private static void RecordLastContributionWeapon(PlayerGameObject player, DamageMeterPlayerStats contentStats)
    {
        var weaponItemIndex = DamageMeterWeaponResolver.GetEquippedWeaponItemIndex(player);
        if (weaponItemIndex <= 0)
        {
            return;
        }

        player.LastContributionWeaponItemIndex = weaponItemIndex;
        lock (contentStats.SyncRoot)
        {
            contentStats.LastContributionWeaponItemIndex = weaponItemIndex;
        }
    }

    private int ResolvePlayerSpellItemIndex(
        IReadOnlyDictionary<Guid, PlayerGameObject> playersByGuid,
        Guid playerGuid,
        int spellIndex)
    {
        if (!playersByGuid.TryGetValue(playerGuid, out var player))
        {
            return 0;
        }

        return GetSpellItemIndex(spellIndex, player);
    }

    private async Task UpdateDamageMeterUiCoreAsync(ObservableCollection<DamageMeterFragment> damageMeter, List<KeyValuePair<Guid, PlayerGameObject>> entities)
    {
        var maximumDamage = entities.GetCurrentTotalDamage();
        var maximumHeal = entities.GetCurrentTotalHeal();
        var maximumTakenDamage = entities.GetCurrentTotalTakenDamage();

        var fragmentsByCauser = damageMeter
            .GroupBy(x => x.CauserGuid)
            .ToDictionary(x => x.Key, x => x.First());

        foreach (var healthChangeObject in entities)
        {
            if (healthChangeObject.Value?.UserGuid == null)
            {
                continue;
            }

            fragmentsByCauser.TryGetValue(healthChangeObject.Value.UserGuid, out var fragment);
            if (fragment != null)
            {
                await UpdateDamageMeterFragmentAsync(fragment, healthChangeObject, entities, maximumDamage, maximumHeal, maximumTakenDamage);
            }
            else
            {
                await AddDamageMeterFragmentAsync(damageMeter, healthChangeObject, entities, maximumDamage, maximumHeal, maximumTakenDamage).ConfigureAwait(true);
            }
        }

        await RemoveDuplicatesAsync(_mainWindowViewModel?.DamageMeterBindings?.DamageMeter);
        Application.Current.Dispatcher.Invoke(() => _mainWindowViewModel.DamageMeterBindings?.SetDamageMeterSort());
    }

    private static async Task UpdateDamageMeterFragmentAsync(DamageMeterFragment fragment, KeyValuePair<Guid, PlayerGameObject> healthChangeObject,
        List<KeyValuePair<Guid, PlayerGameObject>> entities, long maximumDamage, long maximumHeal, long maximumTakenDamage)
    {
        var healthChangeObjectValue = healthChangeObject.Value;
        var combatTime = healthChangeObjectValue.GetCombatTime(DateTime.UtcNow);

        fragment.CauserMainHand = DamageMeterWeaponResolver.GetWeaponByIndex(
            healthChangeObjectValue.LastContributionWeaponItemIndex);

        // Damage
        fragment.DamageInPercent = CalculateBarPercentage(healthChangeObjectValue.Damage, maximumDamage);
        fragment.Damage = healthChangeObjectValue.Damage;
        fragment.Dps = healthChangeObjectValue.Dps;

        // Heal
        fragment.HealInPercent = CalculateBarPercentage(healthChangeObjectValue.Heal, maximumHeal);
        fragment.Heal = healthChangeObjectValue.Heal;
        fragment.Hps = healthChangeObjectValue.Hps;
        fragment.Overhealed = healthChangeObjectValue.Overhealed;

        // Taken Damage
        fragment.TakenDamageInPercent = CalculateBarPercentage(healthChangeObjectValue.TakenDamage, maximumTakenDamage);
        fragment.TakenDamage = healthChangeObjectValue.TakenDamage;

        // Spells
        await AddOrUpdateSpellFragmentAsync(fragment.Spells, healthChangeObjectValue?.Spells);

        // Generally
        if (healthChangeObjectValue != null)
        {
            fragment.CombatTime = combatTime;
            fragment.DamagePercentage = entities.GetDamagePercentage(healthChangeObjectValue.Damage);
            fragment.HealPercentage = entities.GetHealPercentage(healthChangeObjectValue.Heal);
            fragment.TakenDamagePercentage = entities.GetTakenDamagePercentage(healthChangeObjectValue.TakenDamage);
            fragment.OverhealedPercentageOfTotalHealing = GetOverhealedPercentageOfHealWithOverhealed(healthChangeObjectValue.Overhealed, healthChangeObjectValue.Heal);
        }
    }

    public static double GetOverhealedPercentageOfHealWithOverhealed(double overhealed, double heal)
    {
        var totalHealing = heal + overhealed;
        return totalHealing > 0
            ? overhealed / totalHealing * 100
            : 0;
    }

    private static double CalculateBarPercentage(long value, long maximum)
    {
        return value > 0 && maximum > 0 ? (double) value / maximum * 100 : 0;
    }

    private static async Task AddDamageMeterFragmentAsync(ICollection<DamageMeterFragment> damageMeter, KeyValuePair<Guid, PlayerGameObject> healthChangeObject,
        List<KeyValuePair<Guid, PlayerGameObject>> entities, long maximumDamage, long maximumHeal, long maximumTakenDamage)
    {
        if (healthChangeObject.Value == null
            || (double.IsNaN(healthChangeObject.Value.Damage) && double.IsNaN(healthChangeObject.Value.Heal) && double.IsNaN(healthChangeObject.Value.Overhealed))
            || (healthChangeObject.Value.Damage <= 0 && healthChangeObject.Value.Heal <= 0 && healthChangeObject.Value.Overhealed <= 0 && healthChangeObject.Value.TakenDamage <= 0))
        {
            return;
        }

        var healthChangeObjectValue = healthChangeObject.Value;
        var combatTime = healthChangeObjectValue.GetCombatTime(DateTime.UtcNow);
        var item = DamageMeterWeaponResolver.GetWeaponByIndex(
            healthChangeObjectValue.LastContributionWeaponItemIndex);

        var spells = new ObservableCollection<UsedSpellFragment>();
        await AddOrUpdateSpellFragmentAsync(spells, healthChangeObjectValue.Spells);

        var damageMeterFragment = new DamageMeterFragment
        {
            CauserGuid = healthChangeObjectValue.UserGuid,
            CombatTime = combatTime,
            Damage = healthChangeObjectValue.Damage,
            Dps = healthChangeObjectValue.Dps,
            DamageInPercent = CalculateBarPercentage(healthChangeObjectValue.Damage, maximumDamage),
            DamagePercentage = entities.GetDamagePercentage(healthChangeObjectValue.Damage),

            Heal = healthChangeObjectValue.Heal,
            Hps = healthChangeObjectValue.Hps,
            HealInPercent = CalculateBarPercentage(healthChangeObjectValue.Heal, maximumHeal),
            HealPercentage = entities.GetHealPercentage(healthChangeObjectValue.Heal),
            Overhealed = healthChangeObjectValue.Overhealed,
            OverhealedPercentageOfTotalHealing = GetOverhealedPercentageOfHealWithOverhealed(healthChangeObjectValue.Overhealed, healthChangeObjectValue.Heal),

            TakenDamage = healthChangeObjectValue.TakenDamage,
            TakenDamageInPercent = CalculateBarPercentage(healthChangeObjectValue.TakenDamage, maximumTakenDamage),
            TakenDamagePercentage = entities.GetTakenDamagePercentage(healthChangeObjectValue.TakenDamage),

            Name = healthChangeObjectValue.Name,
            CauserMainHand = item,

            Spells = spells
        };

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            damageMeter.Add(damageMeterFragment);
        });
    }

    private static bool HasDamageMeterDupes(IEnumerable<DamageMeterFragment> damageMeter)
    {
        return damageMeter?.ToList().GroupBy(x => x?.Name).Any(g => g.Count() > 1) ?? false;
    }

    private static async Task RemoveDuplicatesAsync(ICollection<DamageMeterFragment> damageMeter)
    {
        if (!HasDamageMeterDupes(damageMeter))
        {
            return;
        }

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var damageMeterWithoutDupes = (from dmf in damageMeter.ToList()
                                           group dmf by dmf.Name into x
                                           select new DamageMeterFragment(x.FirstOrDefault())).ToList();

            if (damageMeterWithoutDupes.Count <= 0)
            {
                return;
            }

            foreach (var damageMeterFragment in damageMeter.ToList())
            {
                if (damageMeterWithoutDupes.Any(x => x.Equals(damageMeterFragment)))
                {
                    damageMeter.Remove(damageMeterFragment);
                }
            }
        });
    }

    public void ResetDamageMeterByClusterChange()
    {
        if (!_mainWindowViewModel.DamageMeterBindings?.IsDamageMeterResetByMapChangeActive ?? false)
        {
            return;
        }

        ResetDamageMeter();
        LastPlayersHealth.Clear();
    }

    public void ResetDamageMeterBeforeCombatStart(long objectId, bool inActiveCombat, bool inPassiveCombat)
    {
        if (!_combatModeWasCombatOver)
        {
            return;
        }

        if (!inActiveCombat && !inPassiveCombat)
        {
            return;
        }

        if (!_mainWindowViewModel.DamageMeterBindings?.IsDamageMeterResetBeforeCombatActive ?? false)
        {
            return;
        }

        if (!_trackingController.EntityController.IsEntityInParty(objectId))
        {
            return;
        }

        ResetDamageMeter();
        LastPlayersHealth.Clear();

        _combatModeWasCombatOver = false;
    }

    private void SetLastCombatMode(long objectId, bool inActiveCombat, bool inPassiveCombat)
    {
        if (!_trackingController.EntityController.IsEntityInParty(objectId))
        {
            return;
        }

        if (!inActiveCombat && !inPassiveCombat)
        {
            _combatModeWasCombatOver = true;
        }
    }

    public void ResetDamageMeter()
    {
        lock (_damageStatsUiUpdateLock)
        {
            _damageStatsVersion++;
            _isDamageStatsUiUpdateActive = false;
            _lastDamageStatsUiUpdate = DateTime.MinValue;
        }

        _damageStatsTracker.Clear();
        _damageStatsTrackersByContent.Clear();
        CombatEventTracker.ClearCombatEvents();
        _trackingController.EntityController.ResetDamageMeterContentStats();
        _trackingController.EntityController.ResetEntitiesDamageTimes();
        _trackingController.EntityController.ResetEntitiesDamage();
        _trackingController.EntityController.ResetEntitiesHeal();
        _trackingController.EntityController.ResetEntitiesTakeDamage();
        _trackingController.EntityController.ResetSpells();
        _trackingController.EntityController.ResetEntitiesHealAndOverhealed();
        _trackingController.EntityController.ResetEntitiesDamageStartTime();

        Application.Current?.Dispatcher?.InvokeAsync(() =>
        {
            _mainWindowViewModel?.DamageMeterBindings?.DamageMeter?.Clear();
            _mainWindowViewModel?.DamageMeterBindings?.ClearMobDamageMeter();
            _mainWindowViewModel?.DamageMeterBindings?.ClearDamageStats();
        });
    }

    public ConcurrentDictionary<Guid, double> LastPlayersHealth = new();

    public bool IsMaxHealthReached(long objectId, double newHealthValue)
    {
        var gameObject = _trackingController?.EntityController?.GetEntity(objectId);
        var playerHealth = LastPlayersHealth?.ToArray().FirstOrDefault(x => x.Key == gameObject?.Value?.UserGuid);
        if (playerHealth?.Value.CompareTo(newHealthValue) == 0)
        {
            return true;
        }

        SetLastPlayersHealth(gameObject?.Value?.UserGuid, newHealthValue);
        return false;
    }

    private void SetLastPlayersHealth(Guid? userGuid, double value)
    {
        if (userGuid is not { } notNullGuid)
        {
            return;
        }

        if (LastPlayersHealth.ContainsKey(notNullGuid))
        {
            LastPlayersHealth[notNullGuid] = value;
        }
        else
        {
            try
            {
                LastPlayersHealth.TryAdd(notNullGuid, value);
            }
            catch (Exception e)
            {
                Log.Warning(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            }
        }
    }

    private static HealthChangeType GetHealthChangeType(double healthChange) => healthChange <= 0 ? HealthChangeType.Damage : HealthChangeType.Heal;

    private DateTime _lastDamageUiUpdate;

    private bool IsUiUpdateRequired(int waitTimeInSeconds = 1)
    {
        var difference = DateTime.UtcNow.Subtract(_lastDamageUiUpdate);
        return difference.TotalSeconds >= waitTimeInSeconds && !_isUiUpdateActive;
    }

    private bool IsUiUpdateAllowed(int waitTimeInSeconds = 1)
    {
        var currentDateTime = DateTime.UtcNow;
        var difference = currentDateTime.Subtract(_lastDamageUiUpdate);
        if (difference.TotalSeconds >= waitTimeInSeconds && !_isUiUpdateActive)
        {
            _lastDamageUiUpdate = currentDateTime;
            return true;
        }

        return false;
    }

    private void OnDamageMeterContentFilterChanged(DashboardContentType? contentType)
    {
        lock (_damageStatsUiUpdateLock)
        {
            _damageStatsVersion++;
            _isDamageStatsUiUpdateActive = false;
            _lastDamageStatsUiUpdate = DateTime.MinValue;
        }

        _lastDamageUiUpdate = DateTime.MinValue;
        _isUiUpdateActive = false;

        Application.Current?.Dispatcher?.Invoke(() =>
        {
            _mainWindowViewModel.DamageMeterBindings.DamageMeter.Clear();
            _mainWindowViewModel.DamageMeterBindings.ClearMobDamageMeter();
            _mainWindowViewModel.DamageMeterBindings.ClearDamageStats();
        });

        UpdateDamageMeterUiForSelectedContent();
        UpdateDamageStatsUiIfAllowed();
    }

    private void OnDamageMeterDisplayChanged()
    {
        _lastDamageUiUpdate = DateTime.MinValue;
        _isUiUpdateActive = false;
        UpdateDamageMeterUiForSelectedContent();
    }

    private DamageMeterSnapshot CreateDamageMeterSnapshot()
    {
        var playersByGuid = _trackingController.EntityController
            .GetAllEntities()
            .Where(x => x.Value != null)
            .ToDictionary(x => x.Key, x => x.Value);
        var snapshot = new DamageMeterSnapshot
        {
            AllContent = CreateDamageMeterContentSnapshot(null, playersByGuid)
        };

        foreach (var contentType in DashboardContentTypeResolver.ContentTypes)
        {
            var contentSnapshot = CreateDamageMeterContentSnapshot(contentType, playersByGuid);
            if (contentSnapshot.HasData)
            {
                snapshot.ContentSnapshots[contentType] = contentSnapshot;
            }
        }

        snapshot.ApplyContentFilter(null);
        return snapshot;
    }

    private DamageMeterContentSnapshot CreateDamageMeterContentSnapshot(
        DashboardContentType? contentType,
        IReadOnlyDictionary<Guid, PlayerGameObject> playersByGuid)
    {
        var entities = _trackingController.EntityController
            .GetAllEntitiesWithDamageOrHealAndInParty(contentType);
        var activePlayerGuids = entities.Select(x => x.Key).ToList();
        var healingPlayerGuids = entities
            .Where(x => x.Value.Heal > 0)
            .Select(x => x.Key)
            .ToList();
        var trackerSnapshot = GetDamageStatsTracker(contentType)
            .CreateSnapshot(activePlayerGuids, healingPlayerGuids);
        var combatEvents = CombatEventTracker.GetCombatEvents(contentType);
        var mobDamageMeter = MobDamageMeterFragmentFactory.Create(
            CombatEventTracker.GetMobDamageStats(contentType),
            (playerGuid, spellIndex) => ResolvePlayerSpellItemIndex(playersByGuid, playerGuid, spellIndex));

        return DamageMeterContentSnapshotFactory.Create(
            entities,
            trackerSnapshot,
            combatEvents,
            CreateYourStatsSnapshot(contentType, combatEvents),
            mobDamageMeter);
    }

    private void UpdateDamageStatsUiIfAllowed()
    {
        if (!IsDamageStatsUiUpdateAllowed())
        {
            return;
        }

        int damageStatsVersion;

        lock (_damageStatsUiUpdateLock)
        {
            damageStatsVersion = _damageStatsVersion;
        }

        _ = UpdateDamageStatsUiAsync(damageStatsVersion);
    }

    private async Task UpdateDamageStatsUiAsync(int damageStatsVersion)
    {
        try
        {
            var selectedContentType = _mainWindowViewModel.DamageMeterBindings.DamageMeterContentFilterSelection?.ContentType;
            var activeEntities = _trackingController.EntityController
                .GetAllEntitiesWithDamageOrHealAndInParty(selectedContentType);
            var damageStatsTracker = GetDamageStatsTracker(selectedContentType);

            var activePlayerGuids = activeEntities
                .Select(x => x.Key)
                .ToList();

            var healingPlayerGuids = activeEntities
                .Where(x => x.Value.Heal > 0)
                .Select(x => x.Key)
                .ToList();

            var combatEvents = CombatEventTracker.GetCombatEvents(selectedContentType);
            var trackerSnapshot = damageStatsTracker.CreateSnapshot(activePlayerGuids, healingPlayerGuids);
            var damageStatsSnapshot = DamageStatsSnapshotFactory.FromLiveData(
                trackerSnapshot,
                activeEntities.Select(x => x.Value),
                combatEvents);
            var yourStatsSnapshot = CreateYourStatsSnapshot(selectedContentType, combatEvents);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var canApplySnapshot = false;

                lock (_damageStatsUiUpdateLock)
                {
                    canApplySnapshot = damageStatsVersion == _damageStatsVersion;
                }

                if (canApplySnapshot)
                {
                    _mainWindowViewModel.DamageMeterBindings.SetLocalPlayer(_trackingController.EntityController.LocalUserData.Guid, _trackingController.EntityController.LocalUserData.Username);
                    _mainWindowViewModel.DamageMeterBindings.SetDamageStats(damageStatsSnapshot);
                    _mainWindowViewModel.DamageMeterBindings.SetYourStats(yourStatsSnapshot);
                }
            });
        }
        catch (Exception e)
        {
            Log.Warning(e, "Damage stats UI update failed");
        }
        finally
        {
            lock (_damageStatsUiUpdateLock)
            {
                _isDamageStatsUiUpdateActive = false;
            }
        }
    }


    private DamageStatsTracker GetDamageStatsTracker(DashboardContentType? contentType)
    {
        if (!contentType.HasValue)
        {
            return _damageStatsTracker;
        }

        return _damageStatsTrackersByContent.TryGetValue(contentType.Value, out var tracker)
            ? tracker
            : new DamageStatsTracker();
    }

    private DamageMeterYourStatsSnapshot CreateYourStatsSnapshot(
        DashboardContentType? contentType,
        IEnumerable<CombatEvent> combatEvents)
    {
        var localUserData = _trackingController.EntityController.LocalUserData;
        PlayerGameObject localPlayer = null;

        if (localUserData.Guid.HasValue)
        {
            localPlayer = _trackingController.EntityController.GetEntity(localUserData.Guid.Value).Value;
        }

        if (localPlayer == null && localUserData.UserObjectId.HasValue)
        {
            localPlayer = _trackingController.EntityController.GetEntity(localUserData.UserObjectId.Value)?.Value;
        }

        if (contentType.HasValue && localPlayer != null)
        {
            localPlayer = localPlayer.CreateDamageMeterContentView(contentType.Value);
        }

        return DamageMeterYourStatsSnapshotFactory.FromLiveData(
            localPlayer,
            combatEvents,
            ResolveObjectName);
    }

    private string ResolveObjectName(long objectId)
    {
        var entity = _trackingController.EntityController.GetEntity(objectId);
        if (entity.HasValue && !string.IsNullOrWhiteSpace(entity.Value.Value.Name))
        {
            return entity.Value.Value.Name;
        }

        var mob = CombatEventTracker.GetKnownMobOrDefault(objectId);
        if (mob?.MobData != null)
        {
            var localizedName = MobsData.GetLocalizedMobName(mob.MobData);
            if (!string.IsNullOrWhiteSpace(localizedName))
            {
                return localizedName;
            }
        }

        if (!string.IsNullOrWhiteSpace(mob?.UniqueName))
        {
            return mob.UniqueName;
        }

        return objectId.ToString();
    }

    private bool IsDamageStatsUiUpdateAllowed()
    {
        lock (_damageStatsUiUpdateLock)
        {
            if (_isDamageStatsUiUpdateActive)
            {
                return false;
            }

            var currentDateTime = DateTime.UtcNow;
            if (currentDateTime.Subtract(_lastDamageStatsUiUpdate).TotalMilliseconds < DamageStatsUiUpdateIntervalInMilliseconds)
            {
                return false;
            }

            _isDamageStatsUiUpdateActive = true;
            _lastDamageStatsUiUpdate = currentDateTime;
            return true;
        }
    }

    private void AddOrUpdateSpell(int causingSpellIndex, PlayerGameObject playerGameObject, ICollection<UsedSpell> spells, HealthChangeType healthChangeType, int healthChangeValue)
    {
        if (causingSpellIndex <= 0)
        {
            var autoAttack = spells.FirstOrDefault(x => x.SpellIndex == 0);
            if (autoAttack is not null)
            {
                autoAttack.DamageHealValue += healthChangeValue;
                autoAttack.Ticks++;
            }
            else
            {
                spells.Add(new UsedSpell(0, 0)
                {
                    UniqueName = "AUTO_ATTACK",
                    Category = "damage",
                    DamageHealValue = healthChangeValue,
                    HealthChangeType = healthChangeType,
                    Ticks = 1
                });
            }

            return;
        }

        var itemIndex = GetSpellItemIndex(causingSpellIndex, playerGameObject);
        var spell = spells.FirstOrDefault(x => x.SpellIndex == causingSpellIndex && x.HealthChangeType == healthChangeType);
        if (spell is not null)
        {
            spell.HealthChangeType = healthChangeType;
            spell.DamageHealValue += healthChangeValue;
            if (itemIndex > 0)
            {
                spell.ItemIndex = itemIndex;
            }

            spell.Ticks++;
        }
        else
        {
            spells.Add(new UsedSpell(causingSpellIndex, itemIndex)
            {
                ItemIndex = itemIndex,
                HealthChangeType = healthChangeType,
                DamageHealValue = healthChangeValue,
                Ticks = 1
            });
        }
    }

    private int GetSpellItemIndex(int causingSpellIndex, PlayerGameObject playerGameObject)
    {
        var equipment = playerGameObject.CharacterEquipment;
        if (equipment is null)
        {
            return 0;
        }

        var causingSpellUniqueName = SpellData.GetUniqueName(causingSpellIndex);
        var spellSlot = equipment.ActiveSpells.LastOrDefault(x => IsMatchingSpell(causingSpellIndex, causingSpellUniqueName, x.Value));
        if (spellSlot is null)
        {
            var slotType = GetSlotTypeBySpellUniqueName(causingSpellUniqueName);
            return slotType == SlotType.Unknown ? equipment.MainHand : GetItemIndexBySlotType(equipment, slotType);
        }

        return spellSlot.ItemIndex > 0 ? GetBasePotionItemIndex(spellSlot.ItemIndex) : GetItemIndexBySlotType(equipment, spellSlot.SlotType);
    }

    private static bool IsMatchingSpell(int causingSpellIndex, string causingSpellUniqueName, int slotSpellIndex)
    {
        if (slotSpellIndex == causingSpellIndex)
        {
            return true;
        }

        var slotSpellUniqueName = SpellData.GetUniqueName(slotSpellIndex);
        return !string.IsNullOrWhiteSpace(causingSpellUniqueName)
               && !string.IsNullOrWhiteSpace(slotSpellUniqueName)
               && causingSpellUniqueName.StartsWith(slotSpellUniqueName, StringComparison.OrdinalIgnoreCase);
    }

    private static SlotType GetSlotTypeBySpellUniqueName(string spellUniqueName)
    {
        if (string.IsNullOrWhiteSpace(spellUniqueName))
        {
            return SlotType.Unknown;
        }

        if (spellUniqueName.StartsWith("PASSIVECAPE_", StringComparison.OrdinalIgnoreCase)
            || spellUniqueName.StartsWith("PASSIVE_CAPE_", StringComparison.OrdinalIgnoreCase)
            || spellUniqueName.StartsWith("CAPE_", StringComparison.OrdinalIgnoreCase))
        {
            return SlotType.Cape;
        }

        if (spellUniqueName.StartsWith("POTION_", StringComparison.OrdinalIgnoreCase))
        {
            return SlotType.Potion;
        }

        if (spellUniqueName.StartsWith("FOOD_", StringComparison.OrdinalIgnoreCase))
        {
            return SlotType.Food;
        }

        return SlotType.Unknown;
    }

    private static int GetItemIndexBySlotType(CharacterEquipment equipment, SlotType slotType)
    {
        var itemIndex = slotType switch
        {
            SlotType.MainHand => equipment.MainHand,
            SlotType.OffHand => equipment.OffHand,
            SlotType.Cape => equipment.Cape,
            SlotType.Bag => equipment.Bag,
            SlotType.Armor => equipment.Chest,
            SlotType.Head => equipment.Head,
            SlotType.Shoes => equipment.Shoes,
            SlotType.Mount => equipment.Mount,
            SlotType.Potion => equipment.Potion,
            SlotType.Food => equipment.BuffFood,
            _ => equipment.MainHand
        };

        return slotType == SlotType.Potion ? GetBasePotionItemIndex(itemIndex) : itemIndex;
    }

    private static int GetBasePotionItemIndex(int itemIndex)
    {
        if (itemIndex <= 0)
        {
            return 0;
        }

        var item = ItemController.GetItemByIndex(itemIndex);
        if (item == null
            || item.Level == 0
            || item.FullItemInformation is not ConsumableItem consumableItem
            || consumableItem.SlotTypeEnum != SlotType.Potion)
        {
            return itemIndex;
        }

        return ItemController.GetItemByUniqueName(ItemController.GetCleanUniqueName(item.UniqueName))?.Index ?? itemIndex;
    }

    private static async Task AddOrUpdateSpellFragmentAsync(ObservableCollection<UsedSpellFragment> spellsFragments, IReadOnlyCollection<UsedSpell> spells)
    {
        var fragmentsToAdd = new List<UsedSpellFragment>();
        var fragmentsToUpdate = new List<UsedSpellFragment>();
        var totalDamage = spells.Sum(spell => spell?.DamageHealValue) ?? 0;
        var maxDamage = spells.Max(spell => spell?.DamageHealValue) ?? 0;

        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            await foreach (var spell in spells.ToList().ToAsyncEnumerable())
            {
                var existingFragment = spellsFragments.FirstOrDefault(x => x.SpellIndex == spell.SpellIndex && x.HealthChangeType == spell.HealthChangeType);

                if (existingFragment != null)
                {
                    existingFragment.ItemIndex = spell.ItemIndex;
                    existingFragment.DamageHealValue = spell.DamageHealValue;
                    existingFragment.Ticks = spell.Ticks;
                    existingFragment.DamageInPercent = (maxDamage != 0) ? (double) spell.DamageHealValue / maxDamage * 100 : 0;
                    existingFragment.DamagePercentage = (totalDamage != 0) ? 100.00 / totalDamage * spell.DamageHealValue : 0;
                    fragmentsToUpdate.Add(existingFragment);
                }
                else
                {
                    var damageInPercent = (maxDamage != 0) ? (double) spell.DamageHealValue / maxDamage * 100 : 0;
                    var damagePercentage = (totalDamage != 0) ? 100.00 / totalDamage * spell.DamageHealValue : 0;

                    fragmentsToAdd.Add(new UsedSpellFragment
                    {
                        SpellIndex = spell.SpellIndex,
                        ItemIndex = spell.ItemIndex,
                        UniqueName = spell.UniqueName,
                        DamageHealValue = spell.DamageHealValue,
                        Category = spell.Category,
                        Target = spell.Target,
                        Ticks = spell.Ticks,
                        HealthChangeType = spell.HealthChangeType,
                        DamageInPercent = damageInPercent > 100 ? 100 : damageInPercent,
                        DamagePercentage = damagePercentage > 100 ? 100 : damagePercentage
                    });
                }
            }

            foreach (var fragment in fragmentsToAdd)
            {
                spellsFragments.Add(fragment);
            }

            foreach (var fragment in fragmentsToUpdate)
            {
                var updatedFragment = fragmentsToAdd.FirstOrDefault(x => x.SpellIndex == fragment.SpellIndex);
                if (updatedFragment != null)
                {
                    fragment.DamageHealValue = updatedFragment.DamageHealValue;
                    fragment.Ticks = updatedFragment.Ticks;
                    fragment.DamageInPercent = updatedFragment.DamageInPercent;
                    fragment.DamagePercentage = updatedFragment.DamagePercentage;
                }
            }

            spellsFragments.SortByDescending(x => x.DamageHealValue);
        });
    }

    #endregion

    #region Combat Mode / Combat Timer

    public event Action<long, bool, bool> OnChangeCombatMode;

    public void UpdateCombatMode(long objectId, bool inActiveCombat, bool inPassiveCombat)
    {
        OnChangeCombatMode?.Invoke(objectId, inActiveCombat, inPassiveCombat);
    }

    private void AddCombatTime(long objectId, bool inActiveCombat, bool inPassiveCombat)
    {
        if (!_trackingController.EntityController.IsEntityInParty(objectId))
        {
            return;
        }

        var player = _trackingController.EntityController.GetEntity(objectId)?.Value;
        if (player == null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (inActiveCombat || inPassiveCombat)
        {
            player.StartCombatInterval(now);
            player.GetOrCreateDamageMeterContentStats(GetCurrentContentType()).StartCombatInterval(now);
            return;
        }

        player.EndCombatInterval(now);
        player.EndDamageMeterContentCombatIntervals(now);
    }

    #endregion

    #region Debug methods

    private static readonly Random Random = new(DateTime.Now.Millisecond);

    private async void RunDamageMeterDebugAsync(int player = 20, int damageRuns = 100)
    {
        var entities = SetRandomDamageValues(player);
        var tasks = new List<Task>();

        foreach (var entity in entities)
        {
            tasks.Add(AddDamageAsync(entity.Value, damageRuns));
        }

        await Task.WhenAll(tasks);
    }

    private async Task AddDamageAsync(PlayerGameObject entity, int runs)
    {
        for (var i = 0; i < runs; i++)
        {
            var damage = Random.Next(-5000, 5000);
            var takenDamage = Random.Next(-5000, 5000);
            await AddDamage(9999, entity.ObjectId ?? -1, damage, Random.Next(2000, 3000), Random.Next(2000, 3000));
            await AddTakenDamage(entity.ObjectId ?? -1, 9999, takenDamage, Random.Next(2000, 3000), Random.Next(2000, 3000));
            //Debug.Print($"--- AddDamage - {entity.Name}: {damage}");

            await Task.Delay(Random.Next(1, 1000));
        }
    }

    private List<KeyValuePair<Guid, PlayerGameObject>> SetRandomDamageValues(int playerAmount)
    {
        for (var i = 0; i < playerAmount; i++)
        {
            var guid = new Guid($"{Random.Next(1000, 9999)}0000-0000-0000-0000-000000000000");
            var interactGuid = Guid.NewGuid();
            var name = TestMethods.GenerateName(Random.Next(3, 10));
            var guildName = TestMethods.GenerateName(Random.Next(4, 10));
            var allianceName = TestMethods.GenerateName(Random.Next(5, 10));
            var charItem = new CharacterEquipment()
            {
                MainHand = TestMethods.GetRandomWeaponIndex(),
                OffHand = 0,
                Head = Random.Next(7729, 7735),
                Chest = Random.Next(2887, 2900),
                Shoes = Random.Next(2905, 2914),
                Bag = Random.Next(2109, 2114),
                Cape = Random.Next(1867, 1874)
            };

            _trackingController?.EntityController?.AddEntity(new Entity
            {
                ObjectId = i,
                UserGuid = guid,
                InteractGuid = interactGuid,
                Name = name,
                Guild = guildName,
                Alliance = allianceName,
                CharacterEquipment = charItem,
                ObjectType = GameObjectType.Player,
                ObjectSubType = GameObjectSubType.Mob
            });
            _trackingController?.EntityController?.AddToPartyAsync(guid);
        }

        return _trackingController?.EntityController?.GetAllEntities();
    }

    #endregion

    #region Load / Save local file data

    public async Task LoadFromFileAsync()
    {
        var dto = await FileController.LoadAsync<List<DamageMeterSnapshotDto>>(
            AppDataPaths.UserDataFile(Settings.Default.DamageMeterSnapshotsFileName));
        var damageMeterSnapshot = dto.Select(SnapshotMapping.Mapping);

        _mainWindowViewModel.DamageMeterBindings.DamageMeterSnapshots = damageMeterSnapshot.ToList();
    }

    public async Task SaveInFileAsync()
    {
        if (!AppDataPaths.TryEnsureUserDataDirectory())
        {
            return;
        }

        await FileController.SaveAsync(_mainWindowViewModel.DamageMeterBindings?.DamageMeterSnapshots?.Select(SnapshotMapping.Mapping),
            AppDataPaths.UserDataFile(Settings.Default.DamageMeterSnapshotsFileName));
        Log.Information("Damage Meter snapshots saved");
    }

    #endregion
}
