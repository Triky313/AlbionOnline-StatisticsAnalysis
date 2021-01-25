using log4net;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Properties;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common
{
    using Models;
    using System;
    using System.Linq;
    using System.Text;

    public static class WorldController
    {
        public static ObservableCollection<MapData> MapData;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetUniqueNameNameOrDefault(string index)
        {
            if (MapData.Any(x => x.Index == index))
            {
                var name = MapData?.FirstOrDefault(x => x.Index == index)?.UniqueName ?? index;
                var splitName = name.Split(new [] { "@" }, StringSplitOptions.None);

                if (splitName.Length > 0 && name.ToLower().Contains('@'))
                {
                    return GetMapNameByMapType(GetMapType(splitName[0]));
                }

                return name;
            }
            
            return index;
        }

        private static string GetMapNameByMapType(MapType mapType)
        {
            switch (mapType)
            {
                case MapType.HellGate:
                    return "Hell gate";
                case MapType.RandomDungeon:
                    return "Dungeon";
                case MapType.CorruptedDungeon:
                    return "Corrupted Lair";
                case MapType.Island:
                    return "Island";
                default:
                    return "Unknown";
            }
        }

        public static MapType GetMapType(string index)
        {
            if (index.ToUpper().Contains("@HELLCLUSTER"))
            {
                return MapType.HellGate;
            }

            if (index.ToUpper().Contains("@RANDOMDUNGEON"))
            {
                return MapType.RandomDungeon;
            }

            if (index.ToUpper().Contains("@CORRUPTEDDUNGEON"))
            {
                return MapType.CorruptedDungeon;
            }

            if (index.ToUpper().Contains("@ISLAND"))
            {
                return MapType.Island;
            }

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
                    if (await GetWorldListFromWebAsync(url))
                    {
                        MapData = GetWorldDataFromLocal();
                    }
                    return (MapData?.Count > 0);
                }

                MapData = GetWorldDataFromLocal();
                return (MapData?.Count > 0);
            }

            if (await GetWorldListFromWebAsync(url))
            {
                MapData = GetWorldDataFromLocal();
            }
            return (MapData?.Count > 0);
        }

        #region Helper methods

        private static ObservableCollection<MapData> GetWorldDataFromLocal()
        {
            try
            {
                var localItemString = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.WorldDataFileName}", Encoding.UTF8);
                return ConvertItemJsonObjectToMapData(JsonConvert.DeserializeObject<ObservableCollection<WorldJsonObject>>(localItemString));
            }
            catch
            {
                return new ObservableCollection<MapData>();
            }
        }

        private static ObservableCollection<MapData> ConvertItemJsonObjectToMapData(ObservableCollection<WorldJsonObject> worldJsonObject)
        {
            var result = worldJsonObject.Select(item => new MapData()
            {
                Index = item.Index,
                UniqueName = item.UniqueName
            }).ToList();

            return new ObservableCollection<MapData>(result);
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
                            File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.WorldDataFileName}", fileString, Encoding.UTF8);
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

    public enum MapType {
        RandomDungeon,
        HellGate,
        CorruptedDungeon,
        Island,
        Unknown
    }
}