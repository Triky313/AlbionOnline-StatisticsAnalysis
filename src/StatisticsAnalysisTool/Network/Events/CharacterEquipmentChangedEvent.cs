using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class CharacterEquipmentChangedEvent
{
    public long? ObjectId { get; private set; }
    public CharacterEquipment CharacterEquipment { get; } = new();

    public CharacterEquipmentChangedEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            ProcessObjectId(parameters);
            ProcessEquipment(parameters);
            ProcessSpells(parameters);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
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
        if (parameters.TryGetValue(6, out object spellsObject))
        {
            var valueType = spellsObject.GetType();
            if (valueType.IsArray && typeof(short[]).Name == valueType.Name)
            {
                var spell = ((short[]) parameters[6]).ToDictionary();

                AddSpell(SlotType.MainHand, spell[0]);
                AddSpell(SlotType.MainHand, spell[1]);
                AddSpell(SlotType.MainHand, spell[2]);
                AddSpell(SlotType.Armor, spell[3]);
                AddSpell(SlotType.Head, spell[4]);
                AddSpell(SlotType.Shoes, spell[5]);
                AddSpell(SlotType.Potion, spell[12]);
                AddSpell(SlotType.Food, spell[13]);
            }
        }
    }

    private void AddSpell(SlotType slotType, object spellObject)
    {
        if (slotType != SlotType.Unknown && spellObject.ObjectToInt() != -1)
        {
            CharacterEquipment.ActiveSpells.Add(new SlotSpell()
            {
                SlotType = slotType,
                Value = spellObject.ObjectToInt()
            });
        }
    }
}