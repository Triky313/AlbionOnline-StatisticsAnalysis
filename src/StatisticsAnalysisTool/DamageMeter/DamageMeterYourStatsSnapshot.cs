using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterYourStatsSnapshot
{
    private const string ZeroValue = "0";
    private const string ZeroPercent = "0.00%";
    private const string ZeroDuration = "0:00";

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
    } = ZeroValue;

    public string TotalDps
    {
        get; init;
    } = ZeroValue;

    public string PeakDpsThreeSeconds
    {
        get; init;
    } = ZeroValue;

    public string PeakDpsFiveSeconds
    {
        get; init;
    } = ZeroValue;

    public string PeakDpsTenSeconds
    {
        get; init;
    } = ZeroValue;

    public string BiggestHit
    {
        get; init;
    } = ZeroValue;

    public string PveDamage
    {
        get; init;
    } = ZeroValue;

    public string PvpDamage
    {
        get; init;
    } = ZeroValue;

    public string TotalHealing
    {
        get; init;
    } = ZeroValue;

    public string EffectiveHealing
    {
        get; init;
    } = ZeroValue;

    public string OverhealPercent
    {
        get; init;
    } = ZeroPercent;

    public IReadOnlyList<DamageMeterYourStatsTopEntry> TopHealingTargets
    {
        get; init;
    } = [];

    public string DamageTaken
    {
        get; init;
    } = ZeroValue;

    public string Dtps
    {
        get; init;
    } = ZeroValue;

    public IReadOnlyList<DamageMeterYourStatsTopEntry> TopDamageTakenBySpell
    {
        get; init;
    } = [];

    public string CombatTime
    {
        get; init;
    } = ZeroDuration;

    public string FightCount
    {
        get; init;
    } = ZeroValue;

    public string AverageFightDuration
    {
        get; init;
    } = ZeroDuration;
}