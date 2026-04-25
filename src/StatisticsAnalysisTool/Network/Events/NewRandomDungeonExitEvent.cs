using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.EventValidations;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class NewRandomDungeonExitEvent
{
    public int ObjectId { get; }
    public WorldPosition? SourceExitPosition { get; }
    public bool IsAlreadyEntered { get; }
    public int Level { get; } = -1;

    public NewRandomDungeonExitEvent(Dictionary<byte, object> parameters)
    {
        EventValidator.IsEventValid(EventCodes.NewRandomDungeonExit, parameters);

        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                ObjectId = objectId.ObjectToInt();
            }

            if (parameters.TryGetValue(1, out object positionObj))
            {
                SourceExitPosition = ParseWorldPosition(positionObj);
            }

            if (parameters.TryGetValue(10, out object alreadyEntered))
            {
                IsAlreadyEntered = alreadyEntered.ObjectToBool();
            }

            if (parameters.TryGetValue(8, out object level))
            {
                Level = level.ObjectToInt();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private static WorldPosition? ParseWorldPosition(object value)
    {
        if (value is float[] { Length: >= 2 } floatArray)
        {
            return new WorldPosition(floatArray[0], floatArray[1]);
        }

        if (value is double[] { Length: >= 2 } doubleArray)
        {
            return new WorldPosition((float) doubleArray[0], (float) doubleArray[1]);
        }

        if (value is object[] { Length: >= 2 } objArray)
        {
            return new WorldPosition(Convert.ToSingle(objArray[0]), Convert.ToSingle(objArray[1]));
        }

        return null;
    }

    public RandomDungeonExitInfo ToRandomDungeonExitInfo(string sourceClusterIndex)
    {
        return new RandomDungeonExitInfo
        {
            ObjectId = ObjectId,
            SourceExitPosition = SourceExitPosition,
            SourceClusterIndex = sourceClusterIndex,
            Level = Level,
            IsAlreadyEntered = IsAlreadyEntered,
            LastSeenUtc = DateTime.UtcNow
        };
    }
}