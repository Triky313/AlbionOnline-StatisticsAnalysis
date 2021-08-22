using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateCurrencyEvent : BaseEvent
    {
        public CityFaction CityFaction;
        public FixPoint GainedFactionCoins;
        public FixPoint BonusPremiumGainedFractionFlagPoints;
        public FixPoint FactionRankPoints;
        public FixPoint FactionRankPointsPopulationBonus;
        public FixPoint FactionRankPointsPremiumBonus;
        public FixPoint TotalPlayerFactionPoints;

        public UpdateCurrencyEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);
            
            try
            {
                if (parameters.ContainsKey(2))
                {
                    CityFaction = FactionWarfareController.GetCityFactionType(parameters[2].ObjectToByte());
                }

                if (parameters.ContainsKey(3))
                {
                    GainedFactionCoins = FixPoint.FromInternalValue(parameters[3].ObjectToLong() ?? 0);
                }

                if (parameters.ContainsKey(4))
                {
                    FactionRankPoints = FixPoint.FromInternalValue(parameters[4].ObjectToLong() ?? 0);
                }

                if (parameters.ContainsKey(6))
                {
                    FactionRankPointsPopulationBonus = FixPoint.FromInternalValue(parameters[6].ObjectToLong() ?? 0);
                }

                if (parameters.ContainsKey(8))
                {
                    FactionRankPointsPremiumBonus = FixPoint.FromInternalValue(parameters[8].ObjectToLong() ?? 0);
                }

                if (parameters.ContainsKey(9))
                {
                    TotalPlayerFactionPoints = FixPoint.FromInternalValue(parameters[9].ObjectToLong() ?? 0);
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}