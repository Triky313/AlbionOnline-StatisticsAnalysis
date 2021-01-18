using Albion.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network
{
    public class UpdateMoneyEvent : BaseEvent
    {
        public UpdateMoneyEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Debug.Print($"-----------------------------------------");
            Debug.Print($"UpdateMoney");

            foreach (var parameter in parameters)
            {
                Debug.Print($"{parameter}");
            }

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
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public double CurrentPlayerSilver { get; }
        public double CurrentPlayerGold { get; }
    }
}