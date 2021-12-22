using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.GameData
{
    public static class WorldData
    {
        public static ObservableCollection<ClusterInfo> MapData;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static string GetUniqueNameOrDefault(string index)
        {
            var name = MapData?.FirstOrDefault(x => x.Index == index)?.UniqueName ?? index;
            var splitName = name?.Split(new[] {"@"}, StringSplitOptions.None);

            if (splitName is { Length: > 0 } && name.ToLower().Contains('@'))
            {
                return GetMapNameByMapType(GetMapType(splitName[1]));
            }

            return name;
        }

        public static Guid? GetDungeonGuid(string index)
        {
            try
            {
                var splitName = index.Split(new[] {"@"}, StringSplitOptions.RemoveEmptyEntries);

                if (splitName.Length > 1 && index.ToLower().Contains('@'))
                {
                    var mapType = GetMapType(splitName[0]);
                    if (mapType is MapType.RandomDungeon or MapType.CorruptedDungeon or MapType.HellGate or MapType.Expedition && !string.IsNullOrEmpty(splitName[1]))
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

        private static string GetMapNameByMapType(MapType mapType)
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
            
            return MapType.Unknown;
        }

        public static async Task<bool> GetDataListFromJsonAsync()
        {
            var url = Settings.Default.WorldDataSourceUrl;
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.WorldDataFileName}";

            if (string.IsNullOrEmpty(url))
            {
                Log.Warn($"{nameof(GetDataListFromJsonAsync)}: No WorldDataSourceUrl found.");
                return false;
            }

            if (File.Exists(localFilePath))
            {
                var fileDateTime = File.GetLastWriteTime(localFilePath);

                if (fileDateTime.AddDays(Settings.Default.UpdateWorldDataByDays) < DateTime.Now)
                {
                    if (await GetWorldListFromWebAsync(url).ConfigureAwait(false))
                    {
                        MapData = GetWorldDataFromLocal();
                    }

                    return MapData?.Count > 0;
                }

                MapData = GetWorldDataFromLocal();
                return MapData?.Count > 0;
            }

            if (await GetWorldListFromWebAsync(url).ConfigureAwait(false))
            {
                MapData = GetWorldDataFromLocal();
            }

            return MapData?.Count > 0;
        }

        public static ClusterInfo GetClusterInfoByIndex(string clusterIndex, string mainClusterIndex, MapType mapType = MapType.Unknown,
            Guid? guid = null)
        {
            return MapData.FirstOrDefault(x => x.Index == clusterIndex) ?? new ClusterInfo
            {
                Index = clusterIndex,
                MainClusterIndex = mainClusterIndex,
                UniqueName = GetUniqueNameOrDefault(clusterIndex),
                MapType = mapType,
                Type = GetTypeByIndex(clusterIndex) ?? GetTypeByIndex(mainClusterIndex) ?? string.Empty,
                Guid = guid,
                File = GetFileByIndex(clusterIndex) ?? GetFileByIndex(mainClusterIndex) ?? string.Empty
            };
        }

        public static string GetTypeByIndex(string index)
        {
            return MapData?.FirstOrDefault(x => x.Index == index)?.Type;
        }

        public static string GetFileByIndex(string index)
        {
            return MapData?.FirstOrDefault(x => x.Index == index)?.File;
        }

        public static Tier GetTier(string value)
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

        public static ClusterType GetClusterType(string type)
        {
            if (IsAvalonClusterTunnel(type)) return ClusterType.AvalonTunnel;

            if (type.ToUpper().Contains("SAFEAREA")) return ClusterType.SafeArea;

            if (type.ToUpper().Contains("YELLOW")) return ClusterType.Yellow;

            if (type.ToUpper().Contains("RED")) return ClusterType.Red;

            if (type.ToUpper().Contains("BLACK")) return ClusterType.Black;

            return ClusterType.Unknown;
        }

        public static AvalonTunnelType GetTunnelType(string type)
        {
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

        public static bool IsAvalonClusterTunnel(string type)
        {
            return type.ToUpper().Contains("TUNNEL_BLACK_LOW") 
                   || (type.ToUpper().Contains("TUNNEL_BLACK_MEDIUM")) 
                   || (type.ToUpper().Contains("TUNNEL_BLACK_HIGH")) 
                   || (type.ToUpper().Contains("TUNNEL_LOW"))
                   || (type.ToUpper().Contains("TUNNEL_MEDIUM"))
                   || (type.ToUpper().Contains("TUNNEL_HIGH"))
                   || (type.ToUpper().Contains("TUNNEL_DEEP"))
                   || (type.ToUpper().Contains("TUNNEL_ROYAL"))
                   || (type.ToUpper().Contains("TUNNEL_HIDEOUT"));
        }

        #region Helper methods

        private static ObservableCollection<ClusterInfo> GetWorldDataFromLocal()
        {
            try
            {
                var options = new JsonSerializerOptions()
                {
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var localItemString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName, Settings.Default.WorldDataFileName), Encoding.UTF8);
                return ConvertItemJsonObjectToMapData(JsonSerializer.Deserialize<ObservableCollection<WorldJsonObject>>(localItemString, options));
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return new ObservableCollection<ClusterInfo>();
            }
        }

        private static ObservableCollection<ClusterInfo> ConvertItemJsonObjectToMapData(IEnumerable<WorldJsonObject> worldJsonObject)
        {
            var result = worldJsonObject.Select(item => new ClusterInfo
            {
                Index = item.Index,
                UniqueName = item.UniqueName,
                Type = item.Type,
                File = item.File
            }).ToList();

            var resultReturn = new ObservableRangeCollection<ClusterInfo>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                resultReturn.AddRange(result);
            });

            return resultReturn;
        }

        private static async Task<bool> GetWorldListFromWebAsync(string url)
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(300)
            };

            try
            {
                using var response = await client.GetAsync(url);
                using var content = response.Content;

                var fileString = await content.ReadAsStringAsync();
                await File.WriteAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", Settings.Default.WorldDataFileName), fileString, Encoding.UTF8);
                return true;
            }
            catch (HttpRequestException e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e.InnerException);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e.InnerException);
                return false;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return false;
            }
        }

        #endregion
    }

    public enum MapType
    {
        RandomDungeon,
        HellGate,
        CorruptedDungeon,
        Island,
        Hideout,
        Expedition,
        Arena,
        Unknown
    }
}