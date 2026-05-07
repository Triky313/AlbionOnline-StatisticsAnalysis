using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class MobDataResolver
{
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
        if (!string.IsNullOrWhiteSpace(mobDataByIndex.UniqueName))
        {
            return mobDataByIndex;
        }

        var mobDataByHealth = MobsData.GetMobByHitPointsMaxOrDefault(newMobEvent.HitPointsMax);
        if (!string.IsNullOrWhiteSpace(mobDataByHealth.UniqueName))
        {
            return mobDataByHealth;
        }

        return UnknownMobData;
    }
}