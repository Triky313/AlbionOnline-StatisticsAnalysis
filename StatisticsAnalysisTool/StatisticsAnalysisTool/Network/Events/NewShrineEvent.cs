using Albion.Network;
using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewShrineEvent : BaseEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool InActiveCombat;
        public bool InPassiveCombat;

        public NewShrineEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                Debug.Print("----- NewShrineEvent (Event) -----");

                foreach (var parameter in parameters)
                {
                    Debug.Print($"{parameter}");
                }

                if (parameters.ContainsKey(1))
                {
                    InActiveCombat = parameters[1] as bool? ?? false;
                }

                if (parameters.ContainsKey(2))
                {
                    InPassiveCombat = parameters[2] as bool? ?? false;
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(UpdateMoneyEvent), e);
            }
        }
    }
}