using Albion.Network;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class InCombatStateUpdateEvent : BaseEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public InCombatStateUpdateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(1))
                {
                    InActiveCombat = parameters[1] as bool? ?? false;
                }

                if (parameters.ContainsKey(2))
                {
                    InPassiveCombat = parameters[2] as bool? ?? false;
                }
            }
            catch(Exception e)
            {
                Log.Error(nameof(UpdateMoneyEvent), e);
            }
        }

        public bool InActiveCombat;
        public bool InPassiveCombat;
    }
}