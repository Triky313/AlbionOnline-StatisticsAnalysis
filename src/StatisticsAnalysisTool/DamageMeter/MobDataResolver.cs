using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class MobDataResolver
{
    private const double HitPointsTolerance = 0.01;
    private static readonly MobJsonObject UnknownMobData = new()
    {
        UniqueName = "UNKNOWN_MOB"
    };

    public MobJsonObject Resolve(NewMobEvent newMobEvent)
    {
        if (newMobEvent == null)
        {
            return UnknownMobData;
        }

        var mobDataByIndex = MobsData.GetMobByIndexOrDefault(newMobEvent.MobIndex);
        var mobDataByUnshiftedIndex = MobsData.GetMobByUnshiftedIndexOrDefault(newMobEvent.MobIndex);
        if (HasUniqueName(mobDataByIndex) && HasMatchingHitPoints(mobDataByIndex, newMobEvent.HitPointsMax))
        {
            return mobDataByIndex;
        }

        if (HasUniqueName(mobDataByUnshiftedIndex) && HasMatchingHitPoints(mobDataByUnshiftedIndex, newMobEvent.HitPointsMax))
        {
            return mobDataByUnshiftedIndex;
        }

        var mobDataByHealth = MobsData.GetMobByHitPointsMaxOrDefault(newMobEvent.HitPointsMax);
        if (HasUniqueName(mobDataByHealth))
        {
            return mobDataByHealth;
        }

        if (HasUniqueName(mobDataByIndex))
        {
            return mobDataByIndex;
        }

        return HasUniqueName(mobDataByUnshiftedIndex) ? mobDataByUnshiftedIndex : UnknownMobData;
    }

    private static bool HasUniqueName(MobJsonObject mobData)
    {
        return !string.IsNullOrWhiteSpace(mobData?.UniqueName);
    }

    private static bool HasMatchingHitPoints(MobJsonObject mobData, double hitPointsMax)
    {
        if (mobData?.HitPointsMax <= 0 || hitPointsMax <= 0)
        {
            return true;
        }

        return System.Math.Abs(mobData.HitPointsMax - hitPointsMax) < HitPointsTolerance;
    }
}