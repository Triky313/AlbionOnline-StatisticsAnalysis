using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameFileData;

public static class WorldData
{
    public static List<WorldJsonObject> MapData;

    public static string GetUniqueNameOrNull(string index)
    {
        return MapData?.FirstOrDefault(x => x?.Index == index)?.UniqueName;
    }

    public static string GetUniqueNameOrDefault(int index)
    {
        return GetUniqueNameOrDefault($"{index:0000}");
    }

    public static string GetUniqueNameOrDefault(string index)
    {
        var name = MapData?.FirstOrDefault(x => x.Index == index)?.UniqueName ?? index;
        var splitName = name?.Split(new[] { "@" }, StringSplitOptions.None);

        if (splitName is { Length: > 0 } && name.ToLower().Contains('@'))
        {
            return GetMapNameByMapType(GetMapType(splitName[1]));
        }

        return name;
    }

    public static List<MapMarkerType> GetMapMarkers(string index)
    {
        var miniMapMarkers = MapData?.FirstOrDefault(x => x.Index == index)?.MiniMapMarkers?.Marker ?? new List<Marker>();
        var mapMarkers = miniMapMarkers.Select(miniMapMarker => GetMapMarkerType(miniMapMarker.Type)).Where(marker => marker != MapMarkerType.Unknown).ToList();
        return mapMarkers;
    }

    private static MapMarkerType GetMapMarkerType(string value)
    {
        return value switch
        {
            "Stone" => MapMarkerType.Stone,
            "Ore" => MapMarkerType.Ore,
            "Hide" => MapMarkerType.Hide,
            "Wood" => MapMarkerType.Wood,
            "Fiber" => MapMarkerType.Fiber,
            "roads_of_avalon_solo_pve" => MapMarkerType.RoadsOfAvalonSoloPve,
            "roads_of_avalon_group_pve" => MapMarkerType.RoadsOfAvalonGroupPve,
            "roads_of_avalon_raid_pve" => MapMarkerType.RoadsOfAvalonRaidPve,
            "dungeon_elite" => MapMarkerType.DungeonElite,
            "dungeon_group" => MapMarkerType.DungeonGroup,
            "dungeon_solo" => MapMarkerType.DungeonSolo,
            _ => MapMarkerType.Unknown
        };
    }

    public static Guid? GetMapGuid(string index)
    {
        // Cluster change event
        // @ISLAND@c640e642-5135-4203-89b5-0007e4215605
        // @RANDOMDUNGEON@fe968505-9771-4653-8ade-29a1bd6ddb56 
        // @HIDEOUT@2306@29c344b3-2138-421d-a97c-06e29d4759ec
        // @MISTS@9283d553-ab71-4c14-bb34-64567137419a

        // Base vault info event
        // 4a82e5ed-8b64-40e3-926b-e0b020c4550a@@ISLAND@c640e642-5135-4203-89b5-0007e4215605
        // f56a368d-2f0b-4d01-a1ba-0079cf8b1fa9@4001

        try
        {
            var splitName = index.Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries);

            var mapTypeIndex = Array.FindIndex(splitName, indexString => indexString is "ISLAND" or "HIDEOUT");

            if (index.Contains("ISLAND") && mapTypeIndex > -1 && splitName.Length >= mapTypeIndex + 1)
            {
                var mapGuid = new Guid(splitName[mapTypeIndex + 1]);
                return mapGuid;
            }

            if (index.Contains("HIDEOUT") && mapTypeIndex > -1 && splitName.Length >= mapTypeIndex + 2)
            {
                var mapGuid = new Guid(splitName[mapTypeIndex + 2]);
                return mapGuid;
            }

            if (splitName.Length > 1 && index.ToLower().Contains('@'))
            {
                var mapType = GetMapType(splitName[0]);
                if (mapType is MapType.RandomDungeon or MapType.CorruptedDungeon or MapType.HellGate or MapType.Expedition or MapType.MistsDungeon or MapType.Mists && !string.IsNullOrEmpty(splitName[1]))
                {
                    var mapGuid = new Guid(splitName[1]);
                    return mapGuid;
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    public static string GetMapNameByMapType(MapType mapType)
    {
        return mapType switch
        {
            MapType.HellGate => LanguageController.Translation("HELLGATE"),
            MapType.RandomDungeon => LanguageController.Translation("DUNGEON"),
            MapType.CorruptedDungeon => LanguageController.Translation("CORRUPTED_LAIR"),
            MapType.Island => LanguageController.Translation("ISLAND"),
            MapType.Hideout => LanguageController.Translation("HIDEOUT"),
            MapType.Expedition => LanguageController.Translation("EXPEDITION"),
            MapType.Arena => LanguageController.Translation("ARENA"),
            MapType.MistsDungeon => LanguageController.Translation("MISTS_DUNGEON"),
            MapType.Mists => LanguageController.Translation("MISTS"),
            _ => LanguageController.Translation("UNKNOWN")
        };
    }

    public static MapType GetMapType(string index)
    {
        if (index.ToUpper().Contains("HELLCLUSTER")) return MapType.HellGate;

        if (index.ToUpper().Contains("RANDOMDUNGEON")) return MapType.RandomDungeon;

        if (index.ToUpper().Contains("CORRUPTEDDUNGEON")) return MapType.CorruptedDungeon;

        if (index.ToUpper().Contains("ISLAND")) return MapType.Island;

        if (index.ToUpper().Contains("HIDEOUT")) return MapType.Hideout;

        if (index.ToUpper().Contains("EXPEDITION")) return MapType.Expedition;

        if (index.ToUpper().Contains("ARENA")) return MapType.Arena;

        if (index.ToUpper().Contains("MISTSDUNGEON")) return MapType.MistsDungeon;

        if (index.ToUpper().Contains("MISTS")) return MapType.Mists;

        return MapType.Unknown;
    }

    public static ClusterType GetClusterTypeByIndex(string index)
    {
        var type = MapData?.FirstOrDefault(x => x?.Index == index)?.Type;

        if (type is null)
        {
            return ClusterType.Unknown;
        }

        if (type.Contains("SAFEAREA"))
        {
            return ClusterType.SafeArea;
        }

        if (type.Contains("YELLOW"))
        {
            return ClusterType.Yellow;
        }

        if (type.Contains("RED"))
        {
            return ClusterType.Red;
        }

        if (type.Contains("BLACK") || type.Contains("TUNNEL"))
        {
            return ClusterType.Black;
        }

        if (type.Contains("EXPEDITION"))
        {
            return ClusterType.Expedition;
        }

        if (type.Contains("CORRUPTED"))
        {
            return ClusterType.Corrupted;
        }

        ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, $"GetClusterType Unknown: {type}", ConsoleColorType.EventMapChangeColor);
        return ClusterType.Unknown;
    }

    public static string GetWorldJsonTypeByIndex(string index)
    {
        if (index == null)
        {
            return null;
        }

        var splitName = index.Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
        if (index.ToLower().Contains('@') && splitName.Length > 0 && !string.IsNullOrEmpty(splitName[0]))
        {
            return splitName[0];
        }

        return MapData?.FirstOrDefault(x => x.Index == index)?.Type;
    }

    public static string GetFileByIndex(string index)
    {
        return MapData?.FirstOrDefault(x => x.Index == index)?.File;
    }

    public static async Task<bool> LoadDataAsync()
    {
        var data = await GameData.LoadDataAsync<WorldJsonObject, WorldJsonRootObject>(
            Settings.Default.WorldDataFileName,
            Settings.Default.ModifiedWorldDataFileName,
            new JsonSerializerOptions()
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            });

        MapData = data;
        return data.Count >= 0;
    }

    public static Tier GetTier(int value)
    {
        return value switch
        {
            1 => Tier.T1,
            2 => Tier.T2,
            3 => Tier.T3,
            4 => Tier.T4,
            5 => Tier.T5,
            6 => Tier.T6,
            7 => Tier.T7,
            8 => Tier.T8,
            _ => Tier.Unknown
        };
    }
}