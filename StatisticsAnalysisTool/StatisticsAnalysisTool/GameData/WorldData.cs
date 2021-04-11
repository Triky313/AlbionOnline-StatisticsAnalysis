using log4net;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameData
{
    public static class WorldData
    {
        public static ObservableCollection<ClusterInfo> MapData;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetUniqueNameOrDefault(string index)
        {
            var name = MapData?.FirstOrDefault(x => x.Index == index)?.UniqueName ?? index;
            var splitName = name?.Split(new[] {"@"}, StringSplitOptions.None);

            if (splitName != null && splitName.Length > 0 && name.ToLower().Contains('@')) return GetMapNameByMapType(GetMapType(splitName[1]));

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
                    if ((mapType == MapType.RandomDungeon || mapType == MapType.CorruptedDungeon || mapType == MapType.HellGate) && !string.IsNullOrEmpty(splitName[1]))
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
            switch (mapType)
            {
                case MapType.HellGate:
                    return LanguageController.Translation("HELLGATE");
                case MapType.RandomDungeon:
                    return LanguageController.Translation("DUNGEON");
                case MapType.CorruptedDungeon:
                    return LanguageController.Translation("CORRUPTED_LAIR");
                case MapType.Island:
                    return LanguageController.Translation("ISLAND");
                case MapType.Hideout:
                    return LanguageController.Translation("HIDEOUT");
                default:
                    return LanguageController.Translation("UNKNOWN");
            }
        }

        public static MapType GetMapType(string index)
        {
            if (index.ToUpper().Contains("HELLCLUSTER")) return MapType.HellGate;

            if (index.ToUpper().Contains("RANDOMDUNGEON")) return MapType.RandomDungeon;

            if (index.ToUpper().Contains("CORRUPTEDDUNGEON")) return MapType.CorruptedDungeon;

            if (index.ToUpper().Contains("ISLAND")) return MapType.Island;

            if (index.ToUpper().Contains("HIDEOUT")) return MapType.Hideout;

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
                    if (await GetWorldListFromWebAsync(url)) MapData = GetWorldDataFromLocal();
                    return MapData?.Count > 0;
                }

                MapData = GetWorldDataFromLocal();
                return MapData?.Count > 0;
            }

            if (await GetWorldListFromWebAsync(url)) MapData = GetWorldDataFromLocal();
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
                Guid = guid
            };
        }

        public static string GetTypeByIndex(string index)
        {
            return MapData?.FirstOrDefault(x => x.Index == index)?.Type;
        }

        public static ClusterType GetClusterType(string type)
        {
            if (type.ToUpper().Contains("SAFEAREA")) return ClusterType.SafeArea;

            if (type.ToUpper().Contains("YELLOW")) return ClusterType.Yellow;

            if (type.ToUpper().Contains("RED")) return ClusterType.Red;

            if (type.ToUpper().Contains("BLACK")) return ClusterType.Black;

            return ClusterType.Unknown;
        }

        #region Helper methods

        private static ObservableCollection<ClusterInfo> GetWorldDataFromLocal()
        {
            try
            {
                var localItemString =
                    File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", Settings.Default.WorldDataFileName),
                        Encoding.UTF8);
                return ConvertItemJsonObjectToMapData(JsonConvert.DeserializeObject<ObservableCollection<WorldJsonObject>>(localItemString));
            }
            catch (Exception e)
            {
                Log.Error(nameof(GetWorldDataFromLocal), e);
                return new ObservableCollection<ClusterInfo>();
            }
        }

        private static ObservableCollection<ClusterInfo> ConvertItemJsonObjectToMapData(ObservableCollection<WorldJsonObject> worldJsonObject)
        {
            var result = worldJsonObject.Select(item => new ClusterInfo
            {
                Index = item.Index,
                UniqueName = item.UniqueName,
                Type = item.Type
            }).ToList();

            return new ObservableCollection<ClusterInfo>(result);
        }

        private static async Task<bool> GetWorldListFromWebAsync(string url)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                try
                {
                    using (var response = await client.GetAsync(url))
                    {
                        using (var content = response.Content)
                        {
                            var fileString = await content.ReadAsStringAsync();
                            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", Settings.Default.WorldDataFileName),
                                fileString, Encoding.UTF8);
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(nameof(GetWorldListFromWebAsync), e);
                    return false;
                }
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
        Unknown
    }
}