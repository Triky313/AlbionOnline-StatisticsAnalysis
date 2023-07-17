using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class GetCharacterEquipmentResponse
{
    public Guid Guid { get; }
    public double ItemPower { get; }
    public InternalCharacterEquipment CharacterEquipment { get; } = new();

    public GetCharacterEquipmentResponse(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.TryGetValue(0, out object guid))
            {
                Guid = guid.ObjectToGuid() ?? Guid.Empty;
            }
            
            if (parameters.ContainsKey(1))
            {
                var valueType = parameters[1].GetType();
                switch (valueType.IsArray)
                {
                    case true when typeof(byte[]).Name == valueType.Name:
                    {
                        var values = ((byte[])parameters[1]).ToDictionary();
                        CharacterEquipment = GetEquipment(values);
                        break;
                    }
                    case true when typeof(short[]).Name == valueType.Name:
                    {
                        var values = ((short[])parameters[1]).ToDictionary();
                        CharacterEquipment = GetEquipment(values);
                        break;
                    }
                    case true when typeof(int[]).Name == valueType.Name:
                    {
                        var values = ((int[])parameters[1]).ToDictionary();
                        CharacterEquipment = GetEquipment(values);
                        break;
                    }
                }

                if (parameters.TryGetValue(3, out object itemPower))
                {
                    ItemPower = itemPower.ObjectToDouble();
                }
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private InternalCharacterEquipment GetEquipment<T>(IReadOnlyDictionary<int, T> values)
    {
        return new InternalCharacterEquipment
        {
            MainHand = values[0].ObjectToInt(),
            OffHand = values[1].ObjectToInt(),
            Head = values[2].ObjectToInt(),
            Chest = values[3].ObjectToInt(),
            Shoes = values[4].ObjectToInt(),
            Bag = values[5].ObjectToInt(),
            Cape = values[6].ObjectToInt(),
            Mount = values[7].ObjectToInt(),
            Potion = values[8].ObjectToInt(),
            BuffFood = values[9].ObjectToInt()
        };
    }
}