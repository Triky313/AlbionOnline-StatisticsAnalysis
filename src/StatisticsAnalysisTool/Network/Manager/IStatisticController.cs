using System;
using System.Threading.Tasks;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Manager;

public interface IStatisticController
{
    event Action OnAddValue;
    void AddValue(ValueType valueType, double gainedValue);

    void SetKillsDeathsValues();
    void UpdateRepairCostsUi();

    Task LoadFromFileAsync();
    Task SaveInFileAsync();
}