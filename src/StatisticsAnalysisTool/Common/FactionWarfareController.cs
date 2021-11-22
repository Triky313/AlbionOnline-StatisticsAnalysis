using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Common
{
    public static class FactionWarfareController
    {
        public static CityFaction GetCityFactionType(byte id)
        {
            return id switch
            {
                6 => CityFaction.Caerleon,
                5 => CityFaction.Thetford,
                4 => CityFaction.Bridgewatch,
                3 => CityFaction.FortSterling,
                2 => CityFaction.Martlock,
                1 => CityFaction.Lymhurst,
                _ => CityFaction.Unknown
            };
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