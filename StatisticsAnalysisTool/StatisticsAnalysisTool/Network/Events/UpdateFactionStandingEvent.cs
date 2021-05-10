using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFactionStandingEvent : BaseEvent
    {
        public CityFaction CityFaction;
        public FixPoint GainedFactionFlagPoints;
        public FixPoint BonusPremiumGainedFractionFlagPoints;
        public FixPoint TotalPlayerFactionFlagPoints;

        public UpdateFactionStandingEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Console.WriteLine($@"[{DateTime.UtcNow}] {GetType().Name}: {JsonConvert.SerializeObject(parameters)}");

            try
            {
                if (parameters.ContainsKey(0))
                {
                    CityFaction = FactionWarfareController.GetCityFactionFlagType(parameters[0].ObjectToByte());
                }

                if (parameters.ContainsKey(1))
                {
                    GainedFactionFlagPoints = FixPoint.FromInternalValue(parameters[1].ObjectToLong() ?? 0);
                }

                if (parameters.ContainsKey(2))
                {
                    BonusPremiumGainedFractionFlagPoints = FixPoint.FromInternalValue(parameters[2].ObjectToLong() ?? 0);
                }

                if (parameters.ContainsKey(3))
                {
                    TotalPlayerFactionFlagPoints = FixPoint.FromInternalValue(parameters[3].ObjectToLong() ?? 0);
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }
}