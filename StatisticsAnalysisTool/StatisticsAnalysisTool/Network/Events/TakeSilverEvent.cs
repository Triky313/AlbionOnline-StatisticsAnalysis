using Albion.Network;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TakeSilverEvent : BaseEvent
    {
        public long? ObjectId;
        public FixPoint ClusterTax;
        public FixPoint GuildTax;
        public FixPoint Multiplier;
        public bool IsPremiumBonus;
        public long? TargetEntityId;
        public long TimeStamp;

        public FixPoint YieldAfterTax;
        public FixPoint YieldPreTax;
        public FixPoint ClusterYieldPreTax;
        public FixPoint PremiumAfterTax;
        public FixPoint ClusterYieldAfterTax;

        public TakeSilverEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);
            
            try
            {
                if (parameters.ContainsKey(0))
                {
                    ObjectId = parameters[0].ObjectToLong();
                }

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
                    IsPremiumBonus = parameters[7] as bool? ?? false;
                }

                if (parameters.ContainsKey(8))
                {
                    var multiplier = parameters[8].ObjectToLong();
                    Multiplier = FixPoint.FromInternalValue(multiplier ?? 0);
                }

                YieldAfterTax = YieldPreTax - GuildTax;
                ClusterYieldPreTax = FixPoint.FromFloatingPointValue(YieldPreTax.DoubleValue - (YieldPreTax.DoubleValue / Multiplier.DoubleValue));
                PremiumAfterTax = ClusterYieldPreTax - ClusterTax;
                ClusterYieldAfterTax = FixPoint.FromFloatingPointValue((ClusterYieldPreTax.DoubleValue / Multiplier.DoubleValue) - ClusterTax.DoubleValue);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
            }
        }
    }
}