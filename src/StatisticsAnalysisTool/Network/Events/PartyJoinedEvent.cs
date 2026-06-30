using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public sealed class PartyJoinedEvent
{
    private const int GuidByteLength = 16;

    public Dictionary<Guid, string> PartyUsers { get; } = new();

    public PartyJoinedEvent(Dictionary<byte, object> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        try
        {
            if (!parameters.TryGetValue(8, out var partyUserGuidsParameter))
            {
                return;
            }

            if (!parameters.TryGetValue(9, out var partyUserNamesParameter))
            {
                return;
            }

            AddPartyUsers(
                partyUserGuidsParameter,
                partyUserNamesParameter);
        }
        catch (Exception exception)
        {
            DebugConsole.WriteError(
                MethodBase.GetCurrentMethod()?.DeclaringType,
                exception);
        }
    }

    private void AddPartyUsers(object partyUserGuidsParameter, object partyUserNamesParameter)
    {
        var partyUserGuids = GetPartyUserGuids(partyUserGuidsParameter);
        var partyUserNames = GetPartyUserNames(partyUserNamesParameter);
        var partyUserCount = Math.Min(partyUserGuids.Count, partyUserNames.Count);

        for (var index = 0; index < partyUserCount; index++)
        {
            var partyUserGuid = partyUserGuids[index];
            var partyUserName = partyUserNames[index];

            if (partyUserGuid == Guid.Empty)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(partyUserName))
            {
                continue;
            }

            PartyUsers[partyUserGuid] = partyUserName;
        }
    }

    private static List<Guid> GetPartyUserGuids(object parameter)
    {
        return parameter switch
        {
            byte[] guidBytes => GetPartyUserGuidsFromBytes(guidBytes),
            object[] guidObjects => GetPartyUserGuidsFromObjects(guidObjects),
            _ => []
        };
    }

    private static List<Guid> GetPartyUserGuidsFromBytes(byte[] guidBytes)
    {
        var partyUserCount = guidBytes.Length / GuidByteLength;
        var partyUserGuids = new List<Guid>(partyUserCount);

        for (var index = 0; index < partyUserCount; index++)
        {
            var guidByteOffset = index * GuidByteLength;
            var partyUserGuid = new Guid(
                guidBytes.AsSpan(guidByteOffset, GuidByteLength));

            partyUserGuids.Add(partyUserGuid);
        }

        return partyUserGuids;
    }

    private static List<Guid> GetPartyUserGuidsFromObjects(object[] guidObjects)
    {
        var partyUserGuids = new List<Guid>(guidObjects.Length);

        foreach (var guidObject in guidObjects)
        {
            var partyUserGuid = guidObject.ObjectToGuid();

            if (partyUserGuid.HasValue)
            {
                partyUserGuids.Add(partyUserGuid.Value);
            }
        }

        return partyUserGuids;
    }

    private static List<string> GetPartyUserNames(object parameter)
    {
        if (parameter is string[] partyUserNames)
        {
            return [.. partyUserNames];
        }

        if (parameter is not object[] partyUserNameObjects)
        {
            return [];
        }

        var partyUserNamesFromObjects =
            new List<string>(partyUserNameObjects.Length);

        foreach (var partyUserNameObject in partyUserNameObjects)
        {
            partyUserNamesFromObjects.Add(
                partyUserNameObject?.ToString() ?? string.Empty);
        }

        return partyUserNamesFromObjects;
    }
}