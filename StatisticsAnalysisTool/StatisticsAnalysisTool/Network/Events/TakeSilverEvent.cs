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
                    TimeStamp = parameters[1] as long? ?? default;
                }

                if (parameters.ContainsKey(2) && int.TryParse(parameters[2].ToString(), out int targetEntityId))
                {
                    TargetEntityId = targetEntityId;
                }

                if (parameters.ContainsKey(3))
                {
                    var yieldPreTax = parameters[3] as int? ?? 0;
                    YieldPreTax = FixPoint.FromInternalValue(yieldPreTax);
                }

                if (parameters.ContainsKey(5))
                {
                    var guildTax = parameters[5] as short? ?? 0;
                    GuildTax = FixPoint.FromInternalValue(guildTax);
                }

                if (parameters.ContainsKey(6))
                {
                    var clusterTax = parameters[6] as short? ?? 0;
                    ClusterTax = FixPoint.FromInternalValue(clusterTax);
                }

                if (parameters.ContainsKey(7))
                {
                    PremiumBonus = parameters[7] as bool? ?? false;
                }

                if (parameters.ContainsKey(8))
                {
                    var multiplier = parameters[8] as short? ?? 0;
                    Multiplier = FixPoint.FromInternalValue(multiplier);
                }

                YieldAfterTax = YieldPreTax - GuildTax;
            }
            catch(Exception e)
            {
                Debug.Print(e.Message);
            }
        }
        
        public long TimeStamp;
        public long TargetEntityId;
        public FixPoint YieldPreTax;
        public FixPoint GuildTax;
        public FixPoint ClusterTax;
        public bool PremiumBonus;
        public FixPoint Multiplier;
        public bool ClusterBonus; // 9?

        public FixPoint YieldAfterTax;
    }
}