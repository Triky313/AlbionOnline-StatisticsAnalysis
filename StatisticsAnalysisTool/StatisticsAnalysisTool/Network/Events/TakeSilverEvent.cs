using Albion.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TakeSilverEvent : BaseEvent
    {
        public TakeSilverEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                Debug.Print($"-----------------------------------------");
                foreach (var parameter in parameters)
                {
                    Debug.Print($"{parameter}");
                }

                if (parameters.ContainsKey(3) && long.TryParse(parameters[3].ToString(), out long totalCollectedSilver))
                {
                    TotalCollectedSilver = totalCollectedSilver / 10000d;
                }

                if (parameters.ContainsKey(5) && long.TryParse(parameters[5].ToString(), out long guildTax))
                {
                    GuildTax = guildTax / 10000d;
                }

                if (!double.IsNaN(TotalCollectedSilver) && !double.IsNaN(GuildTax) && TotalCollectedSilver != 0 && GuildTax != 0)
                {
                    EarnedSilver = TotalCollectedSilver - GuildTax;
                }
            }
            catch(Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public double TotalCollectedSilver { get; }
        public double GuildTax { get; }
        public double EarnedSilver { get; }
    }
}