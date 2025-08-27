using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class NewEquipmentItemEvent
{
    public readonly DiscoveredItem Item;

    private readonly long? _objectId;
    private readonly int _itemId;
    private readonly int _quantity;
    private readonly long _estimatedMarketValue;
    private readonly short _qualityLevel = 1;
    private readonly FixPoint _durability;
    private Dictionary<int, int> SpellDictionary { get; } = new();

    public NewEquipmentItemEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                _objectId = objectId.ObjectToLong();
            }

            if (parameters.TryGetValue(1, out object itemId))
            {
                _itemId = itemId.ObjectToInt();
            }

            if (parameters.TryGetValue(2, out object quantity))
            {
                _quantity = quantity.ObjectToInt();
            }

            if (parameters.TryGetValue(4, out object estimatedMarketValue))
            {
                _estimatedMarketValue = estimatedMarketValue.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(6, out object qualityLevel))
            {
                _qualityLevel = qualityLevel.ObjectToShort();
            }

            if (parameters.TryGetValue(7, out object durabilityValue))
            {
                var durability = durabilityValue.ObjectToLong();
                _durability = FixPoint.FromInternalValue(durability ?? 0);
            }

            if (parameters.ContainsKey(8))
            {
                var valueType = parameters[8].GetType();
                if (valueType.IsArray && typeof(byte[]).Name == valueType.Name)
                {
                    var spells = ((byte[]) parameters[8]).ToDictionary();
                    foreach (var spell in spells)
                        SpellDictionary.Add(spell.Key, spell.Value.ObjectToInt());
                }
                else if (valueType.IsArray && typeof(short[]).Name == valueType.Name)
                {
                    var spells = ((short[]) parameters[8]).ToDictionary();
                    foreach (var spell in spells)
                        SpellDictionary.Add(spell.Key, spell.Value.ObjectToInt());
                }
                else if (valueType.IsArray && typeof(int[]).Name == valueType.Name)
                {
                    var spells = ((int[]) parameters[8]).ToDictionary();
                    foreach (var spell in spells)
                        SpellDictionary.Add(spell.Key, spell.Value.ObjectToInt());
                }
            }

            if (_objectId != null)
            {
                Item = new DiscoveredItem()
                {
                    ObjectId = (long) _objectId,
                    ItemIndex = _itemId,
                    Quantity = _quantity,
                    SpellDictionary = SpellDictionary,
                    CurrentDurability = _durability,
                    Quality = ItemQualityMapper(_qualityLevel),
                    EstimatedMarketValueInternal = _estimatedMarketValue
                };
            }
            else
            {
                Item = null;
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private static ItemQuality ItemQualityMapper(short quality)
    {
        return quality switch
        {
            0 => ItemQuality.Unknown,
            1 => ItemQuality.Normal,
            2 => ItemQuality.Good,
            3 => ItemQuality.Outstanding,
            4 => ItemQuality.Excellent,
            5 => ItemQuality.Masterpiece,
            _ => ItemQuality.Unknown
        };
    }
}