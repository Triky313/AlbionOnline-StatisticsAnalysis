using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.DamageMeter;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager;

public class CombatController
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly TrackingController _trackingController;
    private bool _combatModeWasCombatOver;

    public CombatController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;

        OnChangeCombatMode += AddCombatTime;
        OnChangeCombatMode += SetLastCombatMode;
        OnChangeCombatMode += ResetDamageMeterBeforeCombatStart;
        OnDamageUpdate += UpdateDamageMeterUiAsync;

#if DEBUG
        RunDamageMeterDebugAsync(10, 10);
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

        var gameObject = _trackingController?.EntityController?.GetEntity(causerId);
        var gameObjectValue = gameObject?.Value;

        if (gameObject?.Value is not { ObjectType: GameObjectType.Player } || !_trackingController.EntityController.IsEntityInParty(gameObject.Value.Key))
        {
            return Task.CompletedTask;
        }

        if (healthChangeType == HealthChangeType.Damage)
        {
            var damageChangeValue = (int) Math.Round(healthChange.ToPositiveFromNegativeOrZero(), MidpointRounding.AwayFromZero);
            if (damageChangeValue <= 0)
            {
                return Task.CompletedTask;
            }

            gameObjectValue.Damage += damageChangeValue;
            AddOrUpdateSpell(causingSpellIndex, gameObjectValue, healthChangeType, damageChangeValue, gameObjectValue.CharacterEquipment?.MainHand ?? 0);
        }

        if (healthChangeType == HealthChangeType.Heal)
        {
            var healChangeValue = healthChange;
            if (healChangeValue <= 0)
            {
                return Task.CompletedTask;
            }

            var positiveHealChangeValue = (int) Math.Round(healChangeValue, MidpointRounding.AwayFromZero);
            if (!IsMaxHealthReached(affectedId, newHealthValue))
            {
                gameObjectValue.Heal += positiveHealChangeValue;
                AddOrUpdateSpell(causingSpellIndex, gameObjectValue, healthChangeType, positiveHealChangeValue, gameObjectValue.CharacterEquipment?.MainHand ?? 0);
            }
            else
            {
                gameObjectValue.Overhealed += positiveHealChangeValue;
            }
        }

        gameObjectValue.CombatStart ??= DateTime.UtcNow;

        OnDamageUpdate?.Invoke(_mainWindowViewModel?.DamageMeterBindings?.DamageMeter, _trackingController.EntityController.GetAllEntitiesWithDamageOrHealAndInParty());
        return Task.CompletedTask;
    }

    private static bool _isUiUpdateActive;

    public async void UpdateDamageMeterUiAsync(ObservableCollection<DamageMeterFragment> damageMeter, List<KeyValuePair<Guid, PlayerGameObject>> entities)
    {
        if (!IsUiUpdateAllowed())
        {
            return;
        }

        _isUiUpdateActive = true;

        var currentTotalDamage = entities.GetCurrentTotalDamage();
        var currentTotalHeal = entities.GetCurrentTotalHeal();

        _trackingController.EntityController.DetectUsedWeapon();

        foreach (var healthChangeObject in entities)
        {
            if (healthChangeObject.Value?.UserGuid == null)
            {
                continue;
            }

            var fragment = damageMeter.ToList().FirstOrDefault(x => x.CauserGuid == healthChangeObject.Value.UserGuid);
            if (fragment != null)
            {
                await UpdateDamageMeterFragmentAsync(fragment, healthChangeObject, entities, currentTotalDamage, currentTotalHeal);
            }
            else
            {
                await AddDamageMeterFragmentAsync(damageMeter, healthChangeObject, entities, currentTotalDamage, currentTotalHeal).ConfigureAwait(true);
            }

            Application.Current.Dispatcher.Invoke(() => _mainWindowViewModel.DamageMeterBindings?.SetDamageMeterSort());
        }

        await RemoveDuplicatesAsync(_mainWindowViewModel?.DamageMeterBindings?.DamageMeter);
        _isUiUpdateActive = false;
    }

    private static async Task UpdateDamageMeterFragmentAsync(DamageMeterFragment fragment, KeyValuePair<Guid, PlayerGameObject> healthChangeObject,
        List<KeyValuePair<Guid, PlayerGameObject>> entities, long currentTotalDamage, long currentTotalHeal)
    {
        var healthChangeObjectValue = healthChangeObject.Value;

        if (healthChangeObjectValue?.CharacterEquipment?.MainHand != null)
        {
            var item = ItemController.GetItemByIndex(healthChangeObjectValue.CharacterEquipment?.MainHand);
            fragment.CauserMainHand = ((ItemJsonObject) item?.FullItemInformation)?.ItemType is ItemType.TransformationWeapon or ItemType.Weapon ? item : null;
        }

        // Damage
        if (healthChangeObjectValue?.Damage > 0)
        {
            fragment.DamageInPercent = (double) healthChangeObjectValue.Damage / currentTotalDamage * 100;
            fragment.Damage = healthChangeObjectValue.Damage;
        }

        if (healthChangeObjectValue?.Dps != null)
        {
            fragment.Dps = healthChangeObjectValue.Dps;
        }

        // Heal
        if (healthChangeObjectValue?.Heal > 0)
        {
            fragment.HealInPercent = (double) healthChangeObjectValue.Heal / currentTotalHeal * 100;
            fragment.Heal = healthChangeObjectValue.Heal;
        }

        if (healthChangeObjectValue?.Hps != null)
        {
            fragment.Hps = healthChangeObjectValue.Hps;
        }

        if (healthChangeObjectValue?.Overhealed > 0)
        {
            fragment.Overhealed = healthChangeObjectValue.Overhealed;
        }

        // Spells
        await AddOrUpdateSpellFragmentAsync(fragment.Spells, healthChangeObjectValue?.Spells);

        // Generally
        if (healthChangeObjectValue != null)
        {
            fragment.CombatTime = healthChangeObjectValue.CombatTime;
            fragment.DamagePercentage = entities.GetDamagePercentage(healthChangeObjectValue.Damage);
            fragment.HealPercentage = entities.GetHealPercentage(healthChangeObjectValue.Heal);
            fragment.OverhealedPercentageOfTotalHealing = GetOverhealedPercentageOfHealWithOverhealed(healthChangeObjectValue.Overhealed, healthChangeObjectValue.Heal);
        }
    }

    public static double GetOverhealedPercentageOfHealWithOverhealed(double overhealed, double heal)
    {
        return 100.00 / (heal + overhealed) * overhealed;
    }

    private static async Task AddDamageMeterFragmentAsync(ICollection<DamageMeterFragment> damageMeter, KeyValuePair<Guid, PlayerGameObject> healthChangeObject,
        List<KeyValuePair<Guid, PlayerGameObject>> entities, long currentTotalDamage, long currentTotalHeal)
    {
        if (healthChangeObject.Value == null
            || (double.IsNaN(healthChangeObject.Value.Damage) && double.IsNaN(healthChangeObject.Value.Heal) && double.IsNaN(healthChangeObject.Value.Overhealed))
            || (healthChangeObject.Value.Damage <= 0 && healthChangeObject.Value.Heal <= 0 && healthChangeObject.Value.Overhealed <= 0))
        {
            return;
        }

        var healthChangeObjectValue = healthChangeObject.Value;
        var item = ItemController.GetItemByIndex(healthChangeObject.Value?.CharacterEquipment?.MainHand ?? 0);

        var spells = new ObservableCollection<UsedSpellFragment>();
        await AddOrUpdateSpellFragmentAsync(spells, healthChangeObjectValue.Spells);

        var damageMeterFragment = new DamageMeterFragment
        {
            CauserGuid = healthChangeObjectValue.UserGuid,
            Damage = healthChangeObjectValue.Damage,
            Dps = healthChangeObjectValue.Dps,
            DamageInPercent = (double) healthChangeObjectValue.Damage / currentTotalDamage * 100,
            DamagePercentage = entities.GetDamagePercentage(healthChangeObjectValue.Damage),

            Heal = healthChangeObjectValue.Heal,
            Hps = healthChangeObjectValue.Hps,
            HealInPercent = (double) healthChangeObjectValue.Heal / currentTotalHeal * 100,
            HealPercentage = entities.GetHealPercentage(healthChangeObjectValue.Heal),
            Overhealed = healthChangeObjectValue.Overhealed,
            OverhealedPercentageOfTotalHealing = GetOverhealedPercentageOfHealWithOverhealed(healthChangeObjectValue.Overhealed, healthChangeObjectValue.Heal),

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
        _trackingController.EntityController.ResetEntitiesDamageTimes();
        _trackingController.EntityController.ResetEntitiesDamage();
        _trackingController.EntityController.ResetEntitiesHeal();
        _trackingController.EntityController.ResetSpells();
        _trackingController.EntityController.ResetEntitiesHealAndOverhealed();
        _trackingController.EntityController.ResetEntitiesDamageStartTime();

        Application.Current?.Dispatcher?.InvokeAsync(() =>
        {
            _mainWindowViewModel?.DamageMeterBindings?.DamageMeter?.Clear();
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

    private bool IsUiUpdateAllowed(int waitTimeInSeconds = 1)
    {
        var currentDateTime = DateTime.UtcNow;
        var difference = currentDateTime.Subtract(_lastDamageUiUpdate);
        if (difference.Seconds >= waitTimeInSeconds && !_isUiUpdateActive)
        {
            _lastDamageUiUpdate = currentDateTime;
            return true;
        }

        return false;
    }

    private void AddOrUpdateSpell(int causingSpellIndex, PlayerGameObject playerGameObject, HealthChangeType healthChangeType, int healthChangeValue, int itemIndex)
    {
        if (causingSpellIndex <= 0)
        {
            var autoAttack = playerGameObject.Spells.FirstOrDefault(x => x.SpellIndex == 0);
            if (autoAttack is not null)
            {
                autoAttack.DamageHealValue += healthChangeValue;
                autoAttack.Ticks++;
            }
            else
            {
                playerGameObject.Spells.Add(new UsedSpell(0, 0)
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

        var spell = playerGameObject.Spells.FirstOrDefault(x => x.SpellIndex == causingSpellIndex && x.HealthChangeType == healthChangeType);
        if (spell is not null)
        {
            spell.HealthChangeType = healthChangeType;
            spell.DamageHealValue += healthChangeValue;
            spell.Ticks++;
        }
        else
        {
            playerGameObject.Spells.Add(new UsedSpell(causingSpellIndex, itemIndex)
            {
                ItemIndex = itemIndex,
                HealthChangeType = healthChangeType,
                DamageHealValue = healthChangeValue,
                Ticks = 1
            });
        }
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
                    existingFragment.DamageHealValue = spell.DamageHealValue;
                    existingFragment.Ticks = spell.Ticks;
                    existingFragment.DamageInPercent = (maxDamage != 0) ? (double) spell.DamageHealValue / maxDamage * 100 : 0;
                    existingFragment.DamagePercentage = (totalDamage != 0) ? 100.00 / totalDamage * spell.DamageHealValue : 0;
                    fragmentsToUpdate.Add(existingFragment);
                }
                else
                {
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
                        DamageInPercent = (maxDamage != 0) ? (double) spell.DamageHealValue / maxDamage * 100 : 0,
                        DamagePercentage = (totalDamage != 0) ? 100.00 / totalDamage * spell.DamageHealValue : 0
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

        var playerObject = _trackingController.EntityController.GetEntity(objectId);

        if (playerObject?.Value == null)
        {
            return;
        }

        if ((inActiveCombat || inPassiveCombat) && playerObject.Value.Value.CombatTimes.Any(x => x?.EndTime == null))
        {
            return;
        }

        if (inActiveCombat || inPassiveCombat) playerObject.Value.Value.AddCombatTime(new ActionInterval(DateTime.UtcNow));

        if (!inActiveCombat && !inPassiveCombat)
        {
            var combatTime = playerObject.Value.Value.CombatTimes.FirstOrDefault(x => x.EndTime == null);
            if (combatTime != null)
            {
                combatTime.EndTime = DateTime.UtcNow;
            }
        }
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
            await AddDamage(9999, entity.ObjectId ?? -1, damage, Random.Next(2000, 3000), Random.Next(2000, 3000));
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
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.DamageMeterSnapshotsFileName));
        var damageMeterSnapshot = dto.Select(SnapshotMapping.Mapping);

        _mainWindowViewModel.DamageMeterBindings.DamageMeterSnapshots = damageMeterSnapshot.ToList();
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        await FileController.SaveAsync(_mainWindowViewModel.DamageMeterBindings?.DamageMeterSnapshots?.Select(SnapshotMapping.Mapping),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.DamageMeterSnapshotsFileName));
        Debug.Print("Damage Meter snapshots saved");
    }

    #endregion
}