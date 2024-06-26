using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.EventValidations;

public static class EventValidationRules
{
    public static readonly Dictionary<EventCodes, Func<Dictionary<byte, object>, bool>> Rules = new()
        {
            { EventCodes.NewShrine, NewShrineEvent },
            { EventCodes.NewMob, NewMobEvent },
            { EventCodes.HealthUpdate, HealthUpdateEvent },
            { EventCodes.CharacterEquipmentChanged, CharacterEquipmentChangedEvent },
            { EventCodes.NewCharacter, NewCharacterEvent },
            { EventCodes.TakeSilver, TakeSilverEvent },
        };

    private static bool NewShrineEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            return parameters.ContainsKey(0)
                   && parameters.ContainsKey(3)
                   && parameters[3].ToString()!.Contains("SHRINE")
                   && int.TryParse(parameters[0].ToString(), out var id) && id > 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool NewMobEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            return parameters.ContainsKey(1)
                   && parameters.ContainsKey(7)
                   && parameters.ContainsKey(8)
                   && parameters.ContainsKey(13)
                   && parameters.ContainsKey(14)
                   && parameters.ContainsKey(17);
        }
        catch
        {
            return false;
        }
    }

    private static bool HealthUpdateEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            return parameters.ContainsKey(0)
                   && parameters.ContainsKey(1)
                   && parameters.ContainsKey(2)
                   && parameters.ContainsKey(3)
                   && parameters.ContainsKey(4)
                   && parameters.ContainsKey(5)
                   && parameters.ContainsKey(6)
                   && parameters.ContainsKey(7)
                   && parameters[4].ObjectToByte() < 10
                   && parameters[5].ObjectToByte() < 10;
        }
        catch
        {
            return false;
        }
    }

    private static bool CharacterEquipmentChangedEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            return parameters.ContainsKey(0)
                   && parameters.ContainsKey(2)
                   && parameters.ContainsKey(6)
                   && parameters.TryGetValue(2, out object equipmentObject)
                   && equipmentObject.GetType().IsArray
                   && typeof(short[]).Name == equipmentObject.GetType().Name
                   && (((short[]) parameters[2]).ToDictionary()).Count == 10;
        }
        catch
        {
            return false;
        }
    }

    private static bool NewCharacterEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            return parameters.ContainsKey(0)
                   && parameters.ContainsKey(1)
                   && parameters.ContainsKey(7)
                   && !string.IsNullOrEmpty(parameters[1].ToString())
                   && parameters[7].ObjectToGuid() != Guid.Empty;
        }
        catch
        {
            return false;
        }
    }

    private static bool TakeSilverEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            return parameters.ContainsKey(0)
                   && parameters.ContainsKey(1)
                   && parameters.ContainsKey(2)
                   && parameters.ContainsKey(3)
                   && parameters[1].ObjectToLong() > 0
                   && parameters[2].ObjectToLong() > 0;
        }
        catch
        {
            return false;
        }
    }
}