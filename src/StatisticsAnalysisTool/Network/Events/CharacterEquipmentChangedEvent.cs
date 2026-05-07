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
            ProcessSpells(parameters);
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
        if (parameters.TryGetValue(2, out object equipmentObject))
        {
            var valueType = equipmentObject.GetType();
            if (valueType.IsArray && typeof(short[]).Name == valueType.Name)
            {
                var equipment = ((short[]) parameters[2]).ToDictionary();
                CharacterEquipment.MainHand = equipment[0].ObjectToInt();
                CharacterEquipment.OffHand = equipment[1].ObjectToInt();
                CharacterEquipment.Head = equipment[2].ObjectToInt();
                CharacterEquipment.Chest = equipment[3].ObjectToInt();
                CharacterEquipment.Shoes = equipment[4].ObjectToInt();
                CharacterEquipment.Bag = equipment[5].ObjectToInt();
                CharacterEquipment.Cape = equipment[6].ObjectToInt();
                CharacterEquipment.Mount = equipment[7].ObjectToInt();
                CharacterEquipment.Potion = equipment[8].ObjectToInt();
                CharacterEquipment.BuffFood = equipment[9].ObjectToInt();
            }
        }
    }

    private void ProcessSpells(IReadOnlyDictionary<byte, object> parameters)
    {
        ProcessEquipmentSpells(parameters);
        ProcessCapeSpell(parameters);
    }

    private void ProcessEquipmentSpells(IReadOnlyDictionary<byte, object> parameters)
    {
        if (!TryGetSpells(parameters, 7, out var spells))
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

    private void ProcessCapeSpell(IReadOnlyDictionary<byte, object> parameters)
    {
        if (!TryGetSpells(parameters, 5, out var spells))
        {
            return;
        }

        AddSpell(SlotType.Cape, GetSpellValue(spells, 13));
    }

    private static bool TryGetSpells(IReadOnlyDictionary<byte, object> parameters, byte key, out short[] spells)
    {
        spells = [];

        if (!parameters.TryGetValue(key, out object spellsObject))
        {
            return false;
        }

        var valueType = spellsObject.GetType();
        if (!valueType.IsArray || typeof(short[]).Name != valueType.Name)
        {
            return false;
        }

        spells = (short[]) spellsObject;
        return true;
    }

    private static int GetSpellValue(IReadOnlyList<short> spells, int index)
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