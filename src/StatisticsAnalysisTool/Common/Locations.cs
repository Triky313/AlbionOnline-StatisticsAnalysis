using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using StatisticsAnalysisTool.GameFileData;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Common;

public static class Locations
{
    public static readonly Dictionary<MarketLocation, string> ParameterNames = new()
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
        { MarketLocation.SmugglersDen, "Smugglers Den" },
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

    public static string GetParameterName(MarketLocation location)
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
    
    public static List<string> GetAllMarketLocations()
    {
        return new List<string>()
        {
            "Lymhurst",
            "Martlock",
            "FortSterling",
            "Thetford",
            "Bridgewatch",
            "Caerleon",
            "BlackMarket",
            "Brecilien",
            "SwampCross",
            "ForestCross",
            "SteppeCross",
            "HighlandCross",
            "MountainCross",
            "SmugglersNetwork"
        };
    }

    public static readonly HashSet<string> SmugglersNetworkNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Arthurs Rest Smugglers Network", "Bleachskull Desert Smugglers Network", "DeadpineForest Smugglers Network",
        "Deepwood Copse Smugglers Network", "Driftwood Hollow Smugglers Network", "Dry Basin Riverbed Smugglers Network",
        "Dryvein Confluence Smugglers Network", "Everwinter Peak Smugglers Network", "Farshore Heath Smugglers Network",
        "Floatshoal Floe Smugglers Network", "Frostspring Volcano Smugglers Network", "Gravemound Knoll Smugglers Network",
        "Highstone Loch Smugglers Network", "Iceburn Firth Smugglers Network", "Meltwater Bog Smugglers Network",
        "Merlyns Rest Smugglers Network", "Morganas Rest Smugglers Network", "Munten Fell Smugglers Network",
        "Murdergulch Cross Smugglers Network", "Murdergulch Ravine Smugglers Network", "Razorrock Bank Smugglers Network",
        "Razorrock Verge Smugglers Network", "River Copse Fount Smugglers Network", "Runnelvein Sink Smugglers Network",
        "Scuttle Sink Marsh Smugglers Network", "Slakesands Mesa Smugglers Network", "Springsump Basin Smugglers Network",
        "Sunfang Ravine Smugglers Network", "Sunfang Wasteland Smugglers Network", "Sunkenbough Woods Smugglers Network",
        "Sunstrand Quicksands Smugglers Network", "Thirstwater Steppe Smugglers Network", "Timberscar Copse Smugglers Network",
        "Timberslope Grove Smugglers Network", "Westweald Thicket Smugglers Network", "White Peak Tundra Smugglers Network",
        "Willowshade Hills Smugglers Network", "Willowshade Ice Marsh Smugglers Network",
        "BLACKBANK-2310", "BLACKBANK-0321", "BLACKBANK-0307", "BLACKBANK-4322",
        "BLACKBANK-2336", "BLACKBANK-0320", "BLACKBANK-0341", "BLACKBANK-0344",
        "BLACKBANK-0349", "BLACKBANK-0353", "BLACKBANK-1312", "BLACKBANK-1323",
        "BLACKBANK-1339", "BLACKBANK-1342", "BLACKBANK-1343", "BLACKBANK-1348",
        "BLACKBANK-1359", "BLACKBANK-2308", "BLACKBANK-2311", "BLACKBANK-2333",
        "BLACKBANK-2342", "BLACKBANK-2344", "BLACKBANK-2347", "BLACKBANK-2348",
        "BLACKBANK-3306", "BLACKBANK-3344", "BLACKBANK-3345", "BLACKBANK-3351",
        "BLACKBANK-3355", "BLACKBANK-3357", "BLACKBANK-4313", "BLACKBANK-4318",
        "BLACKBANK-4345", "BLACKBANK-4351", "BLACKBANK-4357", "Smuggler's Den"
    };

    public static MarketLocation GetMarketLocationByLocationNameOrId(this string location)
    {
        if (string.IsNullOrEmpty(location))
        {
            return MarketLocation.Unknown;
        }

        var atIndex = location.LastIndexOf('@');
        var relevantPart = atIndex >= 0 ? location.Substring(atIndex + 1) : location;

        // Smugglers Network
        if (SmugglersNetworkNames.Contains(relevantPart))
        {
            return MarketLocation.SmugglersDen;
        }

        // Normal markets
        return location switch
        {
            "0007" or "0000-HellDen" or "Thetford" => MarketLocation.ThetfordMarket,
            "1002" or "1000-HellDen" or "Lymhurst" => MarketLocation.LymhurstMarket,
            "2004" or "2000-HellDen" or "Bridgewatch" => MarketLocation.BridgewatchMarket,
            "3008" or "3004-HellDen" or "Martlock" => MarketLocation.MartlockMarket,
            "4002" or "4000-HellDen" or "Fort Sterling" => MarketLocation.FortSterlingMarket,
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
            "3003" or "Black Market" or "@BLACK_MARKET" => MarketLocation.BlackMarket,
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
                MarketLocation.CaerleonMarket => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.Caerleon{transparentText}"]),
                MarketLocation.ThetfordMarket => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.Thetford{transparentText}"]),
                MarketLocation.BridgewatchMarket => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.Bridgewatch{transparentText}"]),
                MarketLocation.MartlockMarket => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.Martlock{transparentText}"]),
                MarketLocation.LymhurstMarket => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.Lymhurst{transparentText}"]),
                MarketLocation.FortSterlingMarket => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.FortSterling{transparentText}"]),
                MarketLocation.BrecilienMarket => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.Brecilien{transparentText}"]),
                MarketLocation.ArthursRest => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.ArthursRest{transparentText}"]),
                MarketLocation.MerlynsRest => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.MerlynsRest{transparentText}"]),
                MarketLocation.MorganasRest => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.MorganasRest{transparentText}"]),
                MarketLocation.BlackMarket => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.BlackMarket{transparentText}"]),
                MarketLocation.SmugglersDen => GetSolidColorPaint((SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.City.SmugglersDen{transparentText}"]),
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
            return (Color) Application.Current.Resources[$"Color.City.{location}"];
        }
        catch
        {
            return (Color) Application.Current.Resources["Color.City.Default"];
        }
    }

    public static KeyValuePair<MarketLocation, string>[] OnceMarketLocations { get; } =
    {
        new (MarketLocation.BlackMarket, "Black Market"),
        new (MarketLocation.MartlockMarket, WorldData.GetUniqueNameOrDefault("3004")),
        new (MarketLocation.ThetfordMarket, WorldData.GetUniqueNameOrDefault("0000")),
        new (MarketLocation.FortSterlingMarket, WorldData.GetUniqueNameOrDefault("4000")),
        new (MarketLocation.LymhurstMarket, WorldData.GetUniqueNameOrDefault("1000")),
        new (MarketLocation.BridgewatchMarket, WorldData.GetUniqueNameOrDefault("2000")),
        new (MarketLocation.CaerleonMarket, WorldData.GetUniqueNameOrDefault("3003")),
        new (MarketLocation.BrecilienMarket, WorldData.GetUniqueNameOrDefault("5000")),
        //new (MarketLocation.MerlynsRest, WorldData.GetUniqueNameOrDefault("1012")),
        //new (MarketLocation.MorganasRest, WorldData.GetUniqueNameOrDefault("0008")),
        //new (MarketLocation.ArthursRest, WorldData.GetUniqueNameOrDefault("4300")),
        new (MarketLocation.SmugglersDen, "Smuggler's Den")
    };
}

public enum MarketLocation
{
    Unknown,
    SwampCross,
    ThetfordMarket,
    ThetfordPortal,
    LymhurstMarket,
    LymhurstPortal,
    ForestCross,
    SteppeCross,
    BridgewatchMarket,
    BridgewatchPortal,
    HighlandCross,
    BlackMarket,
    CaerleonMarket,
    MartlockMarket,
    MartlockPortal,
    FortSterlingMarket,
    FortSterlingPortal,
    MountainCross,
    ArthursRest,
    MerlynsRest,
    MorganasRest,
    BrecilienMarket,
    SmugglersDen
}

public enum LocationArea
{
    BlackZone,
    Villages,
    Cities,
    BlackMarket
}