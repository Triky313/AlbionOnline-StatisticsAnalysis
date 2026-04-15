using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class PartyJoinedEvent
{
    private const int GuidByteLength = 16;

    public readonly Dictionary<Guid, string> PartyUsers = new();

    public PartyJoinedEvent(Dictionary<byte, object> parameters)
    {
        // Info: parameters[4] is the party lead, parameters[5] contains party user GUIDs and parameters[6] contains names.
        try
        {
            if (parameters.TryGetValue(5, out object partyUserGuids) && parameters.TryGetValue(6, out object partyUserNames))
            {
                var partyUsers = GetPartyUserGuids(partyUserGuids);
                var partyUserNameArray = GetPartyUserNames(partyUserNames);
                var maxPartyUsers = Math.Min(partyUsers.Count, partyUserNameArray.Count);

                for (var i = 0; i < maxPartyUsers; i++)
                {
                    var name = partyUserNameArray[i];
                    if (partyUsers[i] != Guid.Empty && !string.IsNullOrEmpty(name))
                    {
                        PartyUsers[partyUsers[i]] = name;
                    }
                }
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private static List<Guid> GetPartyUserGuids(object parameter)
    {
        if (parameter is byte[] partyUserGuidBytes)
        {
            return GetPartyUserGuidsFromBytes(partyUserGuidBytes);
        }

        if (parameter is object[] partyUserGuidArray)
        {
            return GetPartyUserGuidsFromObjects(partyUserGuidArray);
        }

        return new List<Guid>();
    }

    private static List<Guid> GetPartyUserGuidsFromBytes(byte[] partyUserGuidBytes)
    {
        var partyUserGuids = new List<Guid>();
        var partyUserCount = partyUserGuidBytes.Length / GuidByteLength;

        for (var i = 0; i < partyUserCount; i++)
        {
            var guidBytes = new byte[GuidByteLength];
            Array.Copy(partyUserGuidBytes, i * GuidByteLength, guidBytes, 0, GuidByteLength);
            partyUserGuids.Add(new Guid(guidBytes));
        }

        return partyUserGuids;
    }

    private static List<Guid> GetPartyUserGuidsFromObjects(object[] partyUserGuidArray)
    {
        var partyUserGuids = new List<Guid>();

        foreach (var partyUserGuid in partyUserGuidArray)
        {
            var guid = partyUserGuid.ObjectToGuid();

            if (guid != null)
            {
                partyUserGuids.Add((Guid) guid);
            }
        }

        return partyUserGuids;
    }

    private static List<string> GetPartyUserNames(object parameter)
    {
        if (parameter is string[] partyUserNameArray)
        {
            return new List<string>(partyUserNameArray);
        }

        if (parameter is object[] partyUserNameObjectArray)
        {
            var partyUserNames = new List<string>();

            foreach (var partyUserName in partyUserNameObjectArray)
            {
                partyUserNames.Add(partyUserName?.ToString() ?? string.Empty);
            }

            return partyUserNames;
        }

        return new List<string>();
    }
}