using System.Collections.Generic;
using System.Windows;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterYourStatsSnapshot
{
    public static DamageMeterYourStatsSnapshot Empty
    {
        get;
    } = new();

    public bool HasData
    {
        get; init;
    }

    public string TotalDamage
    {
        get; init;
    } = string.Empty;

    public string TotalDps
    {
        get; init;
    } = string.Empty;

    public string PeakDpsThreeSeconds
    {
        get; init;
    } = string.Empty;

    public string PeakDpsFiveSeconds
    {
        get; init;
    } = string.Empty;

    public string PeakDpsTenSeconds
    {
        get; init;
    } = string.Empty;

    public string BiggestHit
    {
        get; init;
    } = string.Empty;

    public string PveDamage
    {
        get; init;
    } = string.Empty;

    public string PvpDamage
    {
        get; init;
    } = string.Empty;

    public string TotalHealing
    {
        get; init;
    } = string.Empty;

    public string EffectiveHealing
    {
        get; init;
    } = string.Empty;

    public string OverhealPercent
    {
        get; init;
    } = string.Empty;

    public IReadOnlyList<DamageMeterYourStatsTopEntry> TopHealingTargets
    {
        get; init;
    } = [];

    public string DamageTaken
    {
        get; init;
    } = string.Empty;

    public string Dtps
    {
        get; init;
    } = string.Empty;

    public IReadOnlyList<DamageMeterYourStatsTopEntry> TopDamageTakenBySpell
    {
        get; init;
    } = [];

    public string CombatTime
    {
        get; init;
    } = string.Empty;

    public string FightCount
    {
        get; init;
    } = string.Empty;

    public string AverageFightDuration
    {
        get; init;
    } = string.Empty;

    public Visibility ContentVisibility => HasData ? Visibility.Visible : Visibility.Collapsed;
    public Visibility EmptyVisibility => HasData ? Visibility.Collapsed : Visibility.Visible;
}