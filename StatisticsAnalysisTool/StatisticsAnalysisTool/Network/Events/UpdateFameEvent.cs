using Albion.Network;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEvent : BaseEvent
    {
        public UpdateFameEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // Array[10] exist only by Crafting...
            try
            {
                //Debug.Print($"----- UpdateFame (Events) -----");
                //foreach (var parameter in parameters)
                //{
                //    Debug.Print($"{parameter}");
                //}

                if (parameters.ContainsKey(1))
                {
                    var totalPlayerFame = parameters[1] as long? ?? 0;
                    TotalPlayerFame = FixPoint.FromInternalValue(totalPlayerFame);
                }
                
                if (parameters.ContainsKey(2))
                {
                    var change = parameters[2] as int? ?? 0;
                    Change = FixPoint.FromInternalValue(change);
                }

                if (parameters.ContainsKey(3))
                {
                    GroupSize = parameters[3] as byte? ?? 0;
                }

                if (parameters.ContainsKey(4))
                {
                    var multiplier = parameters[4] as short? ?? 0;
                    Multiplier = FixPoint.FromInternalValue(multiplier);
                }

                if (parameters.ContainsKey(5))
                {
                    IsPremiumBonus = parameters[5] as bool? ?? false;
                }

                if (parameters.ContainsKey(6))
                {
                    BonusFactor = parameters[6] as float? ?? 0;
                }

                if (parameters.ContainsKey(9))
                {
                    var satchelFame = parameters[9] as long? ?? 0;
                    SatchelFame = FixPoint.FromInternalValue(satchelFame);
                }

                if (parameters.ContainsKey(252))
                {
                    UsedItemType = parameters[252] as short? ?? 0;
                }

                if (Change.DoubleValue > 0 && Multiplier.DoubleValue > 0)
                {
                    var newNormalFame = Change.DoubleValue / Multiplier.DoubleValue;
                    NormalFame = FixPoint.FromFloatingPointValue(newNormalFame);
                }

                double fameWithZoneAndPremium = 0;
                if (Change.DoubleValue > 0)
                {
                    if (IsPremiumBonus)
                    {
                        fameWithZoneAndPremium = Change.DoubleValue * 1.5f;
                    }
                    else
                    {
                        fameWithZoneAndPremium = Change.DoubleValue;
                    }
                }

                if (fameWithZoneAndPremium > 0 && Change.DoubleValue > 0)
                {
                    var newPremiumFame = fameWithZoneAndPremium - Change.DoubleValue;
                    PremiumFame = FixPoint.FromFloatingPointValue(newPremiumFame);
                }

                if (Change.InternalValue >= NormalFame.InternalValue)
                {
                    ZoneFame = FixPoint.FromInternalValue(Change.InternalValue - NormalFame.InternalValue);
                }

                TotalGainedFame = NormalFame + ZoneFame + PremiumFame + SatchelFame;
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public FixPoint TotalPlayerFame { get; }
        public FixPoint Change; // Fame with Zone Multiplier and without Premium
        public byte GroupSize;
        public FixPoint Multiplier { get; }
        public bool IsPremiumBonus;
        public float BonusFactor;
        public FixPoint SatchelFame;
        public int UsedItemType;

        public FixPoint NormalFame { get; }
        public FixPoint PremiumFame { get; }
        public FixPoint ZoneFame { get; }
        public FixPoint TotalGainedFame { get; }
    }
}