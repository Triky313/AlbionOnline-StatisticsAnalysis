using Albion.Network;
using StatisticsAnalysisTool.Common;
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
                if (parameters.ContainsKey(1))
                {
                    TimeStamp = parameters[1].ObjectToLong() ?? 0;
                }

                if (parameters.ContainsKey(2))
                {
                    TargetEntityId = parameters[2].ObjectToLong();
                }

                if (parameters.ContainsKey(3))
                {
                    var yieldPreTax = parameters[3].ObjectToLong();
                    YieldPreTax = FixPoint.FromInternalValue(yieldPreTax ?? 0);
                }

                if (parameters.ContainsKey(5))
                {
                    var guildTax = parameters[5].ObjectToLong();
                    GuildTax = FixPoint.FromInternalValue(guildTax ?? 0);
                }

                if (parameters.ContainsKey(6))
                {
                    var clusterTax = parameters[6].ObjectToLong();
                    ClusterTax = FixPoint.FromInternalValue(clusterTax ?? 0);
                }

                if (parameters.ContainsKey(7))
                {
                    PremiumBonus = parameters[7] as bool? ?? false;
                }

                if (parameters.ContainsKey(8))
                {
                    var multiplier = parameters[8].ObjectToLong();
                    Multiplier = FixPoint.FromInternalValue(multiplier ?? 0);
                }

                YieldAfterTax = YieldPreTax - GuildTax;
            }
            catch(Exception e)
            {
                Debug.Print(e.Message);
            }
        }
        
        public long TimeStamp;
        public long? TargetEntityId;
        public FixPoint YieldPreTax;
        public FixPoint GuildTax;
        public FixPoint ClusterTax;
        public bool PremiumBonus;
        public FixPoint Multiplier;
        public bool ClusterBonus; // 9?

        public FixPoint YieldAfterTax;
    }
}