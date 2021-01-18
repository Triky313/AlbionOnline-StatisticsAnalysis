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
            // Array[10] exist only by Crafting...
            try
            {
                //foreach (var parameter in parameters)
                //{
                //    Debug.Print($"{parameter}");
                //}

                if (parameters.ContainsKey(1) && long.TryParse(parameters[1].ToString(), out long totalFame))
                {
                    TotalPlayerFame = totalFame / 10000d;
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

                // TODO: Is not isMobFame. Maybe Aggressive or not Aggressive mobs
                if (parameters.ContainsKey(5))
                {
                    IsMobFame = bool.TryParse(parameters[5].ToString(), out var isMobFame) && isMobFame;
                }
                else
                {
                    IsMobFame = false;
                }

                SatchelFame = 0;
                if (parameters.ContainsKey(9) && long.TryParse(parameters[9].ToString(), out var satchelFameUnrounded))
                {
                    if (satchelFameUnrounded >= 1)
                    {
                        SatchelFame = satchelFameUnrounded / 10000d;
                    }
                }

                NormalFame = 0;
                if (FameWithZoneAndWithoutPremium > 0 && ZoneMultiplier >= 1)
                {
                    NormalFame = FameWithZoneAndWithoutPremium / ZoneMultiplier;
                }

                double fameWithZoneAndPremium = 0;
                if (FameWithZoneAndWithoutPremium > 0)
                {
                    if (IsMobFame)
                    {
                        fameWithZoneAndPremium = FameWithZoneAndWithoutPremium * 1.5f;
                    }
                    else
                    {
                        fameWithZoneAndPremium = FameWithZoneAndWithoutPremium;
                    }
                }

                PremiumFame = 0;
                if (fameWithZoneAndPremium > 0 && FameWithZoneAndWithoutPremium > 0)
                {
                    PremiumFame = fameWithZoneAndPremium - FameWithZoneAndWithoutPremium;
                }

                ZoneFame = 0;
                if (FameWithZoneAndWithoutPremium >= NormalFame)
                {
                    ZoneFame = FameWithZoneAndWithoutPremium - NormalFame;
                }

                TotalGainedFame = NormalFame + ZoneFame + PremiumFame + SatchelFame;
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public double TotalPlayerFame { get; }
        public double FameWithZoneAndWithoutPremium { get; }
        public double ZoneMultiplier { get; }
        public double NormalFame { get; }
        public double PremiumFame { get; }
        public double ZoneFame { get; }
        public double SatchelFame { get; }
        public double TotalGainedFame { get; }
        public bool IsMobFame { get; }
    }
}