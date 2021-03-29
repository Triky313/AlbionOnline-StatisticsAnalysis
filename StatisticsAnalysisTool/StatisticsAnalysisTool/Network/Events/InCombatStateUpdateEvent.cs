using Albion.Network;
using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class InCombatStateUpdateEvent : BaseEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool InActiveCombat;
        public bool InPassiveCombat;

        public long? ObjectId;

        public InCombatStateUpdateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(0))
                {
                    ObjectId = parameters[0].ObjectToLong();
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