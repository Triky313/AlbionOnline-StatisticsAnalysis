using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Models.BindingModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardMobListItem : BaseViewModel
{
    private static readonly Regex WordBoundaryRegex = new("(?<=[a-z])(?=[A-Z])", RegexOptions.Compiled);
    private readonly MobJsonObject _mob;
    private IReadOnlyList<DetailValue> _overviewStats;
    private IReadOnlyList<DetailValue> _combatStats;
    private IReadOnlyList<DetailValue> _aggroStats;
    private IReadOnlyList<DetailValue> _spawnStats;

    public DashboardMobListItem(
        MobJsonObject mob,
        int kills,
        DateTime? firstAttackedUtc,
        DateTime? lastKilledUtc,
        double rangeHours)
    {
        mob ??= new MobJsonObject();
        _mob = mob;

        UniqueName = mob.UniqueName ?? string.Empty;
        MobName = ResolveMobName(mob);
        NameLocatag = string.IsNullOrWhiteSpace(mob.NameLocatag) ? UniqueName : mob.NameLocatag;
        Avatar = MobsData.GetAvatarFileName(mob);
        Tier = mob.Tier;
        TierDisplay = Tier > 0 ? $"T{Tier}" : string.Empty;
        MobTypeCategory = mob.MobTypeCategory ?? string.Empty;
        MobTypeCategoryDisplay = Humanize(MobTypeCategory);
        CategoryDisplay = Humanize(mob.Category);
        FactionDisplay = Humanize(mob.Faction);
        AttackTypeDisplay = Humanize(mob.AttackType);
        Kills = kills;
        KillsPerHour = rangeHours > 0 ? kills / rangeHours : 0;
        FirstAttackedDisplay = FormatDate(firstAttackedUtc);
        LastKilledDisplay = FormatDate(lastKilledUtc);
    }

    public string UniqueName { get; }
    public string MobName { get; }
    public string NameLocatag { get; }
    public string Avatar { get; }
    public short Tier { get; }
    public string TierDisplay { get; }
    public string MobTypeCategory { get; }
    public string MobTypeCategoryDisplay { get; }
    public string CategoryDisplay { get; }
    public string FactionDisplay { get; }
    public string AttackTypeDisplay { get; }
    public int Kills { get; }
    public double KillsPerHour { get; }
    public string FirstAttackedDisplay { get; }
    public string LastKilledDisplay { get; }
    public bool HasTier => !string.IsNullOrWhiteSpace(TierDisplay);
    public bool HasMobTypeCategory => !string.IsNullOrWhiteSpace(MobTypeCategoryDisplay);
    public bool HasCategory => !string.IsNullOrWhiteSpace(CategoryDisplay);
    public bool HasFaction => !string.IsNullOrWhiteSpace(FactionDisplay);
    public bool HasAttackType => !string.IsNullOrWhiteSpace(AttackTypeDisplay);
    public BitmapImage AvatarSource => MobAvatarImageProvider.GetAvatarSource(Avatar);
    public IReadOnlyList<DetailValue> OverviewStats => _overviewStats ??= CreateOverviewStats();
    public IReadOnlyList<DetailValue> CombatStats => _combatStats ??= CreateCombatStats();
    public IReadOnlyList<DetailValue> AggroStats => _aggroStats ??= CreateAggroStats();
    public IReadOnlyList<DetailValue> SpawnStats => _spawnStats ??= CreateSpawnStats();

    public bool IsExpanded
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    private IReadOnlyList<DetailValue> CreateOverviewStats()
    {
        return
        [
            new DetailValue(DashboardMobsBindings.TranslationFame, FormatInteger(_mob.Fame)),
            new DetailValue(DashboardMobsBindings.TranslationAbilityPower, FormatNumber(_mob.AbilityPower)),
            new DetailValue(DashboardMobsBindings.TranslationHealth, FormatNumber(_mob.HitPointsMax, "N0")),
            new DetailValue(DashboardMobsBindings.TranslationEnergy, FormatNumber(_mob.EnergyMax, "N0")),
            new DetailValue(DashboardMobsBindings.TranslationMoveSpeed, FormatNumber(_mob.MoveSpeed)),
            new DetailValue(DashboardMobsBindings.TranslationDangerState, Humanize(_mob.DangerState, true)),
            new DetailValue(DashboardMobsBindings.TranslationEnergyReward, FormatInteger(_mob.EnergyReward)),
            new DetailValue(DashboardMobsBindings.TranslationMobValue, FormatInteger(_mob.MobValue))
        ];
    }

    private IReadOnlyList<DetailValue> CreateCombatStats()
    {
        return
        [
            new DetailValue(DashboardMobsBindings.TranslationAttackDamage, FormatNumber(_mob.AttackDamage, "N0")),
            new DetailValue(DashboardMobsBindings.TranslationAttackRange, FormatWithUnit(_mob.AttackRange, "m")),
            new DetailValue(DashboardMobsBindings.TranslationAttackSpeed, FormatWithUnit(_mob.AttackSpeed, "s")),
            new DetailValue(DashboardMobsBindings.TranslationMeleeDamageTime, FormatWithUnit(_mob.MeleeAttackDamageTime, "s")),
            new DetailValue(DashboardMobsBindings.TranslationAttackMoveSpeed, FormatNumber(_mob.AttackMoveSpeed)),
            new DetailValue(DashboardMobsBindings.TranslationPhysicalArmor, FormatNumber(_mob.PhysicalArmor, "N0")),
            new DetailValue(DashboardMobsBindings.TranslationMagicResistance, FormatNumber(_mob.MagicResistance, "N0")),
            new DetailValue(DashboardMobsBindings.TranslationCrowdControlResistance, FormatNumber(_mob.CrowdControlResistance, "N0")),
            new DetailValue(DashboardMobsBindings.TranslationAttackCollisionRadius, FormatNumber(_mob.AttackCollisionRadius))
        ];
    }

    private IReadOnlyList<DetailValue> CreateAggroStats()
    {
        return
        [
            new DetailValue(DashboardMobsBindings.TranslationAggroRadius, FormatNumber(_mob.AggroRadius)),
            new DetailValue(DashboardMobsBindings.TranslationAlertRadius, FormatNumber(_mob.AlertRadius)),
            new DetailValue(DashboardMobsBindings.TranslationPursuitRadius, FormatNumber(_mob.PursuitRadius)),
            new DetailValue(DashboardMobsBindings.TranslationRoamingRadius, FormatNumber(_mob.RoamingRadius)),
            new DetailValue(DashboardMobsBindings.TranslationRoamingIdleTime, FormatRange(_mob.RoamingIdleTimeMin, _mob.RoamingIdleTimeMax, "s")),
            new DetailValue(DashboardMobsBindings.TranslationDamageAggroFactor, FormatNumber(_mob.DamageAggroFactor)),
            new DetailValue(DashboardMobsBindings.TranslationHealingAggroFactor, FormatNumber(_mob.HealingAggroFactor)),
            new DetailValue(DashboardMobsBindings.TranslationShieldAggroFactor, FormatNumber(_mob.ShieldAggroFactor)),
            new DetailValue(DashboardMobsBindings.TranslationAggroDelayAfterSpawn, FormatWithUnit(_mob.AggroDelayAfterSpawn, "s"))
        ];
    }

    private IReadOnlyList<DetailValue> CreateSpawnStats()
    {
        return
        [
            new DetailValue(DashboardMobsBindings.TranslationRespawnTime, FormatRange(_mob.RespawnTimeSecondsMin, _mob.RespawnTimeSecondsMax, "s")),
            new DetailValue(DashboardMobsBindings.TranslationMaxCharges, FormatInteger(_mob.MaxCharges)),
            new DetailValue(DashboardMobsBindings.TranslationTimePerCharge, FormatWithUnit(_mob.TimePerChargeSeconds, "s")),
            new DetailValue(DashboardMobsBindings.TranslationChargesPerChargeUp, FormatInteger(_mob.ChargesPerChargeUp)),
            new DetailValue(DashboardMobsBindings.TranslationChargeUpChance, FormatPercentage(_mob.ChargeUpChance)),
            new DetailValue(DashboardMobsBindings.TranslationIgnoreDifficultyBonus, FormatBoolean(_mob.IgnoreDifficultyBonus))
        ];
    }

    private static string ResolveMobName(MobJsonObject mob)
    {
        var localizedName = MobsData.GetLocalizedMobName(mob);
        return string.IsNullOrWhiteSpace(localizedName) ? mob.UniqueName ?? string.Empty : localizedName;
    }

    private static string FormatDate(DateTime? value)
    {
        return value.HasValue ? value.Value.CurrentDateTimeFormat() : "—";
    }

    private static string FormatInteger(long? value)
    {
        return value.HasValue ? value.Value.ToString("N0", CultureInfo.CurrentCulture) : "—";
    }

    private static string FormatNumber(double? value, string format = "0.##")
    {
        return value.HasValue ? value.Value.ToString(format, CultureInfo.CurrentCulture) : "—";
    }

    private static string FormatWithUnit(double? value, string unit)
    {
        return value.HasValue ? $"{FormatNumber(value)} {unit}" : "—";
    }

    private static string FormatRange(double? minimum, double? maximum, string unit)
    {
        if (!minimum.HasValue && !maximum.HasValue)
        {
            return "—";
        }

        if (minimum.HasValue && maximum.HasValue && Math.Abs(minimum.Value - maximum.Value) > double.Epsilon)
        {
            return $"{FormatNumber(minimum)}–{FormatNumber(maximum)} {unit}";
        }

        return $"{FormatNumber(minimum ?? maximum)} {unit}";
    }

    private static string FormatPercentage(double? value)
    {
        return value.HasValue ? $"{value.Value * 100:0.##}%" : "—";
    }

    private static string FormatBoolean(string value)
    {
        return bool.TryParse(value, out var parsedValue)
            ? parsedValue ? DashboardMobsBindings.TranslationTrue : DashboardMobsBindings.TranslationFalse
            : "—";
    }

    private static string Humanize(string value, bool preserveEmpty = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return preserveEmpty ? "—" : string.Empty;
        }

        var normalized = value
            .Replace("_", " ", StringComparison.Ordinal)
            .Replace("miniboss", "mini boss", StringComparison.OrdinalIgnoreCase)
            .Replace("hidemob", "hide mob", StringComparison.OrdinalIgnoreCase);
        normalized = WordBoundaryRegex.Replace(normalized, " ");
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normalized.ToLower(CultureInfo.CurrentCulture));
    }

    public sealed class DetailValue
    {
        public DetailValue(string name, string value)
        {
            Name = name ?? string.Empty;
            Value = value ?? string.Empty;
        }

        public string Name { get; }
        public string Value { get; }
    }
}
