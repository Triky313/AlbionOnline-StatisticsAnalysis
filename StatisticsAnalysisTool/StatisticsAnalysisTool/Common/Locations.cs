using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Common
{
    public static class Locations
    {
        public static readonly Dictionary<Location, string> Names = new Dictionary<Location, string>
        {
          {Location.Thetford, "Thetford" },
          {Location.SwampCross, "Swamp Cross" },
          {Location.Lymhurst, "Lymhurst" },
          {Location.ForestCross, "Forest Cross" },
          {Location.Bridgewatch, "Bridgewatch" },
          {Location.SteppeCross, "Steppe Cross" },
          {Location.HighlandCross, "Highland Cross" },
          {Location.BlackMarket, "Black Market" },
          {Location.Martlock, "Martlock" },
          {Location.Caerleon, "Caerleon" },
          {Location.FortSterling, "Fort Sterling" },
          {Location.MountainCross, "Mountain Cross" },
          {Location.ArthursRest, "Arthur's Rest" },
          {Location.MerlynsRest, "Merlyn's Rest" },
          {Location.MorganasRest, "Morgana's Rest" }
        };

        public static string GetName(Location location)
        {
            if (Names.TryGetValue(location, out var name))
            {
                return name;
            }
            return null;
        }

        public static string GetName(int locationId) => GetName((Location)locationId) ?? locationId.ToString();
        
        public static Location GetName(string location) => Names.FirstOrDefault(x => x.Value == location).Key;

        public static List<string> GetLocationListByArea(List<LocationArea> locationAreas)
        {
            var locations = new List<string>();

            foreach (var area in locationAreas)
            {
                if (area == LocationArea.BlackZone)
                {
                    locations.Add(GetName(Location.ArthursRest));
                    locations.Add(GetName(Location.MerlynsRest));
                    locations.Add(GetName(Location.MorganasRest));
                }
                if (area == LocationArea.Villages)
                {
                    locations.Add(GetName(Location.SwampCross));
                    locations.Add(GetName(Location.ForestCross));
                    locations.Add(GetName(Location.SteppeCross));
                    locations.Add(GetName(Location.HighlandCross));
                    locations.Add(GetName(Location.MountainCross));
                }
                if (area == LocationArea.Cities)
                {
                    locations.Add(GetName(Location.Thetford));
                    locations.Add(GetName(Location.Lymhurst));
                    locations.Add(GetName(Location.Bridgewatch));
                    locations.Add(GetName(Location.BlackMarket));
                    locations.Add(GetName(Location.Martlock));
                    locations.Add(GetName(Location.Caerleon));
                }
            }
            return locations;
        }

    }

    public enum Location
    {
        SwampCross = 4,
        Thetford = 7,
        Lymhurst = 1002,
        ForestCross = 1006,
        SteppeCross = 2002,
        Bridgewatch = 2004,
        HighlandCross = 3002,
        BlackMarket = 3003,
        Caerleon = 3005,
        Martlock = 3008,
        FortSterling = 4002,
        MountainCross = 4006,
        ArthursRest = -1,
        MerlynsRest = -2,
        MorganasRest = -3,
    }

    public enum LocationArea
    {
        BlackZone,
        Villages,
        Cities
    }
}