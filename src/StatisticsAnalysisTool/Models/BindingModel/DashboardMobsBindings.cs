using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Models.BindingModel;

public sealed class DashboardMobsBindings : BaseViewModel
{
    private const string AllFilterValue = "";

    private readonly Dictionary<string, MobKillAggregate> _killStats = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<MobCatalogEntry> _mobCatalog = [];
    private IReadOnlyList<MobCatalogEntry> _filteredMobs = [];
    private int _lastStatisticsHash;
    private bool _hasStatisticsHash;
    private double _rangeHours = 1;
    private string _searchText = string.Empty;
    private string _selectedMobTypeCategory = AllFilterValue;
    private string _selectedFaction = AllFilterValue;

    public IReadOnlyList<DashboardMobListItem> VisibleMobs
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MobCountText));
        }
    } = [];
    public DashboardSummaryMetric TotalKillsSummary { get; } = new();

    public DashboardMobListItem MostKilledMob
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = CreateEmptyMob();

    public DashboardMobListItem LastKilledMob
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = CreateEmptyMob();

    public IReadOnlyList<DashboardMobFilterOption> MobTypeCategoryFilters
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public IReadOnlyList<DashboardMobFilterOption> FactionFilters
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public string SearchText
    {
        get => _searchText;
        set
        {
            var normalizedValue = value ?? string.Empty;
            if (string.Equals(_searchText, normalizedValue, StringComparison.Ordinal))
            {
                return;
            }

            _searchText = normalizedValue;
            ApplyListFilters();
            OnPropertyChanged();
        }
    }

    public string SelectedMobTypeCategory
    {
        get => _selectedMobTypeCategory;
        set
        {
            var normalizedValue = value ?? AllFilterValue;
            if (string.Equals(_selectedMobTypeCategory, normalizedValue, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _selectedMobTypeCategory = normalizedValue;
            ApplyListFilters();
            OnPropertyChanged();
        }
    }

    public string SelectedFaction
    {
        get => _selectedFaction;
        set
        {
            var normalizedValue = value ?? AllFilterValue;
            if (string.Equals(_selectedFaction, normalizedValue, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _selectedFaction = normalizedValue;
            ApplyListFilters();
            OnPropertyChanged();
        }
    }

    public string MobCountText => $"{VisibleMobs.Count:N0}";

    public string TranslationMobs => Translate("MOBS", "Mobs", "Mobs");
    public string TranslationMostKilledMob => Translate("MOST_KILLED_MOB", "Am häufigsten getöteter Mob", "Most killed mob");
    public string TranslationLastKilledMob => Translate("LAST_KILLED_MOB", "Zuletzt getöteter Mob", "Last killed mob");
    public string TranslationKilledMobs => LocalizationController.Translation("KILLED_MOBS");
    public string TranslationSearchMobs => LocalizationController.Translation("SEARCH_MOBS");
    public string TranslationMobTypeCategory => Translate("MOB_TYPE_CATEGORY", "Mob-Typ", "Mob type");
    public string TranslationFaction => LocalizationController.Translation("FACTION");
    public string TranslationFirstAttacked => Translate("FIRST_ATTACKED", "Erstmals angegriffen", "First attacked");
    public string TranslationLastKill => LocalizationController.Translation("LAST_KILL");
    public string TranslationKills => LocalizationController.Translation("KILLS");
    public string TranslationPerHour => LocalizationController.Translation("PER_HOUR");
    public string TranslationOverview => LocalizationController.Translation("OVERVIEW");
    public string TranslationCombatStats => Translate("COMBAT_STATS", "Kampfwerte", "Combat stats");
    public string TranslationAggroBehavior => Translate("AGGRO_BEHAVIOR", "Aggro & Verhalten", "Aggro & behavior");
    public string TranslationSpawnCharges => Translate("SPAWN_CHARGES", "Spawn & Aufladungen", "Spawn & charges");

    public static string TranslationFame => LocalizationController.Translation("FAME");
    public static string TranslationAbilityPower => Translate("ABILITY_POWER", "Fähigkeitsstärke", "Ability power");
    public static string TranslationHealth => Translate("HEALTH", "Lebenspunkte", "Health");
    public static string TranslationEnergy => Translate("ENERGY", "Energie", "Energy");
    public static string TranslationMoveSpeed => Translate("MOVE_SPEED", "Bewegungsgeschwindigkeit", "Move speed");
    public static string TranslationDangerState => Translate("DANGER_STATE", "Gefahrenstatus", "Danger state");
    public static string TranslationEnergyReward => Translate("ENERGY_REWARD", "Energiebelohnung", "Energy reward");
    public static string TranslationMobValue => Translate("MOB_VALUE", "Mob-Wert", "Mob value");
    public static string TranslationAttackDamage => Translate("ATTACK_DAMAGE", "Angriffsschaden", "Attack damage");
    public static string TranslationAttackRange => Translate("ATTACK_RANGE", "Angriffsreichweite", "Attack range");
    public static string TranslationAttackSpeed => Translate("ATTACK_SPEED", "Angriffsgeschwindigkeit", "Attack speed");
    public static string TranslationMeleeDamageTime => Translate("MELEE_DAMAGE_TIME", "Nahkampf-Schadenszeit", "Melee damage time");
    public static string TranslationAttackMoveSpeed => Translate("ATTACK_MOVE_SPEED", "Angriffsbewegung", "Attack move speed");
    public static string TranslationPhysicalArmor => Translate("PHYSICAL_ARMOR", "Physische Rüstung", "Physical armor");
    public static string TranslationMagicResistance => Translate("MAGIC_RESISTANCE", "Magieresistenz", "Magic resistance");
    public static string TranslationCrowdControlResistance => Translate("CROWD_CONTROL_RESISTANCE", "Kontrollresistenz", "Crowd control resistance");
    public static string TranslationAttackCollisionRadius => Translate("ATTACK_COLLISION_RADIUS", "Angriffskollisionsradius", "Attack collision radius");
    public static string TranslationAggroRadius => Translate("AGGRO_RADIUS", "Aggro-Radius", "Aggro radius");
    public static string TranslationAlertRadius => Translate("ALERT_RADIUS", "Alarmradius", "Alert radius");
    public static string TranslationPursuitRadius => Translate("PURSUIT_RADIUS", "Verfolgungsradius", "Pursuit radius");
    public static string TranslationRoamingRadius => Translate("ROAMING_RADIUS", "Streifradius", "Roaming radius");
    public static string TranslationRoamingIdleTime => Translate("ROAMING_IDLE_TIME", "Leerlauf beim Streifen", "Roaming idle time");
    public static string TranslationDamageAggroFactor => Translate("DAMAGE_AGGRO_FACTOR", "Schadens-Aggrofaktor", "Damage aggro factor");
    public static string TranslationHealingAggroFactor => Translate("HEALING_AGGRO_FACTOR", "Heilungs-Aggrofaktor", "Healing aggro factor");
    public static string TranslationShieldAggroFactor => Translate("SHIELD_AGGRO_FACTOR", "Schild-Aggrofaktor", "Shield aggro factor");
    public static string TranslationAggroDelayAfterSpawn => Translate("AGGRO_DELAY_AFTER_SPAWN", "Aggroverzögerung nach Spawn", "Aggro delay after spawn");
    public static string TranslationRespawnTime => Translate("RESPAWN_TIME", "Respawn-Zeit", "Respawn time");
    public static string TranslationMaxCharges => Translate("MAX_CHARGES", "Max. Aufladungen", "Max charges");
    public static string TranslationTimePerCharge => Translate("TIME_PER_CHARGE", "Zeit pro Aufladung", "Time per charge");
    public static string TranslationChargesPerChargeUp => Translate("CHARGES_PER_CHARGE_UP", "Aufladungen pro Stufe", "Charges per charge-up");
    public static string TranslationChargeUpChance => Translate("CHARGE_UP_CHANCE", "Aufladungschance", "Charge-up chance");
    public static string TranslationIgnoreDifficultyBonus => Translate("IGNORE_DIFFICULTY_BONUS", "Schwierigkeitsbonus ignoriert", "Ignore difficulty bonus");
    public static string TranslationTrue => Translate("TRUE", "Ja", "True");
    public static string TranslationFalse => Translate("FALSE", "Nein", "False");

    public void UpdateStatistics(
        IReadOnlyCollection<StatisticEntry> currentEntries,
        IReadOnlyCollection<StatisticEntry> previousEntries,
        double rangeHours)
    {
        currentEntries ??= [];
        previousEntries ??= [];
        var catalogChanged = EnsureMobCatalog();
        var statisticsHash = CalculateStatisticsHash(currentEntries, previousEntries, rangeHours);
        if (!catalogChanged && _hasStatisticsHash && statisticsHash == _lastStatisticsHash)
        {
            return;
        }

        _lastStatisticsHash = statisticsHash;
        _hasStatisticsHash = true;
        _rangeHours = Math.Max(rangeHours, 0);
        _killStats.Clear();

        foreach (var entry in currentEntries.Where(IsValidMobKill))
        {
            var uniqueName = entry.MobUniqueName;
            if (!_killStats.TryGetValue(uniqueName, out var aggregate))
            {
                aggregate = new MobKillAggregate(0, entry.OccurredAtUtc, entry.OccurredAtUtc);
            }

            _killStats[uniqueName] = aggregate with
            {
                Kills = aggregate.Kills + 1,
                FirstAttackedUtc = entry.OccurredAtUtc < aggregate.FirstAttackedUtc ? entry.OccurredAtUtc : aggregate.FirstAttackedUtc,
                LastKilledUtc = entry.OccurredAtUtc > aggregate.LastKilledUtc ? entry.OccurredAtUtc : aggregate.LastKilledUtc
            };
        }

        var currentKillCount = currentEntries.Count(IsValidMobKill);
        var previousKillCount = previousEntries.Count(IsValidMobKill);
        TotalKillsSummary.Update(currentKillCount, currentKillCount, previousKillCount);
        TotalKillsSummary.UpdateValuePerHour(_rangeHours > 0 ? currentKillCount / _rangeHours : 0);
        UpdateSummaryMobs();
        ApplyListFilters();
    }

    public void ResetStatistics()
    {
        _hasStatisticsHash = false;
        UpdateStatistics([], [], _rangeHours);
    }

    public void RefreshLocalization()
    {
        _mobCatalog = [];
        _hasStatisticsHash = false;
        EnsureMobCatalog();
        ApplyListFilters();
        OnPropertyChanged(string.Empty);
    }


    private bool EnsureMobCatalog()
    {
        var mobs = MobsData.GetMobs();
        if (_mobCatalog.Count > 0 && _mobCatalog.Count == mobs.Count)
        {
            return false;
        }

        _mobCatalog = mobs
            .Where(x => !string.IsNullOrWhiteSpace(x.UniqueName))
            .GroupBy(x => x.UniqueName, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .Select(x => new MobCatalogEntry(x, ResolveMobName(x)))
            .OrderBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(x => x.Mob.UniqueName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        RefreshFilterOptions();
        return true;
    }

    private void RefreshFilterOptions()
    {
        MobTypeCategoryFilters = CreateFilterOptions(
            _mobCatalog.Select(x => x.Mob.MobTypeCategory),
            Translate("ALL_MOB_TYPES", "Alle Mob-Typen", "All mob types"));
        FactionFilters = CreateFilterOptions(
            _mobCatalog.Select(x => x.Mob.Faction),
            LocalizationController.Translation("ALL_FACTIONS"));

        if (!MobTypeCategoryFilters.Any(x => string.Equals(x.Value, SelectedMobTypeCategory, StringComparison.OrdinalIgnoreCase)))
        {
            _selectedMobTypeCategory = AllFilterValue;
            OnPropertyChanged(nameof(SelectedMobTypeCategory));
        }

        if (!FactionFilters.Any(x => string.Equals(x.Value, SelectedFaction, StringComparison.OrdinalIgnoreCase)))
        {
            _selectedFaction = AllFilterValue;
            OnPropertyChanged(nameof(SelectedFaction));
        }
    }

    private static IReadOnlyList<DashboardMobFilterOption> CreateFilterOptions(IEnumerable<string> values, string allName)
    {
        var options = new List<DashboardMobFilterOption>
        {
            new(AllFilterValue, allName)
        };
        options.AddRange(values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(x => new DashboardMobFilterOption(x, HumanizeFilterValue(x))));
        return options;
    }

    private void ApplyListFilters()
    {
        EnsureMobCatalog();

        _filteredMobs = _mobCatalog
            .Where(MatchesSearchText)
            .Where(x => string.IsNullOrWhiteSpace(SelectedMobTypeCategory)
                        || string.Equals(x.Mob.MobTypeCategory, SelectedMobTypeCategory, StringComparison.OrdinalIgnoreCase))
            .Where(x => string.IsNullOrWhiteSpace(SelectedFaction)
                        || string.Equals(x.Mob.Faction, SelectedFaction, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => _killStats.GetValueOrDefault(x.Mob.UniqueName).Kills)
            .ThenByDescending(x => _killStats.GetValueOrDefault(x.Mob.UniqueName).LastKilledUtc)
            .ThenBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        VisibleMobs = _filteredMobs.Select(CreateListItem).ToArray();
    }

    private bool MatchesSearchText(MobCatalogEntry entry)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        return entry.DisplayName.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase)
               || entry.Mob.UniqueName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
               || (entry.Mob.NameLocatag?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private void UpdateSummaryMobs()
    {
        if (_killStats.Count == 0)
        {
            MostKilledMob = CreateEmptyMob();
            LastKilledMob = CreateEmptyMob();
            return;
        }

        var mostKilled = _killStats
            .OrderByDescending(x => x.Value.Kills)
            .ThenByDescending(x => x.Value.LastKilledUtc)
            .First();
        var lastKilled = _killStats
            .OrderByDescending(x => x.Value.LastKilledUtc)
            .First();
        MostKilledMob = CreateListItem(mostKilled.Key, mostKilled.Value);
        LastKilledMob = CreateListItem(lastKilled.Key, lastKilled.Value);
    }

    private DashboardMobListItem CreateListItem(MobCatalogEntry entry)
    {
        var aggregate = _killStats.GetValueOrDefault(entry.Mob.UniqueName);
        return new DashboardMobListItem(
            entry.Mob,
            aggregate.Kills,
            aggregate.Kills > 0 ? aggregate.FirstAttackedUtc : null,
            aggregate.Kills > 0 ? aggregate.LastKilledUtc : null,
            _rangeHours);
    }

    private DashboardMobListItem CreateListItem(string uniqueName, MobKillAggregate aggregate)
    {
        var catalogEntry = _mobCatalog.FirstOrDefault(x => string.Equals(x.Mob.UniqueName, uniqueName, StringComparison.OrdinalIgnoreCase));
        var mob = catalogEntry?.Mob ?? new MobJsonObject { UniqueName = uniqueName };
        return new DashboardMobListItem(mob, aggregate.Kills, aggregate.FirstAttackedUtc, aggregate.LastKilledUtc, _rangeHours);
    }

    private static DashboardMobListItem CreateEmptyMob()
    {
        return new DashboardMobListItem(new MobJsonObject { UniqueName = "—" }, 0, null, null, 1);
    }

    private static bool IsValidMobKill(StatisticEntry entry)
    {
        return entry?.ValueType == StatisticsAnalysisTool.Enumerations.ValueType.MobKill
               && !string.IsNullOrWhiteSpace(entry.MobUniqueName);
    }

    private static int CalculateStatisticsHash(
        IEnumerable<StatisticEntry> currentEntries,
        IEnumerable<StatisticEntry> previousEntries,
        double rangeHours)
    {
        var hash = new HashCode();
        hash.Add(rangeHours);
        AddEntriesToHash(ref hash, currentEntries);
        AddEntriesToHash(ref hash, previousEntries);
        return hash.ToHashCode();
    }

    private static void AddEntriesToHash(ref HashCode hash, IEnumerable<StatisticEntry> entries)
    {
        foreach (var entry in entries)
        {
            hash.Add(entry.SessionId);
            hash.Add(entry.OccurredAtUtc);
            hash.Add(entry.MobUniqueName, StringComparer.OrdinalIgnoreCase);
        }
    }

    private static string ResolveMobName(MobJsonObject mob)
    {
        var localizedName = MobsData.GetLocalizedMobName(mob);
        return string.IsNullOrWhiteSpace(localizedName) ? mob.UniqueName ?? string.Empty : localizedName;
    }

    private static string HumanizeFilterValue(string value)
    {
        var normalizedValue = value
            .Replace("_", " ", StringComparison.Ordinal)
            .Replace("miniboss", "mini boss", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normalizedValue);
    }

    internal static string Translate(string key, string germanText, string englishText)
    {
        var translation = LocalizationController.Translation(key);
        if (!string.Equals(translation, key, StringComparison.Ordinal))
        {
            return translation;
        }

        var cultureName = SettingsController.CurrentSettings.CurrentCultureIetfLanguageTag ?? string.Empty;
        return cultureName.StartsWith("de", StringComparison.OrdinalIgnoreCase)
            ? germanText
            : englishText;
    }

    private sealed class MobCatalogEntry
    {
        public MobCatalogEntry(MobJsonObject mob, string displayName)
        {
            Mob = mob;
            DisplayName = displayName ?? string.Empty;
        }

        public MobJsonObject Mob { get; }
        public string DisplayName { get; }
    }

    private readonly record struct MobKillAggregate(int Kills, DateTime FirstAttackedUtc, DateTime LastKilledUtc);
}
