using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class UpdateReSpecPointsEvent
    {
        public FixPoint? CurrentTotalReSpecPoints { get; }
        public FixPoint GainedReSpecPoints { get; }

        private readonly double? _lastReSpecValue;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public UpdateReSpecPointsEvent(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(0) && parameters[0] != null)
                {
                    var parameterType = parameters[0].GetType();

                    switch (parameterType.Name)
                    {
                        case "Int32[]":
                            {
                                var reSpecPointsArray = ((int[])parameters[0]).ToDictionary();

                                if (reSpecPointsArray?.Count > 0 && reSpecPointsArray.ContainsKey(1))
                                {
                                    CurrentTotalReSpecPoints = FixPoint.FromInternalValue(reSpecPointsArray[1].ObjectToLong() ?? 0);
                                    Utilities.AddValue(CurrentTotalReSpecPoints.Value.DoubleValue, _lastReSpecValue, out _lastReSpecValue);
                                    GainedReSpecPoints = _lastReSpecValue != null ? FixPoint.FromFloatingPointValue((double)_lastReSpecValue) : FixPoint.FromFloatingPointValue(0);
                                }

                                break;
                            }
                        case "Int64[]":
                            {
                                var reSpecPointsArray = ((long[])parameters[0]).ToDictionary();

                                if (reSpecPointsArray?.Count > 0 && reSpecPointsArray.ContainsKey(1))
                                {
                                    CurrentTotalReSpecPoints = FixPoint.FromInternalValue(reSpecPointsArray[1].ObjectToLong() ?? 0);
                                    Utilities.AddValue(CurrentTotalReSpecPoints.Value.DoubleValue, _lastReSpecValue, out _lastReSpecValue);
                                    GainedReSpecPoints = _lastReSpecValue != null ? FixPoint.FromFloatingPointValue((double)_lastReSpecValue) : FixPoint.FromFloatingPointValue(0);
                                }

                                break;
                            }
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
            catch (InvalidCastException e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}