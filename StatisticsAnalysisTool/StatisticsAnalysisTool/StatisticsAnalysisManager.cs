using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool
{
    public class StatisticsAnalysisManager
    {
        // Info Link -> https://github.com/broderickhyman/ao-bin-dumps
        // Models: https://github.com/broderickhyman/albiondata-models-dotNet

        public static List<Item> Items;
        public static int RefreshRate = Settings.Default.RefreshRate;
        public static int UpdateItemListByDays = Settings.Default.UpdateItemListByDays;

        private static bool GetItemListSourceUrlIfExist(ref string url)
        {
            if (string.IsNullOrEmpty(Settings.Default.CurrentItemListSourceUrl))
            {
                url = Settings.Default.DefaultItemListSourceUrl;
                if (string.IsNullOrEmpty(url))
                    return false;

                Settings.Default.CurrentItemListSourceUrl = Settings.Default.DefaultItemListSourceUrl;
                MessageBox.Show(LanguageController.Translation("DEFAULT_ITEMLIST_HAS_BEEN_LOADED"), LanguageController.Translation("NOTE"));
            }
            return true;
        }

        private static async Task<List<Item>> TryToGetItemListFromWeb(string url)
        {
            using (var wd = new WebDownload(30000))
            {
                try
                {
                    var itemsString = await wd.DownloadStringTaskAsync(url);
                    File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", itemsString, Encoding.UTF8);
                    return JsonConvert.DeserializeObject<List<Item>>(itemsString);
                }
                catch (Exception)
                {
                    try
                    {
                        var itemsString = await wd.DownloadStringTaskAsync(Settings.Default.DefaultItemListSourceUrl);
                        File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", itemsString, Encoding.UTF8);
                        return JsonConvert.DeserializeObject<List<Item>>(itemsString);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        private static List<Item> GetItemListFromLocal()
        {
            try
            {
                var localItemString = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");
                return JsonConvert.DeserializeObject<List<Item>>(localItemString);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> GetItemListFromJsonAsync()
        {
            var url = Settings.Default.CurrentItemListSourceUrl;
            if (!GetItemListSourceUrlIfExist(ref url))
                return false;
            
            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}"))
            {
                var fileDateTime = File.GetLastWriteTime($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");

                if (fileDateTime.AddDays(7) < DateTime.Now)
                {
                    Items = await TryToGetItemListFromWeb(url);
                    return (Items != null);
                }

                Items = GetItemListFromLocal();
                return (Items != null);
            }

            Items = await TryToGetItemListFromWeb(url);
            return (Items != null);
        }

        public static async Task<List<Item>> FindItemsAsync(string searchText)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return Items?.FindAll(s => (s.LocalizedName().ToLower().Contains(searchText.ToLower())));
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    return null;
                }
            });
        }

        public static FrequentlyValues.ItemTier GetItemTier(string uniqueName) => FrequentlyValues.ItemTiers.FirstOrDefault(x => x.Value == uniqueName.Split('_')[0]).Key;

        public static FrequentlyValues.ItemLevel GetItemLevel(string uniqueName)
        {
            if (!uniqueName.Contains("@"))
                return FrequentlyValues.ItemLevel.Level0;

            if(int.TryParse(uniqueName.Split('@')[1], out int number))
                return FrequentlyValues.ItemLevels.First(x => x.Value == number).Key;
            return FrequentlyValues.ItemLevel.Level0;
        }

        public static int GetQuality(FrequentlyValues.ItemQuality value) => FrequentlyValues.ItemQualities.FirstOrDefault(x => x.Key == value).Value;

        public static FrequentlyValues.ItemQuality GetQuality(int value) => FrequentlyValues.ItemQualities.FirstOrDefault(x => x.Value == value).Key;

        public static void AddLocalizedName(ref ItemData itemData, JObject parsedObject)
        {
            foreach (var language in Enum.GetValues(typeof(FrequentlyValues.GameLanguage)).Cast<FrequentlyValues.GameLanguage>())
            {
                var cultureCode = FrequentlyValues.GameLanguages.FirstOrDefault(x => x.Key == language).Value;

                if (parsedObject["localizedNames"][cultureCode] != null)
                    itemData.LocalizedNames.Add(new ItemData.KeyValueStruct() { Key = cultureCode, Value = parsedObject["localizedNames"][cultureCode].ToString() });
            }
        }

    }
}