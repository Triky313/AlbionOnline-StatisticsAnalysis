using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Common
{
    public static class Locations
    {
        public static readonly Dictionary<Location, string> ParameterNames = new()
        {
            { Location.Thetford, "Thetford" },
            { Location.SwampCross, "Swamp Cross" },
            { Location.Lymhurst, "Lymhurst" },
            { Location.ForestCross, "Forest Cross" },
            { Location.Bridgewatch, "Bridgewatch" },
            { Location.SteppeCross, "Steppe Cross" },
            { Location.HighlandCross, "Highland Cross" },
            { Location.BlackMarket, "Black Market" },
            { Location.Martlock, "Martlock" },
            { Location.Caerleon, "Caerleon" },
            { Location.FortSterling, "Fort Sterling" },
            { Location.Brecilien, "Brecilien" },
            { Location.MountainCross, "Mountain Cross" },
            { Location.ArthursRest, "Arthurs Rest" },
            { Location.MerlynsRest, "Merlyns Rest" },
            { Location.MorganasRest, "Morganas Rest" }
        };

        public static readonly Dictionary<MarketLocation, string> DisplayNames = new()
        {
            { MarketLocation.ThetfordMarket, "Thetford" },
            { MarketLocation.ThetfordPortal, "Thetford" },
            { MarketLocation.SwampCross, "Swamp Cross" },
            { MarketLocation.LymhurstMarket, "Lymhurst" },
            { MarketLocation.LymhurstPortal, "Lymhurst" },
            { MarketLocation.ForestCross, "Forest Cross" },
            { MarketLocation.BridgewatchMarket, "Bridgewatch" },
            { MarketLocation.BridgewatchPortal, "Bridgewatch" },
            { MarketLocation.SteppeCross, "Steppe Cross" },
            { MarketLocation.HighlandCross, "Highland Cross" },
            { MarketLocation.BlackMarket, "Black Market" },
            { MarketLocation.MartlockMarket, "Martlock" },
            { MarketLocation.MartlockPortal, "Martlock" },
            { MarketLocation.CaerleonMarket, "Caerleon" },
            { MarketLocation.FortSterlingMarket, "Fort Sterling" },
            { MarketLocation.FortSterlingPortal, "Fort Sterling" },
            { MarketLocation.BrecilienMarket, "Brecilien" },
            { MarketLocation.MountainCross, "Mountain Cross" },
            { MarketLocation.ArthursRest, "Arthurs Rest" },
            { MarketLocation.MerlynsRest, "Merlyns Rest" },
            { MarketLocation.MorganasRest, "Morganas Rest" },
        };

        public static string GetParameterName(Location location)
        {
            return ParameterNames.TryGetValue(location, out var name) ? name : null;
        }

        public static string GetDisplayName(MarketLocation location)
        {
            return DisplayNames.TryGetValue(location, out var name) ? name : null;
        }

        [Obsolete]
        public static Location GetLocationByIndex(string index)
        {
            if (index.Equals("@BLACK_MARKET"))
            {
                return Location.BlackMarket;
            }

            if (index.Equals("3013-Auction2"))
            {
                return Location.Caerleon;
            }
            
            return Enum.TryParse(index, true, out Location location) ? location : Location.Unknown;
        }

        public static MarketLocation GetMarketLocationByIndex(string index)
        {
            if (index.Equals("@BLACK_MARKET"))
            {
                return MarketLocation.BlackMarket;
            }

            if (index.Equals("3013-Auction2"))
            {
                return MarketLocation.CaerleonMarket;
            }

            return Enum.TryParse(index, true, out MarketLocation location) ? location : MarketLocation.Unknown;
        }

        public static List<MarketLocation> GetAllMarketLocations()
        {
            var list = Enum.GetValues(typeof(MarketLocation)).Cast<MarketLocation>().ToList();
            _ = list.Remove(MarketLocation.Unknown);
            return list;
        }

        [Obsolete]
        public static Location GetLocationByLocationNameOrId(string location)
        {
            return location switch
            {
                "Thetford" => Location.Thetford,
                "Lymhurst" => Location.Lymhurst,
                "Bridgewatch" => Location.Bridgewatch,
                "Martlock" => Location.Martlock,
                "Fort Sterling" => Location.FortSterling,
                "0301" or "Thetford Portal" => Location.ThetfordPortal,
                "1301" or "Lymhurst Portal" => Location.LymhurstPortal,
                "2301" or "Bridgewatch Portal" => Location.BridgewatchPortal,
                "3301" or "Martlock Portal" => Location.MartlockPortal,
                "4301" or "Fort Sterling Portal" => Location.FortSterlingPortal,
                "5000" or "5001" or "Brecilien" => Location.Brecilien,
                "Caerleon" => Location.Caerleon,
                "Swamp Cross" => Location.SwampCross,
                "Forest Cross" => Location.ForestCross,
                "Steppe Cross" => Location.SteppeCross,
                "Highland Cross" => Location.HighlandCross,
                "Mountain Cross" => Location.MountainCross,
                "Arthurs Rest" => Location.ArthursRest,
                "Merlyns Rest" => Location.MerlynsRest,
                "Morganas Rest" => Location.MorganasRest,
                "Black Market" => Location.BlackMarket,
                _ => Location.Unknown,
            };
        }

        public static MarketLocation GetMarketLocationByLocationNameOrId(this string location)
        {
            return location switch
            {
                "0007" or "Thetford" => MarketLocation.ThetfordMarket,
                "1002" or "Lymhurst" => MarketLocation.LymhurstMarket,
                "2004" or "Bridgewatch" => MarketLocation.BridgewatchMarket,
                "3008" or "Martlock" => MarketLocation.MartlockMarket,
                "4002" or "Fort Sterling" => MarketLocation.FortSterlingMarket,
                "0301" or "Thetford Portal" => MarketLocation.ThetfordPortal,
                "1301" or "Lymhurst Portal" => MarketLocation.LymhurstPortal,
                "2301" or "Bridgewatch Portal" => MarketLocation.BridgewatchPortal,
                "3301" or "Martlock Portal" => MarketLocation.MartlockPortal,
                "4301" or "Fort Sterling Portal" => MarketLocation.FortSterlingPortal,
                "5003" or "Brecilien" => MarketLocation.BrecilienMarket,
                "3005" or "Caerleon" => MarketLocation.CaerleonMarket,
                "0004" or "Swamp Cross" => MarketLocation.SwampCross,
                "1006" or "Forest Cross" => MarketLocation.ForestCross,
                "2002" or "Steppe Cross" => MarketLocation.SteppeCross,
                "3002" or "Highland Cross" => MarketLocation.HighlandCross,
                "4006" or "Mountain Cross" => MarketLocation.MountainCross,
                "4300" or "Arthurs Rest" => MarketLocation.ArthursRest,
                "1012" or "Merlyns Rest" => MarketLocation.MerlynsRest,
                "0008" or "Morganas Rest" => MarketLocation.MorganasRest,
                "3003" or "Black Market" => MarketLocation.BlackMarket,
                _ => MarketLocation.Unknown,
            };
        }
        
        public static SolidColorPaint GetLocationBrush(Location location, bool transparent)
        {
            if (location == Location.Unknown)
            {
                return new SolidColorPaint
                {
                    Color = new SKColor(0, 0, 0, 0)
                };
            }

            try
            {
                if (transparent)
                {
                    var scbt = (SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.{location}.Transparent"];
                    return new SolidColorPaint
                    {
                        Color = new SKColor(scbt.Color.R, scbt.Color.G, scbt.Color.B, scbt.Color.A)
                    };
                }

                var scb = (SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.{location}"];
                return new SolidColorPaint
                {
                    Color = new SKColor(scb.Color.R, scb.Color.G, scb.Color.B, scb.Color.A)
                };
            }
            catch
            {
                return new SolidColorPaint
                {
                    Color = new SKColor(0, 0, 0, 0)
                };
            }
        }

        [Obsolete]
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

        public static Color GetLocationColor(MarketLocation location)
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

    public enum MarketLocation
    {
        Unknown = 0000,
        SwampCross = 0004,
        ThetfordMarket = 0007,
        ThetfordPortal = 0301,
        LymhurstMarket = 1002,
        LymhurstPortal = 1301,
        ForestCross = 1006,
        SteppeCross = 2002,
        BridgewatchMarket = 2004,
        BridgewatchPortal = 2301,
        HighlandCross = 3002,
        BlackMarket = 3003,
        CaerleonMarket = 3005,
        MartlockMarket = 3008,
        MartlockPortal = 3301,
        FortSterlingMarket = 4002,
        FortSterlingPortal = 4301,
        MountainCross = 4006,
        ArthursRest = 4300,
        MerlynsRest = 1012,
        MorganasRest = 0008,
        BrecilienMarket = 5003
    }

    // TODO: Rework with correct city ID's otherwise use MarketLocation
    public enum Location
    {
        Unknown = 0000,
        SwampCross = 0004,
        Thetford = 0007,
        ThetfordPortal = 0301,
        Lymhurst = 1002,
        LymhurstPortal = 1301,
        ForestCross = 1006,
        SteppeCross = 2002,
        Bridgewatch = 2004,
        BridgewatchPortal = 2301,
        HighlandCross = 3002,
        BlackMarket = 3003,
        Caerleon = 3005,
        Martlock = 3008,
        MartlockPortal = 3301,
        FortSterling = 4002,
        FortSterlingPortal = 4301,
        MountainCross = 4006,
        ArthursRest = 4300,
        MerlynsRest = 1012,
        MorganasRest = 0008,
        Brecilien = 5000
    }

    public enum LocationArea
    {
        BlackZone,
        Villages,
        Cities,
        BlackMarket
    }
}