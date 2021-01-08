using Albion.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEvent : BaseEvent
    {
        public UpdateFameEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                //foreach (var parameter in parameters)
                //{
                //    Debug.Print($"{parameter}");
                //}
                
                if (parameters.ContainsKey(1) && long.TryParse(parameters[1].ToString(), out long totalFame))
                {
                    TotalFame = totalFame / 10000d;
                }

                if (parameters.ContainsKey(2) && long.TryParse(parameters[2].ToString(), out var fameWithZoneAndWithoutPremiumUnrounded))
                {
                    FameWithZoneAndWithoutPremium = 0;
                    if (fameWithZoneAndWithoutPremiumUnrounded > 0)
                    {
                        FameWithZoneAndWithoutPremium = fameWithZoneAndWithoutPremiumUnrounded / 10000d;
                    }
                }

                if (parameters.ContainsKey(4) && long.TryParse(parameters[4].ToString(), out var zoneMultiplierUnrounded))
                {
                    ZoneMultiplier = 1;
                    if (zoneMultiplierUnrounded >= 1)
                    {
                        ZoneMultiplier = zoneMultiplierUnrounded / 10000d;
                    }
                }

                if (FameWithZoneAndWithoutPremium > 0 && ZoneMultiplier >= 1)
                {
                    NormalFame = FameWithZoneAndWithoutPremium / ZoneMultiplier;
                }

                if (FameWithZoneAndWithoutPremium > 0)
                {
                    FameWithZoneAndPremium = FameWithZoneAndWithoutPremium * 1.5f;
                }

                if (FameWithZoneAndPremium > 0 && FameWithZoneAndWithoutPremium > 0)
                {
                    PremiumFame = FameWithZoneAndPremium - FameWithZoneAndWithoutPremium;
                }

                if (FameWithZoneAndWithoutPremium >= NormalFame)
                {
                    ZoneFame = FameWithZoneAndWithoutPremium - NormalFame;
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public double TotalFame { get; }
        public double FameWithZoneAndWithoutPremium { get; }
        public double ZoneMultiplier { get; }
        public double NormalFame { get; }
        public double FameWithZoneAndPremium { get; }
        public double PremiumFame { get; }
        public double ZoneFame { get; }
    }
}