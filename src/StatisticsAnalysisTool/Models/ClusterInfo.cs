using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using System;

namespace StatisticsAnalysisTool.Models
{
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

        // Join data
        public string MainClusterIndex { get; set; }
        public string WorldJsonType { get; set; }
        public string File { get; set; }

        public string UniqueName => WorldData.GetUniqueNameOrDefault(Index) ?? WorldData.GetMapNameByMapType(MapType);
        public string UniqueClusterName => WorldData.GetUniqueNameOrDefault(Index) ?? InstanceName ?? string.Empty;
        public ClusterType ClusterType => GetClusterType(WorldJsonType);
        public bool IsAvalonClusterTunnel => IsAvalonClusterTunnelByType(WorldJsonType);
        public AvalonTunnelType AvalonTunnelType => GetTunnelType(WorldJsonType);
        public Tier Tier => GetTier(File);

        public string TierString 
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

        private static ClusterType GetClusterType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return ClusterType.Unknown;
            }

            if (IsAvalonClusterTunnelByType(type))
            {
                return ClusterType.AvalonTunnel;
            }

            if (type.ToUpper().Contains("SAFEAREA") || type.ToUpper().Equals("HIDEOUT"))
            {
                return ClusterType.SafeArea;
            }
            
            if (type.ToUpper().Contains("ISLAND"))
            {
                return ClusterType.Island;
            }
            
            if (type.ToUpper().Contains("YELLOW"))
            {
                return ClusterType.Yellow;
            }

            if (type.ToUpper().Contains("RED"))
            {
                return ClusterType.Red;
            }

            if (type.ToUpper().Contains("BLACK"))
            {
                return ClusterType.Black;
            }

            return ClusterType.Unknown;
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
    }
}