using Albion.Network;
using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network
{
    public class UpdateMoneyEvent : BaseEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public UpdateMoneyEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(1))
                {
                    CurrentPlayerSilver = FixPoint.FromInternalValue(parameters[1].ObjectToLong() ?? 0);
                }

                if (parameters.ContainsKey(2))
                {
                    CurrentPlayerGold = FixPoint.FromInternalValue(parameters[2].ObjectToLong() ?? 0);
                }
            }
            catch (ArgumentNullException e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        public FixPoint CurrentPlayerSilver { get; }
        public FixPoint CurrentPlayerGold { get; }
    }
}