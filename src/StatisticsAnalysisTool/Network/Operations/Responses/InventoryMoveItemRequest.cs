using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class InventoryMoveItemRequest
{
    public readonly int ContainerSlot;
    public readonly Guid? ContainerGuid;
    public readonly int InventorySlot;
    public readonly Guid? UserInteractGuid;

    public InventoryMoveItemRequest(Dictionary<byte, object> parameters)
    {
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
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}