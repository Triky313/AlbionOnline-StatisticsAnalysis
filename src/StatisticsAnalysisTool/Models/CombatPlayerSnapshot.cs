using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Models;

public sealed class CombatPlayerSnapshot
{
    public string Name { get; set; } = string.Empty;
    public List<int> EquipmentItemIndexes { get; set; } = [];
    public double EstimatedEquipmentValue { get; set; }

    public CombatPlayerSnapshot CreateSnapshot()
    {
        return new CombatPlayerSnapshot
        {
            Name = Name,
            EquipmentItemIndexes = EquipmentItemIndexes?.ToList() ?? [],
            EstimatedEquipmentValue = EstimatedEquipmentValue
        };
    }
}