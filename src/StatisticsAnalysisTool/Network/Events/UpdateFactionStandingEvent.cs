using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFactionStandingEvent
    {
        public CityFaction CityFaction;
        public FixPoint GainedFactionFlagPoints;
        public FixPoint BonusPremiumGainedFractionFlagPoints;
        public FixPoint TotalPlayerFactionFlagPoints;

        public UpdateFactionStandingEvent(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(0))
                {
                    CityFaction = GetCityFactionFlagType(parameters[0].ObjectToByte());
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
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        public static CityFaction GetCityFactionFlagType(byte id)
        {
            return id switch
            {
                6 => CityFaction.Caerleon,
                5 => CityFaction.Thetford,
                4 => CityFaction.FortSterling,
                3 => CityFaction.Bridgewatch,
                2 => CityFaction.Lymhurst,
                1 => CityFaction.Martlock,
                _ => CityFaction.Unknown
            };
        }
    }
}