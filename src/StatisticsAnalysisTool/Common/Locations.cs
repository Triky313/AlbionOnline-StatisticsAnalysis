using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Common;

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
        { Location.MorganasRest, "Morganas Rest" },
        { Location.SmugglersDen, "Smugglers Den" },
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
        { MarketLocation.SmugglersDen, "Smuggler's Den" },
    };

    public static string GetParameterName(Location location)
    {
        return ParameterNames.TryGetValue(location, out var name) ? name : null;
    }

    public static string GetDisplayName(MarketLocation location)
    {
        return DisplayNames.TryGetValue(location, out var name) ? name : null;
    }
    
    public static MarketLocation GetMarketLocationByIndex(string index)
    {
        if (string.IsNullOrEmpty(index))
        {
            return MarketLocation.Unknown;
        }

        if (index.Equals("@BLACK_MARKET"))
        {
            return MarketLocation.BlackMarket;
        }

        if (index.Equals("3013-Auction2"))
        {
            return MarketLocation.CaerleonMarket;
        }

        if (index.Contains("BLACKBANK") || index.Contains("SMUGGLER"))
        {
            return MarketLocation.SmugglersDen;
        }

        return Enum.TryParse(index, true, out MarketLocation location) ? location : MarketLocation.Unknown;
    }

    public static List<MarketLocation> GetAllMarketLocations()
    {
        var list = Enum.GetValues(typeof(MarketLocation)).Cast<MarketLocation>().ToList();
        _ = list.Remove(MarketLocation.Unknown);
        return list;
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

    public static SolidColorPaint GetLocationBrush(MarketLocation location, bool transparent)
    {
        try
        {
            var transparentText = transparent ? ".Transparent" : string.Empty;

            return location switch
            {
                MarketLocation.CaerleonMarket => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.Caerleon{transparentText}"]),
                MarketLocation.ThetfordMarket => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.Thetford{transparentText}"]),
                MarketLocation.BridgewatchMarket => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.Bridgewatch{transparentText}"]),
                MarketLocation.MartlockMarket => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.Martlock{transparentText}"]),
                MarketLocation.LymhurstMarket => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.Lymhurst{transparentText}"]),
                MarketLocation.FortSterlingMarket => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.FortSterling{transparentText}"]),
                MarketLocation.BrecilienMarket => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.Brecilien{transparentText}"]),
                MarketLocation.ArthursRest => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.ArthursRest{transparentText}"]),
                MarketLocation.MerlynsRest => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.MerlynsRest{transparentText}"]),
                MarketLocation.MorganasRest => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.MorganasRest{transparentText}"]),
                MarketLocation.BlackMarket => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.BlackMarket{transparentText}"]),
                MarketLocation.SmugglersDen => GetSolidColorPaint((SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.City.SmugglersDen{transparentText}"]),
                _ => new SolidColorPaint { Color = new SKColor(0, 0, 0, 0) }
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

    private static SolidColorPaint GetSolidColorPaint(SolidColorBrush solidColorBrush)
    {
        return new SolidColorPaint
        {
            Color = new SKColor(solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B, solidColorBrush.Color.A)
        };
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
    BrecilienMarket = 5003,
    SmugglersDen
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
    Brecilien = 5000,
    SmugglersDen
}

public enum LocationArea
{
    BlackZone,
    Villages,
    Cities,
    BlackMarket
}