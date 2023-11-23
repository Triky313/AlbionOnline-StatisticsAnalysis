using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network;

public interface ILiveStatsTracker
{
    void Add(ValueType valueType, double value, CityFaction cityFaction = CityFaction.Unknown);
    void Start();
    void Stop();
    void Reset();
}