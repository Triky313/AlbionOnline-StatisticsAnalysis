using Albion.Network;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFactionStandingEvent : BaseEvent
    {
        public UpdateFactionStandingEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            
            try
            {
                Debug.Print($"----- UpdateFactionStanding (Events) -----");
                foreach (var parameter in parameters)
                {
                    Debug.Print($"{parameter}");
                }
                //[0, 1]  
                //[1, 1] jaka frakcja?
                //[2, 70000] - pkt frakcyjne
                //[3, 20000] - pkt za premium
                //[4, 1549890000]  -aktualne pkt
                //[252, 76]


                if (parameters.ContainsKey(1))
                {
                    //1 - Lym
                    //2 - Martlock
                    //3 - FS
                    //4 - bridg (not tested)
                    //5 - thet

                    CurrentFactionID = parameters[1] as byte? ?? 0;
                }

                if (parameters.ContainsKey(2))
                {
                    GainedFactionPoints = FixPoint.FromInternalValue(parameters[2].ObjectToLong() ?? 0);
                }

                if (parameters.ContainsKey(3))
                {
                    BonusPremiumGainedFractionPoints = FixPoint.FromInternalValue(parameters[3].ObjectToLong() ?? 0);
                }

                if (parameters.ContainsKey(4))
                {
                    TotalPlayerFactionPoints = FixPoint.FromInternalValue(parameters[4].ObjectToLong() ?? 0);
                }

                //if (parameters.ContainsKey(5))
                //{
                //    IsPremiumBonus = parameters[5] as bool? ?? false;
                //}

                //if (parameters.ContainsKey(6))
                //{
                //    BonusFactor = parameters[6] as float? ?? 0;
                //}

                //if (parameters.ContainsKey(9))
                //{
                //    SatchelFame = FixPoint.FromInternalValue(parameters[9].ObjectToLong() ?? 0);
                //}

                //if (parameters.ContainsKey(252))
                //{
                //    UsedItemType = parameters[252].ObjectToInt();
                //}

                //if (Change.DoubleValue > 0 && Multiplier.DoubleValue > 0)
                //{
                //    var newNormalFame = Change.DoubleValue / Multiplier.DoubleValue;
                //    NormalFame = FixPoint.FromFloatingPointValue(newNormalFame);
                //}

                //double fameWithZoneAndPremium = 0;
                //if (Change.DoubleValue > 0)
                //{
                //    if (IsPremiumBonus)
                //    {
                //        fameWithZoneAndPremium = Change.DoubleValue * 1.5f;
                //    }
                //    else
                //    {
                //        fameWithZoneAndPremium = Change.DoubleValue;
                //    }
                //}

                //if (fameWithZoneAndPremium > 0 && Change.DoubleValue > 0)
                //{
                //    var newPremiumFame = fameWithZoneAndPremium - Change.DoubleValue;
                //    PremiumFame = FixPoint.FromFloatingPointValue(newPremiumFame);
                //}

                //if (Change.InternalValue >= NormalFame.InternalValue)
                //{
                //    ZoneFame = FixPoint.FromInternalValue(Change.InternalValue - NormalFame.InternalValue);
                //}

                //TotalGainedFame = NormalFame + ZoneFame + PremiumFame + SatchelFame;
                GainedFactionPoints = GainedFactionPoints;
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }


        public FixPoint TotalPlayerFactionPoints { get; }
        public FixPoint GainedFactionPoints { get; }
        public FixPoint BonusPremiumGainedFractionPoints { get; }
        //public FixPoint NormalGainedFractionPoints { get; }



        public byte CurrentFactionID { get; }

        //public FixPoint Change; // Fame with Zone Multiplier and without Premium
        //public byte GroupSize;
        //public FixPoint Multiplier { get; }
        //public bool IsPremiumBonus;
        //public float BonusFactor;
        //public FixPoint SatchelFame;
        //public int UsedItemType;

        
        //public FixPoint PremiumFame { get; }
        //public FixPoint ZoneFame { get; }

    }
}