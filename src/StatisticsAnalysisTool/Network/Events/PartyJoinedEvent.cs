using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class PartyJoinedEvent
{
    public Guid UserGuid { get; private set; } = Guid.Empty;
    public string Username { get; private set; } = string.Empty;
    public string GuildName { get; private set; } = string.Empty;

    public PartyJoinedEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.TryGetValue(1, out object userGuid) && userGuid is Guid)
            {
                UserGuid = userGuid.ObjectToGuid() ?? Guid.Empty;
            }

            if (parameters.TryGetValue(1, out object username) && username is string usernameString && !string.IsNullOrEmpty(usernameString))
            {
                Username = usernameString;
            }

            if (parameters.TryGetValue(5, out object guildName) && guildName is string guildNameString && !string.IsNullOrEmpty(guildNameString))
            {
                GuildName = guildNameString;
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}