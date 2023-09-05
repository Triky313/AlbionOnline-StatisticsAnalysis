using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Cluster;

public class ClusterInfo
{
    public bool ClusterInfoFullyAvailable { get; set; }

    // Change cluster data
    public DateTime Entered { get; set; }
    public MapType MapType { get; set; } = MapType.Unknown;
    public Guid? Guid { get; set; }
    public string Index { get; set; }
    public string InstanceName { get; set; }
    public string WorldMapDataType { get; set; }
    public byte[] DungeonInformation { get; set; }
    public Tier MistsDungeonTier { get; set; }

    // Join data
    public string MainClusterIndex { get; set; }
    public string WorldJsonType { get; private set; }
    public string File { get; private set; }

    public string UniqueName { get; private set; }
    public string UniqueClusterName { get; private set; }
    public ClusterMode ClusterMode { get; private set; }
    public AvalonTunnelType AvalonTunnelType { get; private set; }
    public Tier Tier { get; private set; }
    public string MapTypeString { get; private set; }
    public string ClusterHistoryString1 { get; private set; }
    public string ClusterHistoryString2 { get; private set; }
    public string ClusterHistoryString3 { get; private set; }
    public MistsRarity MistsRarity { get; private set; }
    public List<MapMarkerType> MapMarkers => WorldData.GetMapMarkers(Index);

    public ClusterInfo()
    {
    }

    public ClusterInfo(ClusterInfo clusterInfo)
    {
        ClusterInfoFullyAvailable = clusterInfo.ClusterInfoFullyAvailable;
        Entered = clusterInfo.Entered;
        MapType = clusterInfo.MapType;
        Guid = clusterInfo.Guid;
        Index = clusterInfo.Index;
        InstanceName = clusterInfo.InstanceName;
        WorldMapDataType = clusterInfo.WorldMapDataType;
        DungeonInformation = clusterInfo.DungeonInformation;
        MainClusterIndex = clusterInfo.MainClusterIndex;
        WorldJsonType = clusterInfo.WorldJsonType;
        File = clusterInfo.File;
        UniqueName = clusterInfo.UniqueName;
        UniqueClusterName = clusterInfo.UniqueClusterName;
        Tier = clusterInfo.Tier;
        MistsDungeonTier = clusterInfo.MistsDungeonTier;
        ClusterMode = clusterInfo.ClusterMode;
        AvalonTunnelType = clusterInfo.AvalonTunnelType;
        MapTypeString = clusterInfo.MapTypeString;
        ClusterHistoryString();
    }

    public void SetClusterInfo(MapType mapType, Guid? mapGuid, string clusterIndex, string instanceName, string worldMapDataType, byte[] dungeonInformation, string mainClusterIndex, Tier mistsDungeonTier)
    {
        Entered = DateTime.UtcNow;
        MapType = mapType;
        Guid = mapGuid;
        Index = clusterIndex;
        InstanceName = instanceName;
        WorldMapDataType = worldMapDataType;
        DungeonInformation = dungeonInformation;
        MistsDungeonTier = mistsDungeonTier;
        MistsRarity = GetMistsRarity(dungeonInformation);

        MainClusterIndex = mainClusterIndex;
        WorldJsonType = null;
        File = null;

        UniqueName = WorldData.GetUniqueNameOrNull(Index);
        UniqueClusterName = WorldData.GetUniqueNameOrDefault(Index) ?? InstanceName ?? string.Empty;
        MapTypeString = GetMapTypeName(MapType);
    }

    public void SetJoinClusterInfo(string index, string mainClusterIndex)
    {
        MainClusterIndex ??= mainClusterIndex;
        WorldJsonType = WorldData.GetWorldJsonTypeByIndex(index) ?? WorldData.GetWorldJsonTypeByIndex(mainClusterIndex) ?? string.Empty;
        File = WorldData.GetFileByIndex(index) ?? WorldData.GetFileByIndex(mainClusterIndex) ?? string.Empty;

        Tier = GetTier(File);
        ClusterMode = GetClusterType(WorldJsonType);
        AvalonTunnelType = GetTunnelType(WorldJsonType);
        ClusterHistoryString();
    }

    public void ClusterHistoryString()
    {
        if (ClusterMode is ClusterMode.Black or ClusterMode.Red or ClusterMode.Yellow or ClusterMode.SafeArea && MapType is MapType.Unknown)
        {
            ClusterHistoryString1 = ClusterModeString(ClusterMode);
            ClusterHistoryString2 = UniqueName;
            ClusterHistoryString3 = string.Empty;
            return;
        }

        if (MapType is MapType.Arena or MapType.CorruptedDungeon or MapType.RandomDungeon or MapType.Expedition or MapType.HellGate or MapType.MistsDungeon or MapType.Mists)
        {
            ClusterHistoryString1 = MapTypeString;
            ClusterHistoryString2 = string.Empty;
            ClusterHistoryString3 = string.Empty;
            return;
        }

        if (MapType is MapType.Island or MapType.Hideout)
        {
            ClusterHistoryString1 = ClusterModeString(ClusterMode);
            ClusterHistoryString2 = InstanceName;
            ClusterHistoryString3 = string.Empty;
            return;
        }

        if (ClusterMode is ClusterMode.AvalonTunnel)
        {
            ClusterHistoryString1 = ClusterModeString(ClusterMode);
            ClusterHistoryString2 = UniqueName;
            ClusterHistoryString3 = AvalonTunnelType.ToString();
        }
    }

    private MistsRarity GetMistsRarity(byte[] infoArray)
    {
        if (infoArray is null)
        {
            return MistsRarity.Unknown;
        }

        var rarity = DungeonInformation[^1];

        return rarity switch
        {
            0 => MistsRarity.Common,
            1 => MistsRarity.Uncommon,
            2 => MistsRarity.Rare,
            3 => MistsRarity.Epic,
            4 => MistsRarity.Legendary,
            _ => MistsRarity.Unknown
        };
    }

    public string TierRomanString
    {
        get
        {
            return Tier switch
            {
                Tier.T1 => "I",
                Tier.T2 => "II",
                Tier.T3 => "III",
                Tier.T4 => "IV",
                Tier.T5 => "V",
                Tier.T6 => "VI",
                Tier.T7 => "VII",
                Tier.T8 => "VIII",
                Tier.Unknown => "?",
                _ => "?"
            };
        }
    }

    public string TierString
    {
        get
        {
            return Tier switch
            {
                Tier.T1 => "T1",
                Tier.T2 => "T2",
                Tier.T3 => "T3",
                Tier.T4 => "T4",
                Tier.T5 => "T5",
                Tier.T6 => "T6",
                Tier.T7 => "T7",
                Tier.T8 => "T8",
                Tier.Unknown => "T?",
                _ => "T?"
            };
        }
    }

    private static string ClusterModeString(ClusterMode clusterMode)
    {
        return clusterMode switch
        {
            ClusterMode.Island => LanguageController.Translation("ISLAND"),
            ClusterMode.AvalonTunnel => LanguageController.Translation("AVALON_ROAD"),
            ClusterMode.Black => LanguageController.Translation("BLACK_ZONE"),
            ClusterMode.Red => LanguageController.Translation("RED_ZONE"),
            ClusterMode.Yellow => LanguageController.Translation("YELLOW_ZONE"),
            ClusterMode.SafeArea => LanguageController.Translation("SAFE_AREA"),
            ClusterMode.Mists => LanguageController.Translation("MISTS"),
            ClusterMode.Unknown => LanguageController.Translation("UNKNOWN_ZONE"),
            _ => ""
        };
    }

    private static bool IsAvalonClusterTunnelByType(string type)
    {
        if (string.IsNullOrEmpty(type))
        {
            return false;
        }

        return type.ToUpper().Contains("TUNNEL_BLACK_LOW")
               || type.ToUpper().Contains("TUNNEL_BLACK_MEDIUM")
               || type.ToUpper().Contains("TUNNEL_BLACK_HIGH")
               || type.ToUpper().Contains("TUNNEL_LOW")
               || type.ToUpper().Contains("TUNNEL_MEDIUM")
               || type.ToUpper().Contains("TUNNEL_HIGH")
               || type.ToUpper().Contains("TUNNEL_DEEP")
               || type.ToUpper().Contains("TUNNEL_ROYAL")
               || type.ToUpper().Contains("TUNNEL_HIDEOUT");
    }

    private static Tier GetTier(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Tier.Unknown;
        }

        if (value.Contains("_T1_"))
        {
            return Tier.T1;
        }

        if (value.Contains("_T2_"))
        {
            return Tier.T2;
        }

        if (value.Contains("_T3_"))
        {
            return Tier.T3;
        }

        if (value.Contains("_T4_"))
        {
            return Tier.T4;
        }

        if (value.Contains("_T5_"))
        {
            return Tier.T5;
        }

        if (value.Contains("_T6_"))
        {
            return Tier.T6;
        }

        if (value.Contains("_T7_"))
        {
            return Tier.T7;
        }

        if (value.Contains("_T8_"))
        {
            return Tier.T8;
        }

        return Tier.Unknown;
    }

    private static ClusterMode GetClusterType(string type)
    {
        if (string.IsNullOrEmpty(type))
        {
            return ClusterMode.Unknown;
        }

        if (IsAvalonClusterTunnelByType(type))
        {
            return ClusterMode.AvalonTunnel;
        }

        if (type.ToUpper().Contains("SAFEAREA") || type.ToUpper().Equals("HIDEOUT") || type.ToUpper().Contains("PLAYERCITY"))
        {
            return ClusterMode.SafeArea;
        }

        if (type.ToUpper().Contains("ISLAND"))
        {
            return ClusterMode.Island;
        }

        if (type.ToUpper().Contains("YELLOW"))
        {
            return ClusterMode.Yellow;
        }

        if (type.ToUpper().Contains("RED"))
        {
            return ClusterMode.Red;
        }

        if (type.ToUpper().Contains("BLACK"))
        {
            return ClusterMode.Black;
        }

        if (type.ToUpper().Contains("MISTS"))
        {
            return ClusterMode.Mists;
        }

        return ClusterMode.Unknown;
    }

    private static AvalonTunnelType GetTunnelType(string type)
    {
        if (string.IsNullOrEmpty(type))
        {
            return AvalonTunnelType.Unknown;
        }

        if (type.ToUpper().Contains("TUNNEL_BLACK_LOW")) return AvalonTunnelType.TunnelBlackLow;

        if (type.ToUpper().Contains("TUNNEL_BLACK_MEDIUM")) return AvalonTunnelType.TunnelBlackMedium;

        if (type.ToUpper().Contains("TUNNEL_BLACK_HIGH")) return AvalonTunnelType.TunnelBlackHigh;

        if (type.ToUpper().Contains("TUNNEL_LOW")) return AvalonTunnelType.TunnelLow;

        if (type.ToUpper().Contains("TUNNEL_MEDIUM")) return AvalonTunnelType.TunnelMedium;

        if (type.ToUpper().Contains("TUNNEL_HIGH")) return AvalonTunnelType.TunnelHigh;

        if (type.ToUpper().Contains("TUNNEL_DEEP")) return AvalonTunnelType.TunnelDeep;

        if (type.ToUpper().Contains("TUNNEL_ROYAL")) return AvalonTunnelType.TunnelRoyal;

        if (type.ToUpper().Contains("TUNNEL_HIDEOUT")) return AvalonTunnelType.TunnelHideout;

        return AvalonTunnelType.Unknown;
    }

    private static string GetMapTypeName(MapType mapType)
    {
        return mapType switch
        {
            MapType.RandomDungeon => LanguageController.Translation("DUNGEON"),
            MapType.HellGate => LanguageController.Translation("HELLGATE"),
            MapType.CorruptedDungeon => LanguageController.Translation("CORRUPTED_DUNGEON"),
            MapType.Island => LanguageController.Translation("ISLAND"),
            MapType.Hideout => LanguageController.Translation("HIDEOUT"),
            MapType.Expedition => LanguageController.Translation("EXPEDITION"),
            MapType.Arena => LanguageController.Translation("ARENA"),
            MapType.MistsDungeon => LanguageController.Translation("MISTS_DUNGEON"),
            MapType.Mists => LanguageController.Translation("MISTS"),
            _ => ""
        };
    }
}