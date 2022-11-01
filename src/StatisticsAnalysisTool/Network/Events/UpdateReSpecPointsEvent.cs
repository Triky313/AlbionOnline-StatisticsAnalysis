using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Network.Events
{
    public class UpdateReSpecPointsEvent
    {
        public FixPoint? CurrentTotalReSpecPoints { get; }
        public FixPoint GainedReSpecPoints { get; }
        public FixPoint PaidSilver { get; }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public UpdateReSpecPointsEvent(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

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
                                var reSpecPointsArray = ((int[])parameters[0]).ToDictionary();

                                if (reSpecPointsArray?.Count > 0 && reSpecPointsArray.ContainsKey(1))
                                {
                                    CurrentTotalReSpecPoints = FixPoint.FromInternalValue(reSpecPointsArray[1].ObjectToLong() ?? 0);
                                }

                                break;
                            }
                        case "Int64[]":
                            {
                                var reSpecPointsArray = ((long[])parameters[0]).ToDictionary();

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