using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class UpdateLootChestEvent
{
    private const int GuidByteLength = 16;

    public int ObjectId { get; }
    public List<Guid> PlayerGuid { get; } = [];
    public List<Guid> PlayerGuid2 { get; } = [];

    public UpdateLootChestEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object objectId) && int.TryParse(objectId.ToString(), out var parsedObjectId))
            {
                ObjectId = parsedObjectId;
            }

            if (parameters.TryGetValue(3, out object playerGuid))
            {
                PlayerGuid.AddRange(GetPlayerGuids(playerGuid));
            }

            if (parameters.TryGetValue(5, out object playerGuid2))
            {
                PlayerGuid2.AddRange(GetPlayerGuids(playerGuid2));
            }
            else if (parameters.TryGetValue(4, out object legacyPlayerGuid2))
            {
                PlayerGuid2.AddRange(GetPlayerGuids(legacyPlayerGuid2));
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private static List<Guid> GetPlayerGuids(object parameter)
    {
        if (parameter is byte[] playerGuidBytes)
        {
            return GetPlayerGuidsFromBytes(playerGuidBytes);
        }

        if (parameter is object[] playerGuidArray)
        {
            return GetPlayerGuidsFromObjects(playerGuidArray);
        }

        return [];
    }

    private static List<Guid> GetPlayerGuidsFromBytes(byte[] playerGuidBytes)
    {
        var playerGuids = new List<Guid>();
        var playerGuidCount = playerGuidBytes.Length / GuidByteLength;

        for (var i = 0; i < playerGuidCount; i++)
        {
            var guidBytes = new byte[GuidByteLength];
            Array.Copy(playerGuidBytes, i * GuidByteLength, guidBytes, 0, GuidByteLength);
            playerGuids.Add(new Guid(guidBytes));
        }

        return playerGuids;
    }

    private static List<Guid> GetPlayerGuidsFromObjects(object[] playerGuidArray)
    {
        var playerGuids = new List<Guid>();

        foreach (var guid in playerGuidArray)
        {
            var playerGuid = guid.ObjectToGuid() ?? Guid.Empty;
            if (playerGuid == Guid.Empty)
            {
                continue;
            }

            playerGuids.Add(playerGuid);
        }

        return playerGuids;
    }
}