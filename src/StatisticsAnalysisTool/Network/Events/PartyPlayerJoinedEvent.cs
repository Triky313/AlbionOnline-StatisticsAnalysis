using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class PartyPlayerJoinedEvent
{
    public Guid UserGuid { get; private set; } = Guid.Empty;
    public string Username { get; private set; } = string.Empty;

    public PartyPlayerJoinedEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.TryGetValue(1, out object userGuid) && userGuid is Guid)
            {
                UserGuid = userGuid.ObjectToGuid() ?? Guid.Empty;
            }

            if (parameters.TryGetValue(2, out object username) && username is string usernameString && !string.IsNullOrEmpty(usernameString))
            {
                Username = usernameString;
            }

        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}