﻿using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class UpdateFameEvent
{
    public double BonusFactor = 1;
    public double BonusFactorInPercent;
    public FixPoint FameWithZoneMultiplier; // Fame with Zone Multiplier and without Premium
    public byte GroupSize;
    public bool IsPremiumBonus;
    public FixPoint SatchelFame;
    public bool IsBonusFactorActive;
    public long UsedBagInsightItemIndex;

    public FixPoint TotalPlayerFame { get; }
    public FixPoint Multiplier { get; } = FixPoint.FromFloatingPointValue(1);
    public double PremiumFame { get; }
    public FixPoint ZoneFame { get; }
    public double TotalGainedFame { get; }

    public UpdateFameEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        // Array[10] exist only by Crafting...
        try
        {
            if (parameters.ContainsKey(1))
            {
                var totalPlayerFame = parameters[1].ObjectToLong() ?? 0;
                TotalPlayerFame = FixPoint.FromInternalValue(totalPlayerFame);
            }

            if (parameters.ContainsKey(2))
            {
                var fameWithZoneMultiplier = FixPoint.FromInternalValue(parameters[2].ObjectToLong() ?? 0);
                FameWithZoneMultiplier = FixPoint.FromFloatingPointValue(fameWithZoneMultiplier.DoubleValue);
            }

            if (parameters.ContainsKey(3))
            {
                GroupSize = parameters[3].ObjectToByte();
            }

            if (parameters.ContainsKey(4))
            {
                Multiplier = FixPoint.FromInternalValue(parameters[4].ObjectToLong() ?? 0);
            }

            if (parameters.ContainsKey(5))
            {
                IsPremiumBonus = parameters[5].ObjectToBool();
            }

            if (parameters.ContainsKey(6))
            {
                BonusFactor = 1 + (parameters[6] as float? ?? 0);

                BonusFactorInPercent = (BonusFactor - 1) * 100;
                IsBonusFactorActive = (BonusFactorInPercent > 0);
            }

            if (parameters.ContainsKey(8))
            {
                UsedBagInsightItemIndex = parameters[8].ObjectToLong() ?? -1;
            }

            if (parameters.ContainsKey(10))
            {
                SatchelFame = FixPoint.FromInternalValue(parameters[10].ObjectToLong() ?? 0);
            }

            double fameWithZoneAndPremium = 0;
            if (FameWithZoneMultiplier.DoubleValue > 0)
            {
                if (IsPremiumBonus)
                {
                    fameWithZoneAndPremium = FameWithZoneMultiplier.DoubleValue * 1.5f;
                }
                else
                {
                    fameWithZoneAndPremium = FameWithZoneMultiplier.DoubleValue;
                }
            }

            if (fameWithZoneAndPremium > 0 && FameWithZoneMultiplier.DoubleValue > 0)
            {
                PremiumFame = fameWithZoneAndPremium - FameWithZoneMultiplier.DoubleValue;
            }

            TotalGainedFame = FameWithZoneMultiplier.DoubleValue + PremiumFame + SatchelFame.DoubleValue;

            ZoneFame = FixPoint.FromFloatingPointValue((TotalGainedFame / BonusFactor) - (PremiumFame + SatchelFame.DoubleValue));
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}
