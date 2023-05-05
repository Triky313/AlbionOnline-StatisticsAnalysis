using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.Network.Events;

public class NewCharacterEvent
{
    public long? ObjectId { get; }
    public Guid? Guid { get; }
    public string Name { get; }
    public string GuildName { get; }
    public float[] Position { get; }
    public CharacterEquipment CharacterEquipment { get; } = new();

    public NewCharacterEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(0)) ObjectId = parameters[0].ObjectToLong();

            if (parameters.ContainsKey(1)) Name = parameters[1].ToString();

            if (parameters.ContainsKey(7)) Guid = parameters[7].ObjectToGuid();

            if (parameters.ContainsKey(8)) GuildName = parameters[8].ToString();

            if (parameters.ContainsKey(13)) Position = (float[]) parameters[13];
                
            if (parameters.ContainsKey(34))
            {
                var valueType = parameters[34].GetType();
                switch (valueType.IsArray)
                {
                    case true when typeof(byte[]).Name == valueType.Name:
                    {
                        var values = ((byte[])parameters[34]).ToDictionary();
                        CharacterEquipment = GetEquipment(values);
                        break;
                    }
                    case true when typeof(short[]).Name == valueType.Name:
                    {
                        var values = ((short[])parameters[34]).ToDictionary();
                        CharacterEquipment = GetEquipment(values);
                        break;
                    }
                    case true when typeof(int[]).Name == valueType.Name:
                    {
                        var values = ((int[])parameters[34]).ToDictionary();
                        CharacterEquipment = GetEquipment(values);
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private CharacterEquipment GetEquipment<T>(IReadOnlyDictionary<int, T> values)
    {
        return new CharacterEquipment
        {
            MainHand = values[0].ObjectToInt(),
            OffHand = values[1].ObjectToInt(),
            Head = values[2].ObjectToInt(),
            Chest = values[3].ObjectToInt(),
            Shoes = values[4].ObjectToInt(),
            Bag = values[5].ObjectToInt(),
            Cape = values[6].ObjectToInt(),
            Mount = values[7].ObjectToInt()
        };
    }
}