using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class UpdateLootChestEvent
{
    public int ObjectId { get; }
    public List<Guid> PlayerGuid { get; } = new();
    public List<Guid> PlayerGuid2 { get; } = new();

    public UpdateLootChestEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(0) && int.TryParse(parameters[0].ToString(), out var objectId))
            {
                ObjectId = objectId;
            }

            if (parameters.ContainsKey(3))
            {
                foreach (var guid in (object[])parameters[3])
                {
                    var playerGuid = guid.ObjectToGuid() ?? Guid.Empty;
                    if (playerGuid == Guid.Empty)
                    {
                        continue;
                    }

                    PlayerGuid.Add(playerGuid);
                }
            }

            if (parameters.ContainsKey(4))
            {
                foreach (var guid in (object[])parameters[4])
                {
                    var playerGuid = guid.ObjectToGuid() ?? Guid.Empty;
                    if (playerGuid == Guid.Empty)
                    {
                        continue;
                    }

                    PlayerGuid2.Add(playerGuid);
                }
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}