using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Common
{
    using System.Windows;
    using System.Windows.Media;

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

        public static readonly Dictionary<Location, string> ParameterNames = new Dictionary<Location, string>
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
            {Location.ArthursRest, "Arthurs Rest" },
            {Location.MerlynsRest, "Merlyns Rest" },
            {Location.MorganasRest, "Morganas Rest" }
        };

        public static string GetName(Location location) => Names.TryGetValue(location, out var name) ? name : null;

        public static string GetParameterName(Location location) => ParameterNames.TryGetValue(location, out var name) ? name : null;

        public static Location GetName(string location) => ParameterNames.FirstOrDefault(x => x.Value == location).Key;
        
        public static List<string> GetLocationsListByArea(IsLocationAreaActive isLocationAreaActive)
        {
            var locationAreas = new List<LocationArea>();

            if (isLocationAreaActive.Villages)
                locationAreas.Add(LocationArea.Villages);

            if (isLocationAreaActive.BlackZoneOutposts)
                locationAreas.Add(LocationArea.BlackZone);

            if (isLocationAreaActive.Cities)
                locationAreas.Add(LocationArea.Cities);

            var locations = new List<string>();

            foreach (var area in locationAreas)
            {
                switch (area)
                {
                    case LocationArea.BlackZone:
                        locations.Add(GetParameterName(Location.ArthursRest));
                        locations.Add(GetParameterName(Location.MerlynsRest));
                        locations.Add(GetParameterName(Location.MorganasRest));
                        break;

                    case LocationArea.Villages:
                        locations.Add(GetParameterName(Location.SwampCross));
                        locations.Add(GetParameterName(Location.ForestCross));
                        locations.Add(GetParameterName(Location.SteppeCross));
                        locations.Add(GetParameterName(Location.HighlandCross));
                        locations.Add(GetParameterName(Location.MountainCross));
                        break;

                    case LocationArea.Cities:
                        locations.Add(GetParameterName(Location.Thetford));
                        locations.Add(GetParameterName(Location.Lymhurst));
                        locations.Add(GetParameterName(Location.Bridgewatch));
                        locations.Add(GetParameterName(Location.BlackMarket));
                        locations.Add(GetParameterName(Location.Martlock));
                        locations.Add(GetParameterName(Location.FortSterling));
                        locations.Add(GetParameterName(Location.Caerleon));
                        break;
                }
            }
            return locations;
        }

        public static Brush GetLocationBrush(Location location, bool transparent)
        {
            try
            {
                if (transparent)
                {
                    return (Brush)Application.Current.Resources[$"SolidColorBrush.City.{GetParameterName(location)}.Transparent"];
                }
                else
                {
                    return (Brush)Application.Current.Resources[$"SolidColorBrush.City.{GetParameterName(location)}"];
                }
            }
            catch
            {
                return (Brush)Application.Current.Resources["SolidColorBrush.City.Default.Transparent"];
            }
        }

        public static Color GetLocationColor(Location location)
        {
            try
            {
                return (Color)Application.Current.Resources[$"Color.City.{location}"];
            }
            catch
            {
                return (Color)Application.Current.Resources["Color.City.Default"];
            }
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
        ArthursRest = 4300,
        MerlynsRest = -2,
        MorganasRest = -3,
    }

    public enum LocationArea
    {
        BlackZone,
        Villages,
        Cities
    }

    public class IsLocationAreaActive
    {
        public IsLocationAreaActive(bool blackZoneOutposts, bool villages, bool cities)
        {
            BlackZoneOutposts = blackZoneOutposts;
            Villages = villages;
            Cities = cities;
        }

        public bool BlackZoneOutposts { get; set; }
        public bool Villages { get; set; }
        public bool Cities { get; set; }
    }
}