using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.EventValidations;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class NewCharacterEvent
{
    public long? ObjectId { get; }
    public Guid? Guid { get; }
    public string Name { get; }
    public string GuildName { get; }
    public CharacterEquipment CharacterEquipment { get; } = new();

    public NewCharacterEvent(Dictionary<byte, object> parameters)
    {
        EventValidator.IsEventValid(EventCodes.NewCharacter, parameters);
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                ObjectId = objectId.ObjectToLong();
            }

            if (parameters.TryGetValue(1, out object name))
            {
                Name = name.ToString();
            }

            if (parameters.TryGetValue(7, out object guid))
            {
                Guid = guid.ObjectToGuid();
            }

            if (parameters.TryGetValue(8, out object guildName))
            {
                GuildName = guildName.ToString();
            }

            if (parameters.ContainsKey(40))
            {
                var valueType = parameters[40].GetType();
                switch (valueType.IsArray)
                {
                    case true when typeof(byte[]).Name == valueType.Name:
                        {
                            var values = ((byte[]) parameters[40]).ToDictionary();
                            CharacterEquipment = GetEquipment(values);
                            break;
                        }
                    case true when typeof(short[]).Name == valueType.Name:
                        {
                            var values = ((short[]) parameters[40]).ToDictionary();
                            CharacterEquipment = GetEquipment(values);
                            break;
                        }
                    case true when typeof(int[]).Name == valueType.Name:
                        {
                            var values = ((int[]) parameters[40]).ToDictionary();
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
            Mount = values[7].ObjectToInt(),
            Potion = values[8].ObjectToInt(),
            BuffFood = values[9].ObjectToInt()
        };
    }
}