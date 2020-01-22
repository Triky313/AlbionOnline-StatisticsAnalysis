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

        public static List<Location> GetLocationListByArea(List<LocationArea> areaList)
        {
            var locations = new List<Location>();

            foreach (var area in areaList)
            {
                if (area == LocationArea.BlackZone)
                {
                    locations.Add(Location.ArthursRest);
                    locations.Add(Location.MerlynsRest);
                    locations.Add(Location.MorganasRest);
                }
                if (area == LocationArea.Villages)
                {
                    locations.Add(Location.SwampCross);
                    locations.Add(Location.ForestCross);
                    locations.Add(Location.SteppeCross);
                    locations.Add(Location.HighlandCross);
                    locations.Add(Location.MountainCross);
                }
                if (area == LocationArea.Cities)
                {
                    locations.Add(Location.Thetford);
                    locations.Add(Location.Lymhurst);
                    locations.Add(Location.Bridgewatch);
                    locations.Add(Location.BlackMarket);
                    locations.Add(Location.Martlock);
                    locations.Add(Location.Caerleon);
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