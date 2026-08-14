using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class NewLootChestEvent
{
    private const byte RarityParameterIndex = 21;

    public int ObjectId { get; set; }
    public string UniqueName { get; set; }
    public string UniqueNameWithLocation { get; set; }
    public TreasureRarity Rarity { get; }

    public NewLootChestEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(0) && int.TryParse(parameters[0].ToString(), out var objectId))
            {
                ObjectId = objectId;
            }

            if (parameters.ContainsKey(3))
            {
                UniqueName = string.IsNullOrEmpty(parameters[3].ToString()) ? string.Empty : parameters[3].ToString();
            }

            if (parameters.ContainsKey(4))
            {
                UniqueNameWithLocation = string.IsNullOrEmpty(parameters[4].ToString()) ? string.Empty : parameters[4].ToString();
            }

            Rarity = GetRarity(parameters);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private static TreasureRarity GetRarity(IReadOnlyDictionary<byte, object> parameters)
    {
        if (!parameters.TryGetValue(RarityParameterIndex, out var rarityValue)
            || !int.TryParse(rarityValue.ToString(), out var rarity))
        {
            return TreasureRarity.Unknown;
        }

        return rarity switch
        {
            0 => TreasureRarity.Common,
            1 => TreasureRarity.Uncommon,
            2 => TreasureRarity.Rare,
            3 => TreasureRarity.Legendary,
            _ => TreasureRarity.Unknown
        };
    }
}
