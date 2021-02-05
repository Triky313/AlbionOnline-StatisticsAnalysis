using log4net;
using Newtonsoft.Json;
using PcapDotNet.Base;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common
{
    public class LootChestController
    {
        public static IEnumerable<LootChest> LootChestData;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<bool> GetDataListFromJsonAsync()
        {
            var url = Settings.Default.WorldDataSourceUrl;
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.WorldDataFileName}";

            if (string.IsNullOrEmpty(url))
            {
                Log.Warn($"{nameof(GetDataListFromJsonAsync)}: No LootChestDataSourceUrl found.");
                return false;
            }

            if (File.Exists(localFilePath))
            {
                var fileDateTime = File.GetLastWriteTime(localFilePath);

                if (fileDateTime.AddDays(Settings.Default.UpdateWorldDataByDays) < DateTime.Now)
                {
                    if (await GetLootChestListFromWebAsync(url))
                    {
                        LootChestData = GetLootChestDataFromLocal();
                    }
                    return LootChestData != null && !LootChestData.IsNullOrEmpty();
                }

                LootChestData = GetLootChestDataFromLocal();
                return LootChestData != null && !LootChestData.IsNullOrEmpty();
            }

            if (await GetLootChestListFromWebAsync(url))
            {
                LootChestData = GetLootChestDataFromLocal();
            }
            return LootChestData != null && !LootChestData.IsNullOrEmpty();
        }

        #region Helper methods

        private static IEnumerable<LootChest> GetLootChestDataFromLocal()
        {
            try
            {
                var localItemString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", Settings.Default.LootChestDataFileName), Encoding.UTF8);
                return JsonConvert.DeserializeObject<LootChests>(localItemString).LootChest;
            }
            catch
            {
                return new List<LootChest>();
            }
        }

        private static async Task<bool> GetLootChestListFromWebAsync(string url)
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
                            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", Settings.Default.LootChestDataFileName), fileString, Encoding.UTF8);
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(nameof(GetLootChestListFromWebAsync), e);
                    return false;
                }
            }
        }

        #endregion
    }
}