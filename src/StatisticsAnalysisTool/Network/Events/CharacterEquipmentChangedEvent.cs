using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class CharacterEquipmentChangedEvent
{
    public CharacterEquipmentChangedEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(0))
            {
                ObjectId = parameters[0].ObjectToLong();
            }

            if (parameters.ContainsKey(2))
            {
                var valueType = parameters[2].GetType();
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
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public long? ObjectId { get; }
    public CharacterEquipment CharacterEquipment { get; } = new CharacterEquipment();
}