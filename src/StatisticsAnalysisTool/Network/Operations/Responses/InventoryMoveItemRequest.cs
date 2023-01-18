using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class InventoryMoveItemRequest
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public readonly int ContainerSlot;
    public readonly Guid? ContainerGuid;
    public readonly int InventorySlot;
    public readonly Guid? UserInteractGuid;

    public InventoryMoveItemRequest(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForMessage(GetType().Name, parameters, ConsoleColorType.EventMapChangeColor);

        try
        {
            if (parameters.ContainsKey(0))
            {
                ContainerSlot = parameters[0].ObjectToInt();
            }

            if (parameters.ContainsKey(1))
            {
                ContainerGuid = parameters[1].ObjectToGuid();
            }

            if (parameters.ContainsKey(3))
            {
                InventorySlot = parameters[3].ObjectToInt();
            }

            if (parameters.ContainsKey(4))
            {
                UserInteractGuid = parameters[4].ObjectToGuid();
            }
        }
        catch (Exception e)
        {
            Log.Debug(nameof(InventoryMoveItemRequest), e);
        }
    }
}