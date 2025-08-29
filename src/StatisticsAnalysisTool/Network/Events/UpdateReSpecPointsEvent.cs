using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class UpdateReSpecPointsEvent
{
    public FixPoint? CurrentTotalReSpecPoints { get; }
    public FixPoint GainedReSpecPoints { get; }
    public FixPoint PaidSilver { get; }

    public UpdateReSpecPointsEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(2))
            {
                GainedReSpecPoints = FixPoint.FromInternalValue(parameters[2].ObjectToLong() ?? 0);
            }

            if (parameters.ContainsKey(3))
            {
                PaidSilver = FixPoint.FromInternalValue(parameters[3].ObjectToLong() ?? 0);
            }

            if (parameters.ContainsKey(0) && parameters[0] != null)
            {
                var parameterType = parameters[0].GetType();

                switch (parameterType.Name)
                {
                    case "Int32[]":
                        {
                            var reSpecPointsArray = ((int[]) parameters[0]).ToDictionary();

                            if (reSpecPointsArray?.Count > 0 && reSpecPointsArray.ContainsKey(1))
                            {
                                CurrentTotalReSpecPoints = FixPoint.FromInternalValue(reSpecPointsArray[1].ObjectToLong() ?? 0);
                            }

                            break;
                        }
                    case "Int64[]":
                        {
                            var reSpecPointsArray = ((long[]) parameters[0]).ToDictionary();

                            if (reSpecPointsArray?.Count > 0 && reSpecPointsArray.ContainsKey(1))
                            {
                                CurrentTotalReSpecPoints = FixPoint.FromInternalValue(reSpecPointsArray[1].ObjectToLong() ?? 0);
                            }

                            break;
                        }
                }
            }
        }
        catch (ArgumentNullException e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        catch (InvalidCastException e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}