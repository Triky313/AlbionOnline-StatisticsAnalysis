using Albion.Network;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace StatisticsAnalysisTool.Network
{
    public class UpdateMoneyEvent : BaseEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public UpdateMoneyEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(1) && long.TryParse(parameters[1].ToString(), out long currentPlayerSilver))
                {
                    CurrentPlayerSilver = currentPlayerSilver / 10000d;
                }

                if (parameters.ContainsKey(2) && long.TryParse(parameters[2].ToString(), out long currentPlayerGold))
                {
                    CurrentPlayerGold = currentPlayerGold / 10000d;
                }
            }
            catch (ArgumentNullException e)
            {
                Log.Error(nameof(UpdateMoneyEvent), e);
                Debug.Print(e.Message);
            }
        }

        public double CurrentPlayerSilver { get; }
        public double CurrentPlayerGold { get; }
    }
}