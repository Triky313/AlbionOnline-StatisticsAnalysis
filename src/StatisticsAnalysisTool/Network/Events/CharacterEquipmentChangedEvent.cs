using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EventValidations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class CharacterEquipmentChangedEvent
{
    public long? ObjectId { get; private set; }
    public CharacterEquipment CharacterEquipment { get; } = new();

    public CharacterEquipmentChangedEvent(Dictionary<byte, object> parameters)
    {
        EventValidator.IsEventValid(EventCodes.CharacterEquipmentChanged, parameters);
        
        try
        {
            ProcessObjectId(parameters);
            ProcessEquipment(parameters);
            ProcessEquipmentSpells(parameters);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private void ProcessObjectId(IReadOnlyDictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out object objectId))
        {
            ObjectId = objectId.ObjectToLong();
        }
    }

    private void ProcessEquipment(IReadOnlyDictionary<byte, object> parameters)
    {
        if (!TryGetValues(parameters, 2, out var equipment))
        {
            return;
        }

        CharacterEquipment.MainHand = GetEquipmentValue(equipment, 0);
        CharacterEquipment.OffHand = GetEquipmentValue(equipment, 1);
        CharacterEquipment.Head = GetEquipmentValue(equipment, 2);
        CharacterEquipment.Chest = GetEquipmentValue(equipment, 3);
        CharacterEquipment.Shoes = GetEquipmentValue(equipment, 4);
        CharacterEquipment.Bag = GetEquipmentValue(equipment, 5);
        CharacterEquipment.Cape = GetEquipmentValue(equipment, 6);
        CharacterEquipment.Mount = GetEquipmentValue(equipment, 7);
        CharacterEquipment.Potion = GetEquipmentValue(equipment, 8);
        CharacterEquipment.BuffFood = GetEquipmentValue(equipment, 9);
    }

    private void ProcessEquipmentSpells(IReadOnlyDictionary<byte, object> parameters)
    {
        if (!TryGetValues(parameters, 7, out var spells))
        {
            return;
        }

        AddSpell(SlotType.MainHand, GetSpellValue(spells, 0));
        AddSpell(SlotType.MainHand, GetSpellValue(spells, 1));
        AddSpell(SlotType.MainHand, GetSpellValue(spells, 2));
        AddSpell(SlotType.Armor, GetSpellValue(spells, 3));
        AddSpell(SlotType.Head, GetSpellValue(spells, 4));
        AddSpell(SlotType.Shoes, GetSpellValue(spells, 5));
        AddSpell(SlotType.Potion, GetSpellValue(spells, 12));
        AddSpell(SlotType.Food, GetSpellValue(spells, 13));
    }

    private static bool TryGetValues(IReadOnlyDictionary<byte, object> parameters, byte key, out int[] values)
    {
        values = [];

        if (!parameters.TryGetValue(key, out object value))
        {
            return false;
        }

        switch (value)
        {
            case int[] intValues:
                values = intValues;
                return true;
            case short[] shortValues:
                values = Array.ConvertAll(shortValues, item => (int) item);
                return true;
            default:
                return false;
        }
    }

    private static int GetEquipmentValue(IReadOnlyList<int> equipment, int index)
    {
        return equipment.Count > index ? equipment[index] : 0;
    }

    private static int GetSpellValue(IReadOnlyList<int> spells, int index)
    {
        return spells.Count > index ? spells[index] : -1;
    }

    private void AddSpell(SlotType slotType, int spellValue)
    {
        if (slotType != SlotType.Unknown && spellValue != -1)
        {
            CharacterEquipment.ActiveSpells.Add(new SlotSpell()
            {
                SlotType = slotType,
                Value = spellValue,
                ItemIndex = GetItemIndexBySlotType(slotType)
            });
        }
    }

    private int GetItemIndexBySlotType(SlotType slotType)
    {
        return slotType switch
        {
            SlotType.MainHand => CharacterEquipment.MainHand,
            SlotType.OffHand => CharacterEquipment.OffHand,
            SlotType.Cape => CharacterEquipment.Cape,
            SlotType.Bag => CharacterEquipment.Bag,
            SlotType.Armor => CharacterEquipment.Chest,
            SlotType.Head => CharacterEquipment.Head,
            SlotType.Shoes => CharacterEquipment.Shoes,
            SlotType.Mount => CharacterEquipment.Mount,
            SlotType.Potion => CharacterEquipment.Potion,
            SlotType.Food => CharacterEquipment.BuffFood,
            _ => 0
        };
    }
}